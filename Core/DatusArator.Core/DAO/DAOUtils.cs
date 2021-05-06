using System;
using System.Collections.Generic;
using DatusArator.Core.DotPath;
using DatusArator.Core.ObjectStore;
using DatusArator.Core.Util;

using static DatusArator.Core.Util.StringUtils;

namespace DatusArator.Core.DAO {
  public static class DAOUtils {
    public static T LoadFromStorage<T>(IDatusAratorObjectStore storage, params string[] key) where T : DotPathObject, new() {
      var empty = new T();
      var schema = empty.Schema;
      
      var bucket = schema.TableName;

      int keyPos = 0;
      foreach (var shard in schema.Shard) {
        if (key[keyPos] == null)
          return default;

        bucket = StringUtils.Concat(bucket, key[keyPos++], ":");
      }

      var objectStoreKey = "";
      for (int i = keyPos; i < key.Length; i++)
        objectStoreKey = StringUtils.Concat(objectStoreKey, key[keyPos], "-");

      var result = storage.Get<T>(bucket, objectStoreKey);
      return result;
    }

    public static List<T> LoadAll<T>(IDatusAratorObjectStore storage, params string[] shardKeys) where T : DotPathObject, new() {
      T empty = new T();

      int keyPos = 0;

      var bucket = empty.Schema.TableName;
      foreach (var shard in empty.Schema.Shard) {
        if (shardKeys[keyPos] == null)
          return new List<T>();

        bucket = StringUtils.Concat(bucket, shardKeys[keyPos++], ":");
      }

      var result = storage.Records<T>(bucket);
      return result;
    }

    public static string GetBucket<T>(params string[] shards) where T : DotPathObject, new() {
      return new T().Schema.TableName + ":" + StringUtils.Concat(shards, ":");
    }

    public static (string bucket, string key) GetBucketAndKey(DotPathObject entry) {
      var bucket = entry.Schema.TableName;
      foreach (var shard in entry.Schema.Shard)
        bucket = StringUtils.Concat(bucket, entry[shard].ToString(), ":");

      var key = "";
      foreach (var part in entry.Schema.KeyFields)
        key = StringUtils.Concat(key, entry[part].ToString(), ":");

      return (bucket, key);
    }

    public static List<string> LoadAllKeys<T>(IDatusAratorObjectStore storage, params string[] shardKeys) where T : DotPathObject, new() {
      T empty = new T();

      int keyPos = 0;

      var bucket = empty.Schema.TableName;
      foreach (var shard in empty.Schema.Shard) {
        if (shardKeys[keyPos] == null)
          return new List<string>();

        bucket = StringUtils.Concat(bucket, shardKeys[keyPos++], ":");
      }

      return storage.Ids(bucket);
    }


    public static void CheckKeys(DotPathObject dpo, BaseStorage storage) {
      CheckSequences(dpo, storage);
      CheckGuids(dpo);
    }

    public static void CheckGuids(DotPathObject dpo) {
      foreach (var field in dpo.Schema.Fields) {
        if (SameString(field.Type, "GUID")) {
          var value = dpo[field.Field];
          if ((value == null) || (value.ToString() == ""))
            dpo[field.Field] = Guid.NewGuid().ToString();
        }
      }
    }

    public static void CheckSequences(DotPathObject dpo, BaseStorage storage) {
      foreach (var field in dpo.Schema.Fields) {
        if (!string.IsNullOrEmpty(field.Sequence)) {
          var value = dpo[field.Field];
          if ((value == null) || (Convert.ToInt32(value) < 1))
            dpo[field.Field] = storage.GetNextSequenceValue(field.Sequence);
        }
      }
    }
  }
}
