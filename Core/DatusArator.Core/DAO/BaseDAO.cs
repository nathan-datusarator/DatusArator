using System;
using System.Collections.Generic;

using DatusArator.Core.DotPath;
using DatusArator.Core.Json;
using DatusArator.Core.ObjectStore;

namespace DatusArator.Core.DAO {
  public abstract class BaseDAO : DotPathObject {
    #region Load / Save
    // Must pass in the Shard values before passing in the Key Values if Sharded
    public static T LoadFromStorage<T>(IDatusAratorObjectStore storage, params string[] key) where T : BaseDAO, new() {
      return DAOUtils.LoadFromStorage<T>(storage, key);
    }

    public static List<T> LoadAll<T>(IDatusAratorObjectStore storage, params string[] shardKeys) where T : BaseDAO, new() {
      return DAOUtils.LoadAll<T>(storage, shardKeys);
    }

    public static List<string> LoadAllKeys<T>(IDatusAratorObjectStore storage, params string[] shardKeys) where T : BaseDAO, new() {
      return DAOUtils.LoadAllKeys<T>(storage, shardKeys);
    }

    public virtual void DeleteFromStorage(IDatusAratorObjectStore storage) {
      var (bucket, key) = DAOUtils.GetBucketAndKey(this);
      storage.Delete(bucket, key);
    }

    public virtual void SaveToStorage(BaseStorage storage) {
      DAOUtils.CheckKeys(this, storage);

      try {
        var (bucket, key) = DAOUtils.GetBucketAndKey(this);
        storage.ObjectStore.Put(bucket, key, JsonUtils.ObjectToJson(this));
      } catch (Exception ex) {
        Console.WriteLine("Error [" + ex.Message + "] " + JsonUtils.ObjectToJson(this));
      }
    }
    #endregion
  }
}
