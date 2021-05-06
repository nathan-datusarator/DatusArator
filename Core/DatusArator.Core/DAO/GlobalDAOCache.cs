using DatusArator.Core.Util;
using System.Collections.Generic;

namespace DatusArator.Core.DAO {
  public static class GlobalDAOCache<T> where T : GlobalDAO, new() {
    public static bool IsAllLoaded { get; private set; }
    private static readonly Dictionary<string, T> fData = new Dictionary<string, T>();

    public static T Get(params string[] keys) {
      var key = "";
      foreach (var part in keys) {
        if (part == null)
          return null;

        key = StringUtils.Concat(key, part, ":");
      }

      if (fData.ContainsKey(key))
        return fData[key];

      var result = GlobalDAO.LoadFromStorage<T>(keys);
      if (result != null) {
        fData[key] = result;
        return result;
      }

      return null;
    }

    public static void Save(T item) {
      item.SaveToStorage();
      Put(item);
    }

    public static void Put(T item) {
      var (_, key) = DAOUtils.GetBucketAndKey(item);
      fData[key] = item;
    }

    public static void Remove(T item) {
      var (_, key) = DAOUtils.GetBucketAndKey(item);
      if (fData.ContainsKey(key))
        fData.Remove(key);
    }

    public static void Clear() {
      IsAllLoaded = false;
      fData.Clear();
    }

    public static List<T> All {
      get {
        CheckAllLoaded();
        return new List<T>(fData.Values);
      }
    }

    public static List<string> AllKeys {
      get {
        return GlobalDAO.LoadAllKeys<T>();
      }
    }

    public static void CheckAllLoaded() {
      if (!IsAllLoaded) {
        LoadAll();
        IsAllLoaded = true;
      }
    }

    private static void LoadAll() {
      var all = GlobalDAO.LoadAll<T>();

      fData.Clear();
      foreach (var item in all) {
        Put(item);
      }
    }

    public static List<T> Records(List<string> ids) {
      var result = new List<T>();
      if (ids == null)
        return result;

      foreach (var id in ids) {
        var item = Get(id);
        if (!result.Contains(item))
          result.Add(item);
      }

      return result;
    }
  }
}
