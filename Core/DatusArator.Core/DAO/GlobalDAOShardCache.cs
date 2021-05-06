using System.Collections.Generic;
using System.Linq;

using DatusArator.Core.Util;

namespace DatusArator.Core.DAO {
  public static class GlobalDAOShardCache<T> where T : GlobalDAO, new() {
    private static Dictionary<string, bool> fAllLoaded { get; } = new Dictionary<string, bool>();
    private static readonly Dictionary<string, Dictionary<string, T>> fData = new Dictionary<string, Dictionary<string, T>>();

    private static Dictionary<string, T> GetShardData(string[] shardKeys) {
      var key = DAOUtils.GetBucket<T>(shardKeys);
      return GetShardData(key);
    }

    private static Dictionary<string, T> GetShardData(string bucket) {
      if (!fData.ContainsKey(bucket))
        fData[bucket] = new Dictionary<string, T>();
      return fData[bucket];
    }

    public static T Get(string shardKey, params string[] keys) {
      return Get(new string[] { shardKey }, keys);
    }

    public static T Get(string[] shardKeys, params string[] keys) {
      var key = "";
      foreach (var part in keys) {
        if (part == null)
          return null;

        key = StringUtils.Concat(key, part, ":");
      }

      var data = GetShardData(shardKeys);
      if (data.ContainsKey(key))
        return data[key];

      keys = shardKeys.Concat(keys).ToArray<string>();
      var result = GlobalDAO.LoadFromStorage<T>(keys);
      if (result != null) {
        data[key] = result;
        return result;
      }

      return null;
    }

    public static void Save(T item) {
      item.SaveToStorage();
      Put(item);
    }

    public static void Put(T item) {
      var (bucket, key) = DAOUtils.GetBucketAndKey(item);
      var data = GetShardData(bucket);
      
      data[key] = item;
    }

    public static void Remove(T item) {
      var (bucket, key) = DAOUtils.GetBucketAndKey(item);
      var data = GetShardData(bucket);

      if (data.ContainsKey(key))
        data.Remove(key);
    }

    public static void Clear(params string[] shardKeys) {
      if (shardKeys.Length < 1) {
        fAllLoaded.Clear();
        fData.Clear();
      }

      var bucket = DAOUtils.GetBucket<T>(shardKeys);
      fAllLoaded.Remove(bucket);
      fData.Remove(bucket);
    }
    
    public static List<T> All(string shardKey) { return All(new string[] { shardKey }); }
    public static List<T> All(string[] shardKeys) {
      CheckAllLoaded(shardKeys);
      return new List<T>(GetShardData(shardKeys).Values);
    }

    public static List<string> AllKeys(string shardKey) { return AllKeys(new string[] { shardKey }); }
    public static List<string> AllKeys(string[] shardKeys) {
      return GlobalDAO.LoadAllKeys<T>(shardKeys);
    }

    public static void CheckAllLoaded(string shardKey ) { CheckAllLoaded(new string[] { shardKey }); }
    public static void CheckAllLoaded(string[] shardKeys) {
      var key = DAOUtils.GetBucket<T>(shardKeys);

      if (!fAllLoaded.ContainsKey(key)) {
        LoadAll(shardKeys);
        fAllLoaded[key] = true;
      }
    }

    private static void LoadAll(string[] shardKeys) {
      var all = GlobalDAO.LoadAll<T>(shardKeys);

      fData.Clear();
      foreach (var item in all) {
        Put(item);
      }
    }

    public static List<T> Records(string shardKey, List<string> ids) { return Records(new string[] { shardKey }, ids); }
    public static List<T> Records(string[] shardKeys, List<string> ids) {
      var result = new List<T>();
      if (ids == null)
        return result;

      foreach (var id in ids) {
        var item = Get(shardKeys, id);
        if (!result.Contains(item))
          result.Add(item);
      }

      return result;
    }
  }
}
