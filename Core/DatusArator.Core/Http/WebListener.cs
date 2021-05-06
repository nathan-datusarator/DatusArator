using DatusArator.Core.Json;
using System;
using System.Net;
using System.Text;

// Remember 
// netsh http add urlacl url="http://+:4200/" user=everyone

namespace DatusArator.Core.Http {
  public class WebListener : IDisposable {
    private HttpListener fListener = new HttpListener();

    public string Protocol;
    public string Host;
    public int Port;

    public Func<HttpListenerRequest, string> OnGetContext { get; set; }

    public WebListener(int port, string host = "localhost", bool ssl = false) {
      this.Port = port;
      this.Host = host;
      this.Protocol = ssl ? "https" : "http";
    }

    public void Dispose() {
      Dispose(true);
    }

    protected virtual void Dispose(bool disposing) {
      if (disposing) {
        if (fListener != null)
          fListener.Close();
      }
    }

    public WebListener Start() {
      fListener = new HttpListener();
      fListener.Prefixes.Add(Protocol + @"://" + Host + ":" + Port + @"/");
      fListener.Start();
      fListener.BeginGetContext(new AsyncCallback(GetContextCallback), null);

      return this;
    }

    public void Stop() {
      fListener.Stop();
      fListener.Close();
      fListener = null;
    }

    public void GetContextCallback(IAsyncResult result) {
      if (fListener == null) return;

      HttpListenerContext context = fListener.EndGetContext(result);
      HttpListenerRequest request = context.Request;
      HttpListenerResponse response = context.Response;

      var responseString = OnGetContext?.Invoke(request);
      if (responseString == null) {
        responseString = new JsonWrapper().Put("code", 200).ToJsonString();
      }

      byte[] buffer = Encoding.UTF8.GetBytes(responseString);
      response.ContentLength64 = buffer.Length;

      using (System.IO.Stream outputStream = response.OutputStream) {
        outputStream.Write(buffer, 0, buffer.Length);
      }

      fListener.BeginGetContext(new AsyncCallback(GetContextCallback), null);
    }
  }
}
