using DatusArator.Core.ObjectStore;
using DatusArator.Core.Util;
using System;
using System.Collections.Generic;

namespace DatusArator.Core.DAO {
  public class DAOCache<T> where T : BaseDAO, new() {
    private readonly BaseStorage fStorage;

    public Action<T> AfterLoad { get; set; }
    public Func<T, bool> BeforeSave { get; set; }
    public Action<T> AfterSave { get; set; }

    public DAOCache(BaseStorage storage) {
      fStorage = storage;
    }

    public bool IsAllLoaded { get; private set; }
    private readonly Dictionary<string, T> fData = new Dictionary<string, T>();

    public bool Contains(params string[] key) {
      return (Get(key) != null);
    }

    public T Get(params string[] keys) {
      var key = "";
      foreach (var part in keys) {
        if (part == null)
          return null;

        key = StringUtils.Concat(key, part, ":");
      }

      if (fData.ContainsKey(key))
        return fData[key];

      var result = DAOUtils.LoadFromStorage<T>(fStorage.ObjectStore, keys);
      if (result != null) {
        AfterLoad?.Invoke(result);

        fData[key] = result;
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
      var (_, key) = DAOUtils.GetBucketAndKey(item);
      fData[key] = item;
    }

    public void Remove(T item) {
      var (_, key) = DAOUtils.GetBucketAndKey(item);
      if (fData.ContainsKey(key))
        fData.Remove(key);
    }

    public void Clear() {
      IsAllLoaded = false;
      fData.Clear();
    }

    public List<T> All {
      get {
        CheckAllLoaded();
        return new List<T>(fData.Values);
      }
    }

    public List<string> AllKeys {
      get {
        return DAOUtils.LoadAllKeys<T>(fStorage.ObjectStore, null);
      }
    }

    public void CheckAllLoaded() {
      if (!IsAllLoaded) {
        LoadAll();
        IsAllLoaded = true;
      }
    }

    private void LoadAll() {
      var all = DAOUtils.LoadAll<T>(fStorage.ObjectStore, null);

      fData.Clear();
      foreach (var item in all) {
        AfterLoad?.Invoke(item);
        Put(item);
      }
    }

    public List<T> Records(List<string> ids) {
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
