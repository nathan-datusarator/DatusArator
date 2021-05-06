using DatusArator.Core.Json;
using System.Collections.Generic;
using System.Net;
using System.Text;

// This is currently set up to only take a single inbound connection. 
// If there is something in the queue, that will be sent immediately
// If not, it will wait for the next SendCommand

namespace DatusArator.Core.Http.Server {
  public class LongPollerHandler : IHttpServerHandler {
    private readonly Queue<JsonWrapper> fQueue = new Queue<JsonWrapper>();

    private HttpListenerContext fCurrentContext;
    public bool Process(string command, HttpListenerContext context) {
      fCurrentContext = context;

      var response = context.Response;
      response.StatusCode = (int)HttpStatusCode.OK;

      response.ContentType = @"application/json";
      SimpleJsonCommandHandler.AddCrossOriginHeaders(response);

      CheckQueue();

      return false;
    }

    public void SendCommand(JsonWrapper command) {
      fQueue.Enqueue(command);
      CheckQueue();
    }

    public void CheckQueue() {
      if (fCurrentContext == null)
        return;

      if (fQueue.Count == 0)
        return;

      var context = fCurrentContext;
      fCurrentContext = null;

      var response = context.Response;

      byte[] buffer = Encoding.UTF8.GetBytes(fQueue.Dequeue().ToJsonString());
      response.ContentLength64 = buffer.Length;

      using (var outputStream = response.OutputStream) {
        outputStream.Write(buffer, 0, buffer.Length);
        outputStream.Flush();
      }

      response.OutputStream.Close();
    }
  }
}
