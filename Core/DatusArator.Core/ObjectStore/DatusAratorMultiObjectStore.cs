using System;
using System.Collections.Generic;

namespace DatusArator.Core.ObjectStore {
  public class DatusAratorMultiObjectStore : IDatusAratorObjectStore {
    public IDatusAratorObjectStore Build(string config) {
      if (config.StartsWith("Multi^", StringComparison.InvariantCultureIgnoreCase))
        return new DatusAratorFileObjectStore(config.Substring(6));
      else
        return null;
    }

    public DatusAratorMultiObjectStore(string config) {
      if (config == null)
        return;

      var parts = config.Split('^');
      foreach (var part in parts) {
        var subparts = part.Split('~');
        var store = DatusAratorObjectStoreFactory.BuildObjectStore(subparts[0]);
        if (subparts[1].StartsWith("R"))
          fReadStores.Add(store);
        fAllStores.Add(store);
      }
    }

    private readonly List<IDatusAratorObjectStore> fReadStores = new List<IDatusAratorObjectStore>();
    private readonly List<IDatusAratorObjectStore> fAllStores = new List<IDatusAratorObjectStore>();

    private readonly Random fRandom = new Random();
    private IDatusAratorObjectStore ReadStore {
      get {
        if (fReadStores.Count == 1)
          return fReadStores[0];
        else
          return fReadStores[fRandom.Next(fReadStores.Count)];
      }
    }

    public bool AutoCommit {
      get {
        return fAllStores[0].AutoCommit;
      }
      set {
        foreach (var store in fAllStores)
          store.AutoCommit = value;
      }
    }

    public bool AllowSearch {
      get {
        return ReadStore.AllowSearch;
      }
      set {
        foreach (var store in fAllStores)
          store.AllowSearch = value;
      }
    }

    public bool BatchMode {
      get {
        return fAllStores[0].BatchMode;
      }
      set {
        foreach (var store in fAllStores)
          store.BatchMode = value;
      }
    }

    public List<string> Buckets(string prefix) {
      return ReadStore.Buckets(prefix);
    }

    public void Clear(string bucket) {
      foreach (var store in fAllStores)
        store.Clear(bucket);
    }

    public void Commit(bool force = false) {
      foreach (var store in fAllStores)
        store.Commit(force);
    }

    public bool Delete(string bucket, string key) {
      bool result = false;

      foreach (var store in fAllStores)
        result = store.Delete(bucket, key);

      return result;
    }

    public void Dispose() {
      Dispose(true);
    }

    protected virtual void Dispose(bool disposing) {
      if (disposing) {
        foreach (var store in fAllStores)
          store.Dispose();
      }
    }

    public string Get(string bucket, string key) {
      return ReadStore.Get(bucket, key);
    }

    public T Get<T>(string bucket, string key) {
      return ReadStore.Get<T>(bucket, key);
    }

    public List<string> Get(string bucket, List<string> keys) {
      return ReadStore.Get(bucket, keys);
    }

    public List<string> Get(string bucket, string[] keys) {
      return ReadStore.Get(bucket, keys);
    }

    public byte[] GetBin(string bucket, string key) {
      return ReadStore.GetBin(bucket, key);
    }

    public List<string> Ids(string bucket) {
      return ReadStore.Ids(bucket);
    }

    public IDatusAratorObjectStore Put(string bucket, string key, string value, List<ObjectStoreSearchField> searchFields = null) {
      foreach (var store in fAllStores)
        store.Put(bucket, key, value, searchFields);
      return this;
    }

    public IDatusAratorObjectStore Put(string bucket, string key, object value, List<ObjectStoreSearchField> searchFields = null) {
      foreach (var store in fAllStores)
        store.Put(bucket, key, value, searchFields);
      return this;
    }

    public IDatusAratorObjectStore PutBin(string bucket, string key, byte[] bytes, List<ObjectStoreSearchField> searchFields = null) {
      foreach (var store in fAllStores)
        store.PutBin(bucket, key, bytes, searchFields);
      return this;
    }

    public Dictionary<string, string> Records(string bucket) {
      return ReadStore.Records(bucket);
    }

    public List<T> Records<T>(string bucket, string progressPrefix = null) {
      return ReadStore.Records<T>(bucket);
    }

    public Dictionary<string, string> Search(string field, string value) {
      return ReadStore.Search(field, value);
    }
  }
}
