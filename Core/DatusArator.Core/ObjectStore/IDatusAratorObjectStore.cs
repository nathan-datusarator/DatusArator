using System;
using System.Collections.Generic;
using DatusArator.Core.Json;

namespace DatusArator.Core.ObjectStore {
  public interface IDatusAratorObjectStore : IDisposable {
    IDatusAratorObjectStore Build(string config);

    bool AutoCommit { get; set; }
    bool AllowSearch { get; set; }
    bool BatchMode { get; set; }

    string Get(string bucket, string key);
    T Get<T>(string bucket, string key);
    List<string> Get(string bucket, List<string> keys);
    List<string> Get(string bucket, string[] keys);
    byte[] GetBin(string bucket, string key);

    IDatusAratorObjectStore Put(string bucket, string key, string value, List<ObjectStoreSearchField> searchFields = null);
    IDatusAratorObjectStore Put(string bucket, string key, object value, List<ObjectStoreSearchField> searchFields = null);
    IDatusAratorObjectStore PutBin(string bucket, string key, byte[] bytes, List<ObjectStoreSearchField> searchFields = null);

    void Commit(bool force = false);

    List<string> Buckets(string prefix);

    List<string> Ids(string bucket);
    Dictionary<string, string> Records(string bucket);
    Dictionary<string, string> Search(string field, string value);

    List<T> Records<T>(string bucket, string progressPrefix = null);

    bool Delete(string bucket, string key);

    void Clear(string bucket);
  }

  public class ObjectStoreSearchField {
    public string Field { get; set; }
    public string Value { get; set; }
    public bool Stored { get; set; } = false;
    public bool Analyzed { get; set; } = false;

    public static ObjectStoreSearchField FromJson(JsonWrapper item) {
      var result = new ObjectStoreSearchField {
        Field = item.Get("field"),
        Value = item.Get("value"),
        Stored = item.GetAsBool("stored"),
        Analyzed = item.GetAsBool("analyzed")
      };

      return result;
    }

    public void ToJson(JsonWrapper result) {
      result["field"] = Field;
      result["value"] = Value;
      result["stored"] = Stored;
      result["analyzed"] = Analyzed;
    }
  }
}
