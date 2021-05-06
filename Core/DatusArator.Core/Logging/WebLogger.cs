using DatusArator.Core.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Xml.Serialization;

namespace DatusArator.Core.Logging {
  public class WebLogEntry {
    public HttpMethod Method { get; set; }
    public string Url { get; set; }
    public string Params { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string ContentType { get; set; }
    public int PostDataLength { get; set; }
  }

  public class WebLogger : List<WebLogEntry> {
    public static void LogWebError(Exception _) {
      // Do nothing for now
    }

    public static WebLogEntry LogWebCall(string requestUrl) {
      return LogWebCall(HttpMethod.Get, requestUrl, null, null);
    }

    public static WebLogEntry LogWebCall(HttpMethod httpMethod, string requestUrl, byte[] postData, string contentType) {
      var logEntry = new WebLogEntry {
        Method = httpMethod,
        Url = requestUrl,
        StartTime = DateTime.Now,
        ContentType = contentType,
        PostDataLength = postData?.Length ?? 0
      };

      lock (WebLogger.Instance) {
        WebLogger.Instance.Add(logEntry);
      }

      return logEntry;
    }

    private string logDir = FileUtils.FileRoot + @"weblogs\";
    [XmlIgnore]
    [JsonIgnore]
    public string LogDir {
      get { return logDir; }
      set { logDir = FileUtils.AddSlash(value); }
    }

    private static WebLogger instance;
    [XmlIgnore]
    [JsonIgnore]
    public static WebLogger Instance {
      get {
        if (instance == null)
          instance = new WebLogger();
        return instance; }
    }

    public void Save() {
      var filename = string.Format("weblog_{0:yyyymmdd_hhmmss}.json", DateTime.Now);

      Directory.CreateDirectory(LogDir);

      string value = JsonConvert.SerializeObject(this);
      FileUtils.StringToFile(value, LogDir + filename);
    }
  }
}
