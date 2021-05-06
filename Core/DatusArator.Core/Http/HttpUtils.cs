using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

namespace DatusArator.Core.Http {
  public static class HttpUtils {
    public static string RemoveComments(string html) {
      do {
        var index = html.IndexOf(@"<!--");
        if (index >= 0) {
          var index2 = html.IndexOf(@"-->");
          html = (index > 0) ? html.Substring(0, index) : "" + html.Substring(index2 + 3).Trim();
        } else {
          break;
        }
      } while (true);

      return html;
    }

    #region Internet Explorer Cookies
    private static class NativeMethods {
      [DllImport("wininet.dll", SetLastError = true, CharSet = CharSet.Unicode)]
      public static extern bool InternetGetCookieEx(
                  string url,
                  string cookieName,
                  StringBuilder cookieData,
                  ref int size,
                  Int32 dwFlags,
                  IntPtr lpReserved);
    }

    private const Int32 INTERNET_COOKIE_HTTPONLY = 0x2000;

    private readonly static Dictionary<string, CookieContainer> fInternetExplorerCookies = new Dictionary<string, CookieContainer>();
    public static void ClearInternetExplorerCookies(string uri) {
      if (fInternetExplorerCookies.ContainsKey(uri))
        fInternetExplorerCookies.Remove(uri);
    }

    public static CookieContainer GetInternetExplorerCookies(string uri) {
      if (!fInternetExplorerCookies.ContainsKey(uri)) {
        CookieContainer cookies = null;
        // Determine the size of the cookie
        int datasize = 8192 * 16;
        StringBuilder cookieData = new StringBuilder(datasize);
        if (!NativeMethods.InternetGetCookieEx(uri, null, cookieData, ref datasize, INTERNET_COOKIE_HTTPONLY, IntPtr.Zero)) {
          if (datasize < 0)
            fInternetExplorerCookies[uri] = null;
          else {
            // Allocate stringbuilder large enough to hold the cookie
            cookieData = new StringBuilder(datasize);
            if (!NativeMethods.InternetGetCookieEx(
                uri.ToString(),
                null, cookieData,
                ref datasize,
                INTERNET_COOKIE_HTTPONLY,
                IntPtr.Zero))
              fInternetExplorerCookies[uri] = null;
          }
        }

        if (cookieData.Length > 0) {
          cookies = new CookieContainer();
          cookies.SetCookies(new Uri(uri), cookieData.ToString().Replace(';', ','));
        }

        fInternetExplorerCookies[uri] = cookies;
      }

      return fInternetExplorerCookies[uri];
    }
    #endregion

    public static void GotoURL(string url) {
      using (Process process = new Process()) {
        process.StartInfo.FileName = url;
        process.StartInfo.Verb = "open";
        process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;

        try {
          process.Start();
        } catch { }
      }
    }
  }
}
