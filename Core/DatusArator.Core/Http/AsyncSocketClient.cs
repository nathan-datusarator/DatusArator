using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.ComponentModel;
using DatusArator.Core.Json;

namespace DatusArator.Core.Http {
  // State object for receiving data from remote device.
  public class StateObject {
    // Client socket.
    public Socket workSocket = null;
    // Size of receive buffer.
    public const int BufferSize = 4096;
    // Receive buffer.
    public byte[] buffer = new byte[BufferSize];
    // Received data string.
    public StringBuilder sb = new StringBuilder();
  }

  public class AsyncSocketClient : Component {
    // ManualResetEvent instances signal completion.
    public ManualResetEvent ConnectDone { get; } = new ManualResetEvent(false);
    public ManualResetEvent SendDone { get; } = new ManualResetEvent(false);
    public ManualResetEvent ReceiveDone { get; } = new ManualResetEvent(false);

    // The response from the remote device.
    public string LastResponse { get; set; } = string.Empty;

    public string HostName { get; private set; }
    public int Port { get; private set; }

    public Action<AsyncSocketClient, string> OnConnect { get; set; }
    public Action<AsyncSocketClient, string> OnReceive { get; set; }
    public Action<AsyncSocketClient, int> OnSend { get; set; }

    private readonly Socket Client;

    public JsonWrapper Context { get; } = new JsonWrapper();

    public AsyncSocketClient(string hostName, int port, bool autoConnect) {
      HostName = hostName;
      Port = port;

      Disposed += HandleDisposed;

      Client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

      if (autoConnect)
        Connect();
    }

    private void HandleDisposed(object sender, EventArgs e) {
      ShutDown();

      ConnectDone.Dispose();
      SendDone.Dispose();
      ReceiveDone.Dispose();
      Client.Dispose();
    }

    public void Connect() {
      ConnectDone.Reset();

      // Establish the remote endpoint for the socket.
      // The name of the 
      // remote device is "host.contoso.com".
      IPHostEntry ipHostInfo = Dns.GetHostEntry(HostName);
      IPAddress ipAddress = PickHost(ipHostInfo);
      IPEndPoint remoteEP = new IPEndPoint(ipAddress, Port);

      // Connect to the remote endpoint.
      Client.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), Client);
      ConnectDone.WaitOne();
    }

    private IPAddress PickHost(IPHostEntry ipHostInfo) {
      foreach (var host in ipHostInfo.AddressList)
        if (host.AddressFamily == AddressFamily.InterNetwork)
          return host;

      return ipHostInfo.AddressList[0];
    }

    public void ShutDown() {
      if (Client.Connected) {
        try {
          Client.Shutdown(SocketShutdown.Both);
          Client.Close();
        } catch (Exception ex) {
          Console.WriteLine("Error Shutting down Client: " + ex.ToString());
        }
      }
    }

    private void ConnectCallback(IAsyncResult ar) {
      try {
        // Retrieve the socket from the state object.
        Socket client = (Socket)ar.AsyncState;

        // Complete the connection.
        client.EndConnect(ar);

        OnConnect?.Invoke(this, client.RemoteEndPoint.ToString());

        ConnectDone.Set();
      } catch (Exception e) {
        Console.WriteLine(e.ToString());
      }
    }

    public void Receive() {
      ReceiveDone.Reset();

      try {
        // Create the state object.
        StateObject state = new StateObject {
          workSocket = Client
        };

        // Begin receiving the data from the remote device.
        Client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
      } catch (Exception e) {
        Console.WriteLine(e.ToString());
      }
    }

    private void ReceiveCallback(IAsyncResult ar) {
      try {
        // Retrieve the state object and the client socket 
        // from the asynchronous state object.
        StateObject state = (StateObject)ar.AsyncState;
        Socket client = state.workSocket;

        // Read data from the remote device.
        int bytesRead = client.EndReceive(ar);

        if (bytesRead > 0) 
          state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

        if (bytesRead == StateObject.BufferSize) { 
          // Get the rest of the data.
          client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
        } else {
          // All the data has arrived; put it in response.
          LastResponse = state.sb.ToString().Trim();

          // Signal that all bytes have been received.
          ReceiveDone.Set();

          OnReceive?.Invoke(this, LastResponse);
        }
      } catch (Exception e) {
        Console.WriteLine(e.ToString());
      }
    }

    public void Send(string data) {
      SendDone.Reset();

      // Convert the string data to byte data using ASCII encoding.
      byte[] byteData = Encoding.ASCII.GetBytes(data);

      // Begin sending the data to the remote device.
      Client.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), Client);
    }

    private void SendCallback(IAsyncResult ar) {
      try {
        // Retrieve the socket from the state object.
        Socket client = (Socket)ar.AsyncState;

        // Complete sending the data to the remote device.
        int bytesSent = client.EndSend(ar);
        Console.WriteLine("Sent {0} bytes to server.", bytesSent);

        // Signal that all bytes have been sent.
        SendDone.Set();

        OnSend?.Invoke(this, bytesSent);
      } catch (Exception e) {
        Console.WriteLine(e.ToString());
      }
    }
  }
}
