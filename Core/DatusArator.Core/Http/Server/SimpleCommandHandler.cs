using DatusArator.Core.Json;
using System;
using System.Net;
using System.Text;

namespace DatusArator.Core.Http.Server {
  public class SimpleCommandHandler : IHttpServerHandler {
    private readonly Func<string, HttpListenerRequest, object> fOnGetContext;
    private readonly string fContentType;

    public SimpleCommandHandler(Func<string, HttpListenerRequest, object> callback, string contentType = @"text/plain") {
      this.fOnGetContext = callback;
      this.fContentType = contentType;
    }

    public bool Process(string command, HttpListenerContext context) {
      string stringResult = null;

      object result;
      if (context.Request.HttpMethod.Equals("OPTIONS", StringComparison.CurrentCultureIgnoreCase)) {
        result = new JsonWrapper().Put("code", 200); 
      } else {
        result = fOnGetContext?.Invoke(command, context.Request);
      }

      int statusCode = (int)HttpStatusCode.OK;
      string contentType = fContentType;

      if (result is JsonWrapper) {
        stringResult = (result as JsonWrapper).ToJsonString();
        statusCode = (result as JsonWrapper).GetAsInt("code", statusCode).Value;
        contentType = @"application/json";
      } else if (result is string) {
        stringResult = result as string;
      } else {
        stringResult = JsonUtils.ObjectToJson(result);
        contentType = @"application/json";
      }

      if (string.IsNullOrEmpty(stringResult)) {
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
      } else {
        var response = context.Response;
        response.StatusCode = statusCode;

        response.ContentType = contentType;
        AddCrossOriginHeaders(response);

        byte[] buffer = Encoding.UTF8.GetBytes(stringResult);
        response.ContentLength64 = buffer.Length;

        using (var outputStream = response.OutputStream) {
          outputStream.Write(buffer, 0, buffer.Length);
          outputStream.Flush();
        }
      }

      return true;
    }

    public static void AddCrossOriginHeaders(HttpListenerResponse response) {
      response.Headers.Add("Access-Control-Allow-Origin: *");
      response.Headers.Add("Access-Control-Allow-Headers: Content-Type");
      response.Headers.Add("Access-Control-Allow-Methods: GET,POST");
      response.Headers.Add("Access-Control-Allow-Credentials: true");
    }
  }
}
