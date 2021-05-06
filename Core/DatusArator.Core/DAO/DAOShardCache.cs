using System;
using System.Collections.Generic;
using System.Linq;

using DatusArator.Core.ObjectStore;
using DatusArator.Core.Util;

namespace DatusArator.Core.DAO {
  public class DAOShardCache<T> where T : BaseDAO, new() {
    private readonly BaseStorage fStorage;

    public Action<T> AfterLoad { get; set; }
    public Func<T, bool> BeforeSave { get; set; }
    public Action<T> AfterSave { get; set; }

    public DAOShardCache(BaseStorage storage) {
      fStorage = storage;
    }

    private Dictionary<string, bool> fAllLoaded { get; } = new Dictionary<string, bool>();
    private readonly Dictionary<string, Dictionary<string, T>> fData = new Dictionary<string, Dictionary<string, T>>();

    private Dictionary<string, T> GetShardData(string[] shardKeys) {
      var key = DAOUtils.GetBucket<T>(shardKeys);
      return GetShardData(key);
    }

    private Dictionary<string, T> GetShardData(string bucket) {
      if (!fData.ContainsKey(bucket))
        fData[bucket] = new Dictionary<string, T>();
      return fData[bucket];
    }

    public T Get(string shardKey, params string[] keys) {
      return Get(new string[] { shardKey }, keys);
    }

    public T Get(string[] shardKeys, params string[] keys) {
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
      var result = DAOUtils.LoadFromStorage<T>(fStorage.ObjectStore, keys);
      if (result != null) {
        AfterLoad?.Invoke(result);

        data[key] = result;
        return result;
      }

      return null;
    }

    public void Save(T item) {
      if (BeforeSave?.Invoke(item) ?? true)
        item.SaveToStorage(fStorage);
      AfterSave?.Invoke(item);

      Put(item);
    }

    public void Put(T item) {
      var (bucket, key) = DAOUtils.GetBucketAndKey(item);
      var data = GetShardData(bucket);

      data[key] = item;
    }

    public void Remove(T item) {
      var (bucket, key) = DAOUtils.GetBucketAndKey(item);
      var data = GetShardData(bucket);

      if (data.ContainsKey(key))
        data.Remove(key);
    }

    public void Clear(params string[] shardKeys) {
      if (shardKeys.Length < 1) {
        fAllLoaded.Clear();
        fData.Clear();
      }

      var bucket = DAOUtils.GetBucket<T>(shardKeys);
      fAllLoaded.Remove(bucket);
      fData.Remove(bucket);
    }

    public List<T> All(string shardKey) { return All(new string[] { shardKey }); }
    public List<T> All(string[] shardKeys) {
      CheckAllLoaded(shardKeys);
      return new List<T>(GetShardData(shardKeys).Values);
    }

    public List<string> AllKeys(string shardKey) { return AllKeys(new string[] { shardKey }); }
    public List<string> AllKeys(string[] shardKeys) {
      return DAOUtils.LoadAllKeys<T>(fStorage.ObjectStore, shardKeys);
    }

    public void CheckAllLoaded(string shardKey) { CheckAllLoaded(new string[] { shardKey }); }
    public void CheckAllLoaded(string[] shardKeys) {
      var key = DAOUtils.GetBucket<T>(shardKeys);

      if (!fAllLoaded.ContainsKey(key)) {
        LoadAll(shardKeys);
        fAllLoaded[key] = true;
      }
    }

    private void LoadAll(string[] shardKeys) {
      var bucket = DAOUtils.GetBucket<T>(shardKeys);
      fAllLoaded.Remove(bucket);
      fData.Remove(bucket);

      var all = DAOUtils.LoadAll<T>(fStorage.ObjectStore, shardKeys);
      foreach (var item in all) {
        AfterLoad?.Invoke(item);
        Put(item);
      }
    }

    public List<T> Records(string shardKey, List<string> ids) { return Records(new string[] { shardKey }, ids); }
    public List<T> Records(string[] shardKeys, List<string> ids) {
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
