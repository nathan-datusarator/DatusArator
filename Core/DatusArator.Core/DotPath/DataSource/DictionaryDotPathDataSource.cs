using DatusArator.Core.DAO;
using System;
using System.Collections;
using System.Collections.Generic;

namespace DatusArator.Core.DotPath.DataSource {
  public class DictionaryDotPathDataSource<T> : IDotPathDataSource where T : DotPathObject {
    public DictionaryDotPathDataSource(string keyField, params string[] indexes) {
      this.KeyField = keyField;
      this.Indexes = indexes;

      if (Indexes == null) Indexes = new string[0];

      foreach (var index in Indexes)
        fIndexes[index] = new Dictionary<string, List<T>>();
    }

    public DictionaryDotPathDataSource(IEnumerable<T> items, string displayName, Schema schema, string keyField, params string[] indexes) : this(keyField, indexes) {
      this.Schema = schema;
      this.DisplayName = displayName;
      this.LinkType = items.GetType().GetGenericArguments()[0];
      Put(items);
    }

    public string Key { get; set; }
    public string DisplayName { get; set; }
    public bool IsSystem { get; set; }
    public bool IsPrimary { get; set; } = true;

    public string SuperGrid { get; set; } = "Grid";

    public Type LinkType { get; set; }
    public string ClassName { get; set; }
    public Schema Schema { get; set; }

    public string KeyField { get; private set; }
    public string[] Indexes { get; private set; }

    protected Dictionary<string, T> fData { get; } = new Dictionary<string, T>();
    protected Dictionary<string, Dictionary<string, List<T>>> fIndexes { get; } = new Dictionary<string, Dictionary<string, List<T>>>();

    public Action CheckAllLoaded { get; set; }

    public override string ToString() {
      return DisplayName;
    }

    public List<K> Items<K>() where K : DotPathObject {
      var result = new List<K>();
      foreach (var item in fData.Values) {
        if (item as K != null)
          result.Add(item as K);
      }

      return result;
    }

    public IList GetAll() {
      CheckAllLoaded?.Invoke();
      return new List<T>(fData.Values);
    }

    #region Get
    public DotPathObject Get(string keyName, string keyValue) {
      if (keyName == KeyField) {
        if (fData.ContainsKey(keyValue))
          return fData[keyValue];
        else
          return null;
      } else {
        var indexedValues = GetIndexedList(keyName, keyValue);
        if (indexedValues != null) {
          return indexedValues.Count > 0 ? indexedValues[0] : null;
        } else { 
          foreach (DotPathObject item in fData.Values) {
            if (item[keyName]?.ToString() == keyValue)
              return item;
          }
        }

        return null;
      }
    }

    // Indexing
    public IEnumerable GetChildren(string keyName, string keyValue) {
      var indexedValues = GetIndexedList(keyName, keyValue);
      if (indexedValues != null) {
        return new List<T>(indexedValues);
      } else {
        var result = new List<T>();
        foreach (T item in fData.Values) {
          if (item[keyName]?.ToString() == keyValue)
            result.Add(item);
        }
        return result;
      }
    }

    private List<T> GetIndexedList(string keyName, string keyValue) {
      if (!fIndexes.ContainsKey(keyName))
        return null;

      var dictionary = fIndexes[keyName];
      if (dictionary.ContainsKey(keyValue))
        return dictionary[keyValue];
      else
        return new List<T>();
    }
    #endregion

    #region Put
    public void Put(IEnumerable<T> values) {
      foreach (var value in values)
        Put(value);
    }

    public void Put(T value) {
      var key = value[KeyField]?.ToString();
      if (!string.IsNullOrEmpty(key))
        fData[key] = value;

      if (fIndexes != null) {
        foreach (var index in Indexes)
          PutInIndex(index, value);
      }
    }

    private void PutInIndex(string index, T value) {
      var key = value[index]?.ToString();
      if (string.IsNullOrEmpty(key))
        return;

      var dict = fIndexes[index];
      List<T> values;
      if (dict.ContainsKey(key))
        values = dict[key];
      else {
        values = new List<T>();
        dict[key] = values;
      }

      values.Add(value);
    }
    #endregion
  }
}
