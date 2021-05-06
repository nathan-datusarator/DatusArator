using System;
using System.Collections.Generic;
using System.Text;
using DatusArator.Core.Json;

namespace DatusArator.Core.Http {
  public class GetUriEventArgs : EventArgs {
    public string Method { get; set; }
    public string Url { get; set; }
    public List<string> Headers { get; set; }

    public JsonWrapper Error { get; set; }

    public string PostData { get; set; }

    public int ResponseCode { get; set; }
    public string Response { get; set; }

    public GetUriEventArgs(GetUriOptions options, string responseData, JsonWrapper error) {
      Method = options.MethodAsString;
      Url = options.Url;

      Headers = new List<string>(options.Headers);
      foreach (var header in options.HeadersEx)
        Headers.Add(header.Key + ": " + header.Value);

      if (options.PostContentType?.Contains("json") ?? false)
        PostData = Encoding.UTF8.GetString(options.PostData);

      ResponseCode = (int)options.ResponseCode;
      Response = responseData;

      Error = error;
    }

    public override string ToString() {
      var url = Url.Substring(Url.LastIndexOf("/"));
      if (url.IndexOf("?") > 0)
        url = url.Substring(0, url.IndexOf("?"));

      return Method + " " + url;
    }
  }
}
