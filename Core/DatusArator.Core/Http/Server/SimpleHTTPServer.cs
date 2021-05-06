using DatusArator.Core.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

//  netsh http add urlacl url="http://+:4201/" user=everyone

namespace DatusArator.Core.Http.Server {
  // MIT License - Copyright (c) 2016 Can Güney Aksakalli
  public class SimpleHTTPServer : IDisposable {
    private readonly Thread fServerThread;
    private HttpListener fListener;

    private readonly Dictionary<string, IHttpServerHandler> fHandlers = new Dictionary<string, IHttpServerHandler>();
    public IEnumerable<string> Prefixes { get; private set; }

    public SimpleHTTPServer(string host = "localhost", int? port = null, bool ssl = false) {
      if (port == null) {
        //get an empty port
        TcpListener l = new TcpListener(IPAddress.Loopback, 0);
        l.Start();
        port = ((IPEndPoint)l.LocalEndpoint).Port;
        l.Stop();
      }

      Prefixes = new string[] { (ssl ? "https" : "http") + @"://" + host + ":" + port.Value + "/" };

      fServerThread = new Thread(this.Listen) {
        IsBackground = true
      };
      fServerThread.Start();
    }

    public SimpleHTTPServer(IEnumerable<string> prefixes) {
      this.Prefixes = prefixes;

      fServerThread = new Thread(this.Listen) {
        IsBackground = true
      };
      fServerThread.Start();
    }

    public void Dispose() {
      Dispose(true);
    }

    protected virtual void Dispose(bool disposing) {
      if (disposing) {
        fServerThread.Abort();
        fListener.Close();
      }
    }

    public void Stop() {
      fServerThread.Abort();     
      fListener.Close();
    }

    public void AddHandler(string root, IHttpServerHandler handler) {
      fHandlers[root] = handler;
    }

    private void Listen() {
      fListener = new HttpListener();
      foreach (var prefix in Prefixes)
        fListener.Prefixes.Add(prefix);
      fListener.Start();

      while (true) {
        HttpListenerContext context = fListener.GetContext();
        try {
          Process(context);
        } catch (Exception ex) {
          Console.WriteLine("Error: " + ex.Message);

          var data = new JsonWrapper();
          data.Put("error", ex.Message);

          SendErrorResponse(context.Response, (int)HttpStatusCode.InternalServerError, data.ToJsonString());
        }
      }
    }

    private void Process(HttpListenerContext context) {
      string command = context.Request.Url.AbsolutePath;
      if (string.IsNullOrEmpty(command))
        command = "";
      command = WebUtility.UrlDecode(command);

      //      Console.WriteLine("Command: " + command);

      command = command.Substring(1);

      var longest = -1;
      IHttpServerHandler handler = null;
      foreach (var entry in fHandlers) {
        if (command.StartsWith(entry.Key)) {
          if (entry.Key.Length > longest) {
            longest = entry.Key.Length;
            handler = entry.Value;
          }
        }
      }

      if (longest == -1) {
        if (fHandlers.ContainsKey(""))
          handler = fHandlers[""];
        else {
          SendErrorResponse(context.Response, (int)HttpStatusCode.BadRequest,
            new JsonWrapper().Put("error", "Unknown Command.  No Default Handler set up").ToJsonString());

          return;
        }
      } else if (longest > 0)
        command = command.Substring(longest);

      bool closeStream = true;
      if (handler != null)
        closeStream = handler.Process(command, context);
      else
        context.Response.StatusCode = (int)HttpStatusCode.NotFound;

      if (closeStream)
        context.Response.OutputStream.Close();
    }

    private void SendErrorResponse(HttpListenerResponse response, int errorCode, string data) {
      response.StatusCode = errorCode;
      response.ContentType = @"application/json";

      byte[] buffer = Encoding.UTF8.GetBytes(data);
      response.ContentLength64 = buffer.Length;

      using (var outputStream = response.OutputStream) {
        outputStream.Write(buffer, 0, buffer.Length);
        outputStream.Flush();
      }
    }

    public static JsonWrapper RequestToJsonWrapper(HttpListenerRequest request) {
      JsonWrapper message;
      if (request.HasEntityBody) {
        using (System.IO.StreamReader reader = new System.IO.StreamReader(request.InputStream, request.ContentEncoding)) {
          message = new JsonWrapper(reader.ReadToEnd());
        }
      } else {
        message = new JsonWrapper();
        foreach (var key in request.QueryString.AllKeys)
          message.Put(key, request.QueryString[key]);
      }

      return message;
    }

    public static JsonWrapper RequestToJsonWrapperEx(HttpListenerRequest request) {
      JsonWrapper result = new JsonWrapper();

      if (request.HasEntityBody) {
        using (System.IO.StreamReader reader = new System.IO.StreamReader(request.InputStream, request.ContentEncoding)) {
          result.CopyDataFrom(new JsonWrapper(reader.ReadToEnd()), "body");
        }
      }

      foreach (var key in request.QueryString.AllKeys)
        result.Put("query." + key, request.QueryString[key]);

      return result;
    }
  }
}
