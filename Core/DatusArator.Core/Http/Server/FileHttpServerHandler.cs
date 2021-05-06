using DatusArator.Core.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace DatusArator.Core.Http.Server {
  public class FileHttpServerHandler : IHttpServerHandler {
    private static readonly string[] fIndexFiles = {
        "index.html",
        "index.htm",
        "default.html",
        "default.htm"
    };

    private static readonly IDictionary<string, string> fMimeTypeMappings = 
      new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase) {
        #region extension to MIME type list
        {".asf", "video/x-ms-asf"},
        {".asx", "video/x-ms-asf"},
        {".avi", "video/x-msvideo"},
        {".bin", "application/octet-stream"},
        {".cco", "application/x-cocoa"},
        {".crt", "application/x-x509-ca-cert"},
        {".css", "text/css"},
        {".deb", "application/octet-stream"},
        {".der", "application/x-x509-ca-cert"},
        {".dll", "application/octet-stream"},
        {".dmg", "application/octet-stream"},
        {".ear", "application/java-archive"},
        {".eot", "application/octet-stream"},
        {".exe", "application/octet-stream"},
        {".flv", "video/x-flv"},
        {".gif", "image/gif"},
        {".hqx", "application/mac-binhex40"},
        {".htc", "text/x-component"},
        {".htm", "text/html"},
        {".html", "text/html"},
        {".ico", "image/x-icon"},
        {".img", "application/octet-stream"},
        {".iso", "application/octet-stream"},
        {".jar", "application/java-archive"},
        {".jardiff", "application/x-java-archive-diff"},
        {".jng", "image/x-jng"},
        {".jnlp", "application/x-java-jnlp-file"},
        {".jpeg", "image/jpeg"},
        {".jpg", "image/jpeg"},
        {".js", "application/x-javascript"},
        {".mml", "text/mathml"},
        {".mng", "video/x-mng"},
        {".mov", "video/quicktime"},
        {".mp3", "audio/mpeg"},
        {".mpeg", "video/mpeg"},
        {".mpg", "video/mpeg"},
        {".msi", "application/octet-stream"},
        {".msm", "application/octet-stream"},
        {".msp", "application/octet-stream"},
        {".pdb", "application/x-pilot"},
        {".pdf", "application/pdf"},
        {".pem", "application/x-x509-ca-cert"},
        {".pl", "application/x-perl"},
        {".pm", "application/x-perl"},
        {".png", "image/png"},
        {".prc", "application/x-pilot"},
        {".ra", "audio/x-realaudio"},
        {".rar", "application/x-rar-compressed"},
        {".rpm", "application/x-redhat-package-manager"},
        {".rss", "text/xml"},
        {".run", "application/x-makeself"},
        {".sea", "application/x-sea"},
        {".shtml", "text/html"},
        {".sit", "application/x-stuffit"},
        {".swf", "application/x-shockwave-flash"},
        {".tcl", "application/x-tcl"},
        {".tk", "application/x-tcl"},
        {".txt", "text/plain"},
        {".war", "application/java-archive"},
        {".wbmp", "image/vnd.wap.wbmp"},
        {".wmv", "video/x-ms-wmv"},
        {".xml", "text/xml"},
        {".xpi", "application/x-xpinstall"},
        {".zip", "application/zip"},
        #endregion
    };

    private readonly string fRootDirectory;

    public FileHttpServerHandler(string path) {
      fRootDirectory = path;
    }

    public bool Process(string filename, HttpListenerContext context) {
      if (string.IsNullOrEmpty(filename) || filename.EndsWith(@"/")) {
        foreach (string indexFile in fIndexFiles) {
          var check = StringUtils.Concat(filename, indexFile, "");
          if (File.Exists(Path.Combine(fRootDirectory, check))) {
            filename = check;
            break;
          }
        }
      }

      if (string.IsNullOrEmpty(filename) || filename.EndsWith(@"/")) {
        context.Response.StatusCode = (int)HttpStatusCode.NotFound;
        return true;
      }

      filename = Path.Combine(fRootDirectory, filename);

      if (File.Exists(filename)) {
        try {
          Stream input = new FileStream(filename, FileMode.Open);

          //Adding permanent http response headers
          context.Response.StatusCode = (int)HttpStatusCode.OK;
          context.Response.ContentType = fMimeTypeMappings.TryGetValue(Path.GetExtension(filename), out string mime) ? mime : "application/octet-stream";
          context.Response.ContentLength64 = input.Length;
          context.Response.AddHeader("Date", DateTime.Now.ToString("r"));
          context.Response.AddHeader("Last-Modified", System.IO.File.GetLastWriteTime(filename).ToString("r"));

          AddCrossOriginHeaders(context.Response);

          if (context.Request.HttpMethod != "HEAD") {
            byte[] buffer = new byte[1024 * 16];
            int nbytes;
            while ((nbytes = input.Read(buffer, 0, buffer.Length)) > 0)
              context.Response.OutputStream.Write(buffer, 0, nbytes);
          }
          input.Close();

          context.Response.OutputStream.Flush();
        } catch (Exception) {
          context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        }
      } else {
        Process(filename + @"/", context);
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
