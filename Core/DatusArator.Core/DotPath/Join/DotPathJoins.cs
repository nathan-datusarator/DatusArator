using System;
using System.Collections.Generic;

namespace DatusArator.Core.DotPath.Join {
  public static class DotPathJoins {
    private static Dictionary<string, DotPathDataSourceJoin> fJoins { get; } = new Dictionary<string, DotPathDataSourceJoin>();

    public static void Register(DotPathDataSourceJoin source) {
      var key = source.LinkName + ":" + source.ClassName + ":" + (source.LinkType?.ToString() ?? "");
      fJoins[key] = source;
    }

    public static void UnRegister(DotPathDataSourceJoin source) {
      var key = source.LinkName + ":" + source.ClassName + ":" + (source.LinkType?.ToString() ?? "");
      if (fJoins.ContainsKey(key))
        fJoins.Remove(key);
    }

    public static void Clear() {
      fJoins.Clear();
    }

    public static DotPathObject Get(DotPathObject source, string linkName) {
      foreach (var join in fJoins.Values) {
        if (join.Matches(source, linkName))
          return join.Get(source);
      }

      return null;
    }

    public static List<DotPathDataSourceJoin> Joins(Type parentType, string className) {
      var result = new List<DotPathDataSourceJoin>();
      foreach (var join in fJoins.Values) {
        if (join.CanJoin(parentType, className))
          result.Add(join);
      }

      return result;
    }
  }
}
