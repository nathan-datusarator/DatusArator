using System;
using System.Collections.Generic;

namespace DatusArator.Core.Util {
  public static class HistogramSupport {
    public static int? HistogramIncrement(this Dictionary<String, int> data, String key, int amount = 1) {
      if (data == null)
        return null;

      if (data.ContainsKey(key))
        data[key] += amount;
      else
        data[key] = amount;

      return data[key];
    }

    public static int? HistogramDecrement(this Dictionary<String, int> data, String key, int amount = 1) {
      if (data == null)
        return null;

      if (data.ContainsKey(key))
        data[key] -= amount;
      else
        data[key] = -amount;

      return data[key];
    }
    
    public static int HistogramTotal(this Dictionary<String, int> data) {
      if (data == null)
        return 0;

      int result = 0;
      foreach (var item in data.Values)
        result += item;

      return result;
    }

    public static string HistogramMaxKey(this Dictionary<string, int> data) {
      string result = null;
      int max = -10000000;

      foreach (var item in data) {
        if (item.Value > max) {
          result = item.Key;
          max = item.Value;
        }
      }

      return result;
    }

    public static List<string> HistogramTopN(this Dictionary<string, int> data, int n = 3) {
      var result = new List<string>();
      var max = new List<int>();

      if (n < 1)
        return result;

      foreach (var item in data) {
        int index = IndexGreaterThan(max, item.Value);
        if (index > -1) {
          result.Insert(index, item.Key);
          max.Insert(index, item.Value);
        } else if (result.Count < n) {
          result.Add(item.Key);
          max.Add(item.Value);
        }

        while (result.Count > n)
          result.RemoveAt(result.Count - 1);
        while (max.Count > n)
          max.RemoveAt(max.Count - 1);
      }

      return result;
    }

    private static int IndexGreaterThan(List<int> max, int value) {
      for (int i = 0; i < max.Count; i++) {
        if (value > max[i])
          return i;
      }
      return -1;
    }
  }
}
