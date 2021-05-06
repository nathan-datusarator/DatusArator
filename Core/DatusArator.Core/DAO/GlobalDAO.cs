using System;
using System.Collections.Generic;

using DatusArator.Core.DotPath;
using DatusArator.Core.Json;
using DatusArator.Core.ObjectStore;
using DatusArator.Core.Util;

using Newtonsoft.Json;

using static DatusArator.Core.Util.StringUtils;

namespace DatusArator.Core.DAO {
  public abstract class GlobalDAO : DotPathObject {
    [JsonIgnore]
    protected abstract BaseStorage Storage { get; }

    #region Load / Save
    // Must pass in the Shard values before passing in the Key Values if Sharded
    public static T LoadFromStorage<T>(params string[] key) where T : GlobalDAO, new() {
      return DAOUtils.LoadFromStorage<T>(new T().Storage.ObjectStore, key);
    }

    public static List<T> LoadAll<T>(params string[] shardKeys) where T : GlobalDAO, new() {
      return DAOUtils.LoadAll<T>(new T().Storage.ObjectStore, shardKeys);
    }

    public static List<string> LoadAllKeys<T>(params string[] shardKeys) where T : GlobalDAO, new() {
      return DAOUtils.LoadAllKeys<T>(new T().Storage.ObjectStore, shardKeys);
    }

    public virtual void DeleteFromStorage() {
      var (bucket, key) = DAOUtils.GetBucketAndKey(this);
      Storage.ObjectStore.Delete(bucket, key);
    }

    public virtual void SaveToStorage() {
      DAOUtils.CheckKeys(this, Storage);

      try {
        var (bucket, key) = DAOUtils.GetBucketAndKey(this);
        Storage.ObjectStore.Put(bucket, key, JsonUtils.ObjectToJson(this));
      } catch (Exception ex) {
        Console.WriteLine("Error [" + ex.Message + "] " + JsonUtils.ObjectToJson(this));
      }
    }
    #endregion
  }
}
