using System;
using System.Collections.Generic;
using System.Net;

namespace DatusArator.Core.Http {
  public class GetUriOptions {
    public HttpMethod Method { get; set; }
    public string Url { get { return Uri.ToString(); } set { Uri = new Uri(value); } }
    public Uri Uri { get; set; }
    public byte[] PostData { get; set; }
    public string PostContentType { get; set; }
    public List<string> Headers { get; } = new List<string>();
    public Dictionary<string, string> HeadersEx { get; } = new Dictionary<string, string>();

    public string Host { get; set; }
    public string Referer { get; set; }
    public string Accept { get; set; }
    public string Connection { get; set; }

    // @"Mozilla/5.0 (Windows NT 6.3; Trident/7.0;
    public string UserAgent { get; set; } = @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.84 Safari/537.36";

    public CookieContainer CookieContainer { get; set; }
    public bool? UseCrawlera { get; set; }

    public bool RetryOnFail { get; set; }

    public Action<int> OnFailTimer { get; set; }

    public HttpStatusCode ResponseCode { get; set; }

    public string MethodAsString {
      get {
        switch (Method) {
          case HttpMethod.Get:
            return "GET";
          case HttpMethod.Delete:
            return "DELETE";
          case HttpMethod.Post:
            return "POST";
          case HttpMethod.Options:
            return "OPTIONS";
          default:
            return "GET";
        }
      }
    }
  }
}
