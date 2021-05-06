using DatusArator.Core.Http;
using DatusArator.Core.Json;
using DatusArator.Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DatusArator.HTML.Turo {
  public class TuroService {
    public static List<TuroEntry> Query(string query) {
      var parts = query.Split('&').ToList();
      for (int j = parts.Count - 1; j >= 0; j--) {
        if (parts[j].StartsWith("itemsPerPage"))
          parts[j] = "itemsPerPage=2000";
        if (parts[j].StartsWith("minimumPrice"))
          parts.RemoveAt(j);
        if (parts[j].StartsWith("maximumPrice"))
          parts.RemoveAt(j);
      }
      query = StringUtils.Join((IList<string>)parts, "&");

      var Cookies = new CookieContainer();

      var options = new GetUriOptions();
      options.Method = HttpMethod.Get;
      options.Uri = new Uri(@"https://turo.com/search?" + query);
      options.RetryOnFail = true;

      options.CookieContainer = Cookies;

      WebUtils.GetUri(options);

      var resultMap = new Dictionary<int, TuroEntry>();

      var priceRange = new[] {
        new { Min = 1, Max = 30 },
        new { Min = 30, Max = 50 },
        new { Min = 50, Max = 80 },
        new { Min = 80, Max = 120 },
        new { Min = 120, Max = 250 },
      };

      foreach (var range in priceRange) {
        GeneralUtils.GlobalProgress?.UpdateProgress("Query Turo: $" + range.Min + " to $" + range.Max, 0);

        options = new GetUriOptions();
        options.Method = HttpMethod.Get;
        options.Referer = @"https://turo.com/search?" + query;
        options.Host = "turo.com";

        var url = @"https://turo.com/api/search?" + query;
        url = url + "&minimumPrice=" + range.Min;
        url = url + "&maximumPrice=" + range.Max;
        options.Uri = new Uri(url);

        options.Headers.Add(@"X-Requested-With: XMLHttpRequest");

        options.CookieContainer = Cookies;

        var response = WebUtils.GetUri(options);
        var parser = new JsonWrapper(response);

        var items = parser.GetArrayAsObjects("list");
        foreach (var item in items) {
          var entry = TuroEntry.Populate(item);
          resultMap[entry.Id] = entry;
        }

        // Try to not get caught by checking for too fast
        Thread.Sleep(1000);
      }

      GeneralUtils.GlobalProgress?.UpdateProgress("Get Estimated Values from Car Gurus", 0);

      int i = 0;
      foreach (var item in resultMap.Values) {
        GeneralUtils.GlobalProgress?.UpdateProgress("Looking up Est Value: " + (++i) + " of " + resultMap.Count, i, resultMap.Count);
        item.LookupEstValue();
      }

      return new List<TuroEntry>(resultMap.Values);
    }
  }
}
