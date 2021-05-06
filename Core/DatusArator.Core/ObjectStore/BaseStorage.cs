using DatusArator.Core.Json;
using System;
using System.Collections.Generic;

namespace DatusArator.Core.ObjectStore {
  public class BaseStorage {
    public IDatusAratorObjectStore ObjectStore;

    public void DumpInstance() {
      ObjectStore?.Commit(true);
      ObjectStore?.Dispose();
      ObjectStore = null;
    }

    public string Get(string bucket, string key) {
      return ObjectStore.Get(bucket, key);
    }

    public T Get<T>(string bucket, string key) {
      return ObjectStore.Get<T>(bucket, key);
    }

    public List<string> Get(string bucket, List<string> keys) {
      return ObjectStore.Get(bucket, keys);
    }

    public List<string> Get(string bucket, string[] keys) {
      return ObjectStore.Get(bucket, keys);
    }

    public IDatusAratorObjectStore Put(string bucket, string key, string value) {
      return ObjectStore.Put(bucket, key, value);
    }

    public IDatusAratorObjectStore Put(string bucket, string key, string value, List<ObjectStoreSearchField> searchFields) {
      return ObjectStore.Put(bucket, key, value, searchFields);
    }

    public byte[] GetBin(string bucket, string key) {
      return ObjectStore.GetBin(bucket, key);
    }

    public IDatusAratorObjectStore PutBin(string bucket, string key, byte[] value) {
      return ObjectStore.PutBin(bucket, key, value);
    }

    public List<string> Ids(string bucket) {
      return ObjectStore.Ids(bucket);
    }

    public Dictionary<string, string> Records(string bucket) {
      return ObjectStore.Records(bucket);
    }

    public List<T> Records<T>(string bucket) {
      return ObjectStore.Records<T>(bucket);
    }

    public bool Delete(string bucket, string key) {
      return ObjectStore.Delete(bucket, key);
    }

    public void Clear(string bucket) {
      if (String.IsNullOrEmpty(bucket))
        throw new Exception("You cannot clear all");

      ObjectStore.Clear(bucket);
    }

    public List<string> Buckets(string bucket) {
      return ObjectStore.Buckets(bucket);
    }

    private JsonWrapper fSequenceValues;
    public int GetNextSequenceValue(string sequanceName) {
      lock (ObjectStore) {
        if (fSequenceValues == null) {
          fSequenceValues = new JsonWrapper();
          fSequenceValues.ParseJson(Get("system", "sequences"));
        }

        var valueString = fSequenceValues.Get(sequanceName);
        var value = string.IsNullOrEmpty(valueString) ? 1000 : Convert.ToInt32(valueString) + 1;

        fSequenceValues.Put(sequanceName, value.ToString());
        Put("system", "sequences", fSequenceValues.ToJsonString());

        return value;
      }
    }
  }
}
