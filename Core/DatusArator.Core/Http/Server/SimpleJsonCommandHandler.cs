using DatusArator.Core.Json;
using System;
using System.Net;
using System.Text;

namespace DatusArator.Core.Http.Server {
  public class SimpleJsonCommandHandler : IHttpServerHandler {
    private readonly Func<HttpListenerRequest, object> fOnGetContext;

    public SimpleJsonCommandHandler(Func<HttpListenerRequest, object> callback) {
      this.fOnGetContext = callback;
    }

    public bool Process(string command, HttpListenerContext context) {
      string stringResult = null;
      JsonWrapper jsonResult = null;

      var result = fOnGetContext?.Invoke(context.Request);
      if (result is JsonWrapper) {
        jsonResult = (result as JsonWrapper);
        stringResult = jsonResult.ToJsonString();
      } else if (result is string)
        stringResult = result as string;
      else {
        stringResult = JsonUtils.ObjectToJson(result);
        jsonResult = new JsonWrapper(stringResult);
      }

      if (string.IsNullOrEmpty(stringResult)) {
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
      } else {
        var response = context.Response;
        if (jsonResult?.HasValue("code") ?? false)
          response.StatusCode = jsonResult.GetAsInt("code", 200).Value;
        else
          response.StatusCode = (int)HttpStatusCode.OK;

        response.ContentType = @"application/json";
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
