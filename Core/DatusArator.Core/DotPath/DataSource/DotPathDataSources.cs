using System;
using System.Collections.Generic;

namespace DatusArator.Core.DotPath.DataSource {
  public static class DotPathDataSources {
    private static readonly Dictionary<string, IDotPathDataSource> fSources = new Dictionary<string, IDotPathDataSource>();

    public static void Register(string name, IDotPathDataSource source) {
      fSources[name.ToUpper()] = source;
      source.Key = name;
    }

    public static void UnRegister(string name) {
      if (fSources.ContainsKey(name.ToUpper()))
        fSources.Remove(name.ToUpper());
    }

    public static List<IDotPathDataSource> SourcesList(bool primary = false) {
      if (primary) {
        var result = new List<IDotPathDataSource>();
        foreach (var source in fSources.Values)
          if (source.IsPrimary)
            result.Add(source);

        return result;
      } else
      return new List<IDotPathDataSource>(fSources.Values);
    }

    // Don't call this TOO often.  You will have to reregister data sources
    public static void Clear() {
      fSources.Clear();
    }

    public static IDotPathDataSource Get(string name) {
      if (name == null) return null;

      var key = name?.ToUpper();
      if (fSources.ContainsKey(key))
        return fSources[key];
      return null;
    }

    public static IDotPathDataSource GetByName(string name) {
      if (name == null) return null;

      foreach (var source in fSources.Values)
        if (source.DisplayName.Equals(name, StringComparison.InvariantCultureIgnoreCase))
          return source;

      return null;
    }
  }
}
