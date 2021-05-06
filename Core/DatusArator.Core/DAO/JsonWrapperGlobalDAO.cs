using DatusArator.Core.Json;
using DatusArator.Core.ObjectStore;
using DatusArator.Core.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;

namespace DatusArator.Core.DAO {
  public abstract class JsonWrapperGlobalDAO : JsonWrapper {
    [JsonIgnore]
    protected abstract BaseStorage Storage { get; }

    public static T LoadFromStorage<T>(params string[] key) where T : JsonWrapperGlobalDAO, new() {
      var result = new T();

      int keyPos = 0;

      var bucket = result.Schema.TableName;
      foreach (var shard in result.Schema.Shard) {
        if (key[keyPos] == null)
          return default;

        bucket = StringUtils.Concat(bucket, key[keyPos++], ":");
      }

      var objectStoreKey = "";
      for (int i = keyPos; i < key.Length; i++)
        objectStoreKey = StringUtils.Concat(objectStoreKey, key[keyPos], "-");

      var json = result.Storage.ObjectStore.Get(bucket, objectStoreKey);
      if (json != null)
        result.ParseJson(json);
      else
        result = null;

      return result;
    }

    public static List<string> LoadAllKeys<T>(params string[] shardKeys) where T : JsonWrapperGlobalDAO, new() {
      return DAOUtils.LoadAllKeys<T>(new T().Storage.ObjectStore, shardKeys);
    }

    public static List<T> LoadAll<T>(params string[] key) where T : JsonWrapperGlobalDAO, new() {
      var empty = new T();

      int keyPos = 0;

      var bucket = empty.Schema.TableName;
      foreach (var shard in empty.Schema.Shard) {
        if (key[keyPos] == null)
          return new List<T>();

        bucket = StringUtils.Concat(bucket, key[keyPos++], ":");
      }

      List<T> result = new List<T>();
      var jsons = empty.Storage.ObjectStore.Records(bucket);
      foreach (var json in jsons.Values) {
        var value = new T();
        value.ParseJson(json);
        result.Add(value);
      }

      return result;
    }

    public virtual void DeleteFromStorage() {
      var (bucket, key) = GetBucketAndKey();
      Storage.ObjectStore.Delete(bucket, key);
    }

    public virtual (string bucket, string key) GetBucketAndKey() {
      return DAOUtils.GetBucketAndKey(this);
    }

    public virtual void SaveToStorage() {
      DAOUtils.CheckKeys(this, Storage);

      try {
        var (bucket, key) = GetBucketAndKey();
        Storage.ObjectStore.Put(bucket, key, this.ToJsonString());
      } catch (Exception ex) {
        Console.WriteLine("Error [" + ex.Message + "] " + JsonUtils.ObjectToJson(this));
      }
    }
  }
}
