using DatusArator.Core.Json;
using DatusArator.Core.Util;

using System;
using System.Collections.Generic;

namespace DatusArator.Core.ObjectStore {
  public class DatusAratorMemoryObjectStore : DatusAratorBaseObjectStore {
    #region Serialize and Deserialize
    public override IDatusAratorObjectStore Build(string config) {
      if (config.StartsWith("Memory", StringComparison.InvariantCultureIgnoreCase))
        return new DatusAratorMemoryObjectStore();
      else
        return null;
    }

    public string Serialize(bool plainJson = false) {
      var value = JsonUtils.ObjectToJson(this, true);
      if (!plainJson)
        value = CryptoUtils.GzipBase64String(value);

      return value;
    }

    public static DatusAratorMemoryObjectStore Deserialize(string value) {
      var json = value.StartsWith("{") ? value : CryptoUtils.FromGzipBase64String(value);
      return JsonUtils.JsonToObject<DatusAratorMemoryObjectStore>(json);
    }

    public class CopyObjectStoreOptions {
      public bool Clear { get; set; } = true;
      public List<string> Bin { get; set; } = new List<string>();
      public List<string> Skip { get; set; } = new List<string>();
    }

    public DatusAratorMemoryObjectStore CopyObjectStore(IDatusAratorObjectStore source, bool clear = true, params string[] binDirs) {
      var options = new CopyObjectStoreOptions() { Clear = clear, };
      options.Bin.AddRange(binDirs);

      return CopyObjectStore(source, options);
    }

    public DatusAratorMemoryObjectStore CopyObjectStore(IDatusAratorObjectStore source, CopyObjectStoreOptions options) {
      if (options.Clear)
        Clear(null);

      var buckets = source.Buckets("");
      foreach (var bucket in buckets) {
        if (SkipBucket(bucket))
          continue;

        if (options.Bin.Contains(bucket)) {
          foreach (var key in source.Ids(bucket)) {
            var data = source.GetBin(bucket, key);
            PutBin(bucket, key, data);
          }
        } else {
          foreach (var record in source.Records(bucket)) {
            string value = record.Value;
            Put(bucket, record.Key, value);
          }
        }
      }

      return this;

      bool SkipBucket(string bucket) {
        foreach (var skip in options.Skip) {
          if (bucket.Equals(skip))
            return true;
          if (bucket.StartsWith(skip + ":"))
            return true;
        }
        return false;
      }
    }
    #endregion

    public override string ToString() {
      return "Memory Object Store. " + KeyMap.Count + " objects";
    }

    #region Implementation
    private Dictionary<string, string> fKeyMap;
    public Dictionary<string, string> KeyMap {
      get {
        if (fKeyMap == null)
          fKeyMap = new Dictionary<string, string>();
        return fKeyMap;
      }
      protected set {
        fKeyMap = value;
      }
    }

    public override List<string> Buckets(string prefix) {
      var result = new List<string>();

      bool isEmpty = string.IsNullOrEmpty(prefix);
      foreach (var key in KeyMap.Keys) {
        if (isEmpty)
          result.Add(GetBucket(key));
        else if (key.StartsWith(prefix))
          result.Add(GetBucket(key));
      }

      return result;
    }

    public override List<string> Ids(string bucket) {
      var result = new List<string>();
      foreach (var key in KeyMap.Keys) {
        if (string.IsNullOrEmpty(bucket)) {
          if (!key.Contains("!"))
            result.Add(GetId(key));
        } else if (key.StartsWith(bucket + "!"))
          result.Add(GetId(key));
      }

      return result;
    }

    public override Dictionary<string, string> Records(string bucket) {
      var result = new Dictionary<string, string>();
      foreach (var key in KeyMap.Keys) {
        if (string.IsNullOrEmpty(bucket)) {
          if (!key.Contains("!"))
            result[key] = KeyMap[key];
        } else if (key.StartsWith(bucket + "!"))
          result[key] = KeyMap[key];
      }

      return result;
    }

    public override string Get(string bucket, string key) {
      key = BuildKey(bucket, key);

      if (KeyMap.ContainsKey(key))
        return KeyMap[key];

      return null;
    }

    public override IDatusAratorObjectStore Put(string bucket, string key, string value, List<ObjectStoreSearchField> searchFields = null) {
      key = BuildKey(bucket, key);
      KeyMap[key] = value;
      return this;
    }

    public override bool Delete(string bucket, string key) {
      key = BuildKey(bucket, key);
      if (!KeyMap.ContainsKey(key))
        return false;

      KeyMap.Remove(key);
      return true;
    }

    public override void Clear(string bucket) {
      if (bucket == null)
        fKeyMap = null;
      else {
        var toBeDeleted = new List<string>();
        foreach (var key in KeyMap.Keys) {
          if (key.StartsWith(bucket + "!"))
            toBeDeleted.Add(key);
        }

        foreach (var key in toBeDeleted)
          KeyMap.Remove(key);
      }
    }

    protected override void Dispose(bool disposing) {
      if (disposing)
        Clear(null);
    }

    private string BuildKey(string bucket, string key) {
      return StringUtils.Concat(bucket, key, "!");
    }

    private string GetBucket(string key) {
      var index = key.IndexOf('!');
      return (index >= 0) ? key.Substring(0, index) : "";
    }

    private string GetId(string key) {
      var index = key.IndexOf('!');
      return key.Substring(index + 1);
    }
    #endregion
  }
}
