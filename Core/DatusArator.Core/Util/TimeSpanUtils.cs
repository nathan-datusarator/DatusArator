using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DatusArator.Core.Util {
  public static class TimeSpanUtils {
    public static string Format(TimeSpan ts) {
      StringBuilder result = new StringBuilder();

      if (ts.Days > 0) {
        result.Append(ts.Days);
        result.Append(" day");
        if (ts.Days > 1)
          result.Append("s");
      }

      if (ts.Hours > 0) {
        if (result.Length > 0)
          result.Append(" ");

        result.Append(ts.Hours);
        result.Append(" hr");
        if (ts.Hours > 1)
          result.Append("s");
      }

      if (ts.Minutes > 0) {
        if (result.Length > 0)
          result.Append(" ");

        result.Append(ts.Minutes);
        result.Append(" min");
        if (ts.Minutes > 1)
          result.Append("s");
      }

      if (ts.Seconds > 0) {
        if (result.Length > 0)
          result.Append(" ");

        result.Append(ts.Seconds);
        result.Append(" sec");
        if (ts.Seconds > 1)
          result.Append("s");
      }

      return result.ToString();
    }
  }
}
