using System;
using System.Collections.Generic;

using DatusArator.Core.Json;
using DatusArator.Core.Util;

namespace DatusArator.Core.ObjectStore {
  public abstract class DatusAratorBaseObjectStore : IDatusAratorObjectStore {
    public abstract IDatusAratorObjectStore Build(string config);

    public bool AutoCommit { get; set; } = true;
    public bool AllowSearch { get; set; } = false;

    private bool fBatchMode = false;
    public bool BatchMode {
      get { return fBatchMode; }
      set {
        fBatchMode = value;
        BatchModeChanged();
      }
    }

    public List<string> Get(string bucket, List<string> keys) {
      return Get(bucket, keys.ToArray());
    }

    public virtual List<string> Get(string bucket, string[] keys) {
      var result = new List<string>();

      foreach (var key in keys) {
        var value = Get(bucket, key);
        if (!String.IsNullOrEmpty(value))
          result.Add(value);
      }

      return result;
    }

    public T Get<T>(string bucket, string key) {
      var json = Get(bucket, key);
      return (json != null) ? JsonUtils.JsonToObject<T>(json) : default;
    }

    public abstract List<string> Buckets(string prefix);

    public abstract List<string> Ids(string bucket);
    public abstract Dictionary<string, string> Records(string bucket);

    public virtual Dictionary<string, string> Search(string field, string value) {
      return null;
    }

    public virtual List<T> Records<T>(string bucket, string progressPrefix = null) {
      GeneralUtils.GlobalProgress?.UpdateProgress(StringUtils.Concat(progressPrefix, "Loading Data", " "), 0);

      var result = new List<T>();

      var i = 0;
      var records = Records(bucket);
      foreach (var record in records.Values) {
        result.Add(JsonUtils.JsonToObject<T>(record));

        if (++i % 100 == 0) {
          GeneralUtils.GlobalProgress?.UpdateProgress(StringUtils.Concat(progressPrefix, "Parsing", " ") + ": " + i + " of " + records.Count, i, records.Count);
        }
      }

      return result;
    }

    public abstract string Get(string bucket, string key);
    public abstract IDatusAratorObjectStore Put(string bucket, string key, string value, List<ObjectStoreSearchField> searchFields = null);
    public virtual IDatusAratorObjectStore Put(string bucket, string key, object value, List<ObjectStoreSearchField> searchFields = null) {
      return Put(bucket, key, JsonUtils.ObjectToJson(value), searchFields);
    }

    public virtual byte[] GetBin(string bucket, string key) {
      var value = Get(bucket, key);
      return (value != null) ? CryptoUtils.FromBase64(value) : null;
    }

    public virtual IDatusAratorObjectStore PutBin(string bucket, string key, byte[] bytes, List<ObjectStoreSearchField> searchFields = null) {
      var value = CryptoUtils.Base64(bytes);
      return Put(bucket, key, value, searchFields);
    }

    // Most don't have commits
    protected virtual void BatchModeChanged() { }
    public virtual void Commit(bool force = false) {

    }

    public abstract void Clear(string bucket);
    public abstract bool Delete(string bucket, string key);

    public void Dispose() {
      Dispose(true);
    }

    protected virtual void Dispose(bool disposing) {
      BatchMode = false;
    }
  }
}
