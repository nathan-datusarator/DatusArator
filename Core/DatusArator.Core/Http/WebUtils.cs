using DatusArator.Core.Json;
using DatusArator.Core.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace DatusArator.Core.Http {
  public enum HttpMethod {
    Get,
    Post,
    Delete,
    Options
  }

  public static class WebUtils {
    public static event EventHandler<GetUriEventArgs> OnGetUri;

    public static bool UseCrawlera = false;
    private static string CRAWLERA_KEY = "";

    public static void InitCrawlera(string key) {
      CRAWLERA_KEY = key;
    }

    private static WebProxy fCrawleraProxy;
    private static WebProxy GetCrawleraProxy() {
      if (fCrawleraProxy == null) {
        fCrawleraProxy = new WebProxy("http://proxy.crawlera.com:8010") {
          Credentials = new NetworkCredential(CRAWLERA_KEY, "")
        };
      }

      return fCrawleraProxy;
    }

    private static void PrepForCrawlera(HttpWebRequest request, GetUriOptions options) {
      if (!(options.UseCrawlera ?? UseCrawlera))
        return;

      request.Proxy = GetCrawleraProxy();
      request.PreAuthenticate = true;

      request.Headers.Add("X-Crawlera-UA: pass");
    }

    // TODO Write this
    private static void CheckCrawleraHeaders(HttpWebResponse response, GetUriOptions options) {
      if (!(options.UseCrawlera ?? UseCrawlera))
        return;

      // BS Code to get rid of warning
      if (response == null)
        return;
    }

    public static string GetUri(Uri requestUrl, bool retryOnFail = false) {
      var options = new GetUriOptions() { Method = HttpMethod.Get, Uri = requestUrl, RetryOnFail = retryOnFail };
      return GetUri(options);
    }

    public static string GetUri(HttpMethod httpMethod, Uri requestUrl, byte[] postData, string contentType, bool retryOnfail = false) {
      var options = new GetUriOptions() { Method = httpMethod, Uri = requestUrl, PostData = postData, PostContentType = contentType, RetryOnFail = retryOnfail };
      return GetUri(options);
    }

    public static string GetUri(GetUriOptions options) {
      var request = (HttpWebRequest)WebRequest.Create(options.Uri);
      request.Method = options.MethodAsString;
      request.UserAgent = options.UserAgent;
      request.CookieContainer = options.CookieContainer;

      if (!string.IsNullOrEmpty(options.Referer))
        request.Referer = options.Referer;
      if (!string.IsNullOrEmpty(options.Host))
        request.Host = options.Host;
      if (!string.IsNullOrEmpty(options.Accept))
        request.Accept = options.Accept;
      if (!string.IsNullOrEmpty(options.Connection)) {
        if (options.Connection.Equals("keep-alive", StringComparison.InvariantCultureIgnoreCase))
          request.KeepAlive = true;
        else
          request.Connection = options.Connection;
      }

      PrepForCrawlera(request, options);

      foreach (var header in options.Headers)
        request.Headers.Add(header);

      foreach (var header in options.HeadersEx)
        request.Headers[header.Key] = header.Value;

      try {
        if (options.PostData != null) {
          request.ContentLength = options.PostData.Length;
          request.ContentType = options.PostContentType;
          using (Stream dataStream = request.GetRequestStream()) {
            dataStream.Write(options.PostData, 0, options.PostData.Length);
          }
        }

        var responseData = string.Empty;
        using (var response = (HttpWebResponse)request.GetResponse()) {
          if (response.ContentEncoding == "gzip") {
            responseData = Encoding.UTF8.GetString(StringUtils.FromGzipData(response.GetResponseStream()));
          } else {
            using (var streamReader = new StreamReader(response.GetResponseStream())) {
              responseData = streamReader.ReadToEnd();
            }
          }

          CheckCrawleraHeaders(response, options);

          options.ResponseCode = response.StatusCode;
        }

        if (OnGetUri != null) {
          var args = new GetUriEventArgs(options, responseData, null);
          OnGetUri(options, args);
        }

        return responseData;
      } catch (Exception ex) {
        JsonWrapper result = null;

        if (ex.Message.Contains("Unauthorized"))
          result = new JsonWrapper().Put("code", 401).Put("error", ex.Message);
        else if (ex.Message.Contains("Forbidden"))
          result = new JsonWrapper().Put("code", 500).Put("error", ex.Message);
        else if (options.RetryOnFail) {
          GeneralUtils.RaiseGlobalError("Internet Down: Waiting 10 sec", true);

          int tenths = 10 * 10;

          options.OnFailTimer?.Invoke(tenths);
          for (int i = 0; i < tenths; i++) {
            Thread.Sleep(100);
            options.OnFailTimer?.Invoke(tenths - i - 1);
          }
          options.OnFailTimer?.Invoke(0);

          return GetUri(options);
        }

        if (result == null)
          result = new JsonWrapper().Put("code", 500).Put("error", ex.Message);

        if (OnGetUri != null) {
          var args = new GetUriEventArgs(options, null, result);
          OnGetUri(options, args);
        }

        return result.ToJsonString();
      }
    }

    public static string GetIpAddress() {
      IPAddress[] ipv4Addresses = Array.FindAll(Dns.GetHostEntry(string.Empty).AddressList, a => a.AddressFamily == AddressFamily.InterNetwork);

      return ipv4Addresses.Length > 0 ? ipv4Addresses[ipv4Addresses.Length - 1].ToString() : null;
    }

    public static bool PingGoogle() {
      try {
        using (Ping myPing = new Ping()) {
          String host = "8.8.8.8";
          byte[] buffer = new byte[32];
          int timeout = 1000;
          PingOptions pingOptions = new PingOptions();
          PingReply reply = myPing.Send(host, timeout, buffer, pingOptions);
          
          return (reply.Status == IPStatus.Success);
        }
      } catch (Exception) {
        return false;
      }
    }

    public static bool SameContentUrl(string url1, string url2, bool ignoreParams = false) {
      if (string.IsNullOrEmpty(url1)) return string.IsNullOrEmpty(url2);
      if (string.IsNullOrEmpty(url2)) return false;

      url1 = CleanSameContentUrl(url1, ignoreParams);
      url2 = CleanSameContentUrl(url2, ignoreParams);

      return url1.Equals(url2, StringComparison.InvariantCultureIgnoreCase);
    }

    private static string CleanSameContentUrl(string url, bool ignoreParams) {
      if (url.StartsWith(@"https://")) url = url.Substring("https://".Length);
      if (url.StartsWith(@"http://")) url = url.Substring("http://".Length);

      var index = url.IndexOf('/');
      url = url.Substring(index + 1);

      if (ignoreParams) {
        index = url.IndexOf('?');
        if (index > 0)
          url = url.Substring(0, index);
      }

      return url;
    }

    public static string FindFirstUrl(string text) {
      if (text == null)
        return null;

      var linkIndex = text.IndexOf(@"http://");
      if (linkIndex == -1) linkIndex = text.IndexOf(@"https://");
      if (linkIndex == -1) linkIndex = text.IndexOf(@"ftp://");
      if (linkIndex == -1) linkIndex = text.IndexOf(@"ftps://");

      if (linkIndex >= 0) {
        var result = text.Substring(linkIndex);
        if (result.IndexOf(" ") > 0)
          result = result.Substring(0, result.IndexOf(" ")).Trim();

        return result;
      }

      return null;
    }
  }
}
