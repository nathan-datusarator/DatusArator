using DatusArator.Core.DotPath;
using DatusArator.Core.Logging;
using DatusArator.Core.Util;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Globalization;
using System.Collections;
using Newtonsoft.Json;

namespace DatusArator.Core.Json {
  public class JsonWrapper : DotPathObject {
    public static bool THROW_PARSE_EXCEPTIONS = false;

    private readonly Dictionary<string, TypedBasicObject> values = new Dictionary<string, TypedBasicObject>();
    private readonly Dictionary<string, int> arrays = new Dictionary<string, int>();

    private bool fTrackChanges = true;
    private Dictionary<string, JsonWrapperChange> fChanges = new Dictionary<string, JsonWrapperChange>();

    [JsonIgnore]
    public JsonWrapper ParentWrapper { get; private set; }
    [JsonIgnore]
    public string KeyBase { get; private set; }

    private readonly Dictionary<string, string> metaData = new Dictionary<string, string>();

    [JsonIgnore]
    public bool LoadedFromDatabase { get; set; } = false;
    [JsonIgnore]
    public string TableName { get; set; }
    [JsonIgnore]
    public List<string> KeyFields { get; } = new List<string>();

    #region Constructors, ToString
    public JsonWrapper() { }

    public JsonWrapper(string json) {
      if (!string.IsNullOrEmpty(json))
        ParseJson(json);

      ClearChanges();
    }

    public JsonWrapper(JsonWrapper parent, string keyBase) {
      this.ParentWrapper = parent;
      this.KeyBase = keyBase;
    }

    public T ToObject<T>() {
      return JsonUtils.JsonToObject<T>(ToJsonString());
    }

    public static JsonWrapper FromObject(object source) {
      return new JsonWrapper(JsonUtils.ObjectToJson(source));
    }

    public override string ToString() {
      return ToJsonString();
    }
    #endregion

    #region DotPathObject overrides
    public override Type GetType(string key) {
      var rawValue = GetRaw(key);
      return rawValue != null ? rawValue.Value.GetType() : typeof(string);
    }

    public override T Clone<T>() {
      var result = new T();
      (result as JsonWrapper).ParseJson(ToJsonString());

      return result;
    }

    public override object GetValue(string propertyName) {
      var index = propertyName.IndexOf(".");
      if (index > 0) {
        if (GetChildValue(propertyName.Substring(0, index), propertyName.Substring(index + 1), out object result))
          return result;
      } else if (GetChildValue(propertyName, null, out object result))
        return result;

      var raw = GetRaw(propertyName);
      if (raw == null) {
        var wrapper = new JsonWrapper(this, propertyName);
        if (wrapper.GetValues().Count > 0)
          return wrapper;
        else
          return null;
      } else {
        return raw.Value;
      }
    }

    public override void SetValue(string propertyName, object value) {
      var index = propertyName.IndexOf(".");
      if ((index > 0) && SetChildValue(propertyName.Substring(0, index), propertyName.Substring(index + 1), value))
        return;

      Put(propertyName, value);
    }

    public override PropertyInfo GetPropertyInfo(string propertyName, bool createSubObjects, out object outRoot, out int? outIndex) {
      outRoot = this;
      outIndex = null;

      return new JsonWrapperPropertyInfo(propertyName, GetType(propertyName));
    }

    public override object Evaluate(string exp) {
      string filterExp = null;
      var index = exp.IndexOf(":");
      if (index > 0) {
        filterExp = exp.Substring(index + 1);
        filterExp = filterExp.Substring(0, filterExp.Length);

        exp = exp.Substring(0, index);
      }

      // Find Parent Object
      string rootPath = null;
      index = exp.LastIndexOf(".");
      if (index > 0) {
        rootPath = exp.Substring(0, index);
        exp = exp.Substring(index + 1);
      }

      var filter = string.IsNullOrEmpty(filterExp) ? null : new DotPathFilter(filterExp, null);

      if ((rootPath != null) && (GetArrayLength(rootPath) > 0)) {
        var result = new List<string>();
        foreach (JsonWrapper obj in GetArrayAsObjects(rootPath)) {
          var value = Evaluate(obj, exp, filter)?.ToString();
          if (value != null && !result.Contains(value))
            result.Add(value);
        }

        return result.Count > 0 ? StringUtils.Concat(result, ", ") : null;
      } else {
        var root = string.IsNullOrEmpty(rootPath) ? this : new JsonWrapper(this, rootPath);
        return Evaluate(root, exp, filter);
      }
    }

    private object Evaluate(JsonWrapper root, string exp, DotPathFilter filter) {
      // Evaluate Filter
      if (filter != null) {
        var filterResult = filter.Evaluate(root);
        if (!filterResult || filter.MissingValues)
          return null;
      }

      if (exp.StartsWith("'") || exp.StartsWith("\""))
        return exp.Substring(1, exp.Length - 2);

      if (root.GetArrayLength(exp) > 0) {
        string result = "";
        foreach (JsonWrapper obj in GetArrayAsObjects(exp)) {
          result = StringUtils.Concat(result, obj.ToString(), ", ");
        }
        return string.IsNullOrEmpty(result) ? null : result;
      } else {
        return root.GetValue(exp);
      }
    }

    public DotPathObject AssignTo(DotPathObject target) {
      foreach (var key in GetFields()) {
        target[key] = GetRaw(key)?.Value;
      }

      return target;
    }
    #endregion

    #region Join Keys, Bulk Clear
    public string JoinKeys(string key1, string key2) {
      if ((key1 != null) && key1.EndsWith("."))
        key1 = key1.Substring(key1.Length - 1);

      if ((key2 != null) && key2.StartsWith("."))
        key2 = key2.Substring(1);

      if (string.IsNullOrEmpty(key1))
        return key2;

      if (string.IsNullOrEmpty(key2))
        return key1;

      if (key2.StartsWith("["))
        return key1 + key2;

      return key1 + "." + key2;
    }

    public void Clear() {
      if (ParentWrapper != null) {
        ParentWrapper.ClearAll(KeyBase);
      } else {
        values.Clear();
        arrays.Clear();

        ClearChanges();
      }

      metaData.Clear();
    }

    public void ClearAll(string prefix) {
      if (ParentWrapper != null) {
        ParentWrapper.ClearAll(JoinKeys(KeyBase, prefix));
        return;
      }

      var keys = GetFields(prefix);
      foreach (var key in keys)
        ClearValue(key);

      var arrayKeys = GetArrayKeys(prefix);
      foreach (var arrayKey in arrayKeys)
        SetArrayLength(arrayKey, 0);
    }
    #endregion

    #region Fields
    public List<string> GetFields(string prefix = null) {
      var result = new List<string>();

      if (ParentWrapper != null) {
        var parentFields = ParentWrapper.GetFields(JoinKeys(KeyBase, prefix));

        if (string.IsNullOrEmpty(KeyBase))
          return parentFields;
        else {
          var keySubBase = StringUtils.RemoveArrayReferences(KeyBase);
          foreach (var field in parentFields)
            result.Add(field.Substring(keySubBase.Length + 1));

          return result;
        }
      }

      bool matchAll = string.IsNullOrEmpty(prefix);

      foreach (var key in values.Keys) {
        if (matchAll || key.StartsWith(prefix))
          result.Add(StringUtils.RemoveArrayReferences(key));
      }

      result.Sort();

      return StringUtils.RemoveDuplicates(result);
    }

    // Change this to only return immediate sub fields
    public List<string> GetSubFields(string prefix) {
      var fields = GetFields(prefix);
      List<string> subFields;

      if (!string.IsNullOrEmpty(prefix))
        subFields = fields.Select(s => s.Substring(prefix.Length + 1)).ToList();
      else
        subFields = fields;

      return subFields.Select(s => (s.IndexOf(".") > 0) ? s.Substring(0, s.IndexOf(".")) : s).Distinct().ToList();
    }

    public List<JsonWrapper> GetSubFieldsAsObjects(string prefix) {
      var result = new List<JsonWrapper>();
      foreach (var field in GetSubFields(prefix))
        result.Add(new JsonWrapper(this, StringUtils.Concat(prefix, field, ".")));

      return result;
    }

    public List<string> GetArrayKeys(string prefix = null) {
      var result = new List<string>();

      if (ParentWrapper != null) {
        var parentArrays = ParentWrapper.GetArrayKeys(JoinKeys(KeyBase, prefix));
        if (string.IsNullOrEmpty(KeyBase))
          return parentArrays;
        else {
          foreach (var field in parentArrays)
            result.Add(field.Substring(KeyBase.Length + 1));

          return result;
        }
      }

      bool matchAll = string.IsNullOrEmpty(prefix);

      foreach (var key in arrays.Keys) {
        if (matchAll || key.StartsWith(prefix))
          result.Add(key);
      }

      result.Sort();

      return StringUtils.RemoveDuplicates(result);
    }
    #endregion

    #region Accessors
    public Dictionary<string, TypedBasicObject>.Enumerator GetEnumerator() {
      return GetValues().GetEnumerator();
    }

    public Dictionary<string, TypedBasicObject> GetValues(string prefix = null) {
      var result = new Dictionary<string, TypedBasicObject>();

      if (ParentWrapper != null) {
        var parentValues = ParentWrapper.GetValues(JoinKeys(KeyBase, prefix));
        if (string.IsNullOrEmpty(KeyBase))
          return parentValues;
        else {
          foreach (var entry in parentValues)
            result[entry.Key.Substring(KeyBase.Length + 1)] = entry.Value;

          return result;
        }
      }

      if (string.IsNullOrEmpty(prefix))
        return values;

      if (!prefix.EndsWith("."))
        prefix += ".";

      foreach (var entry in values) {
        if (entry.Key.StartsWith(prefix))
          result[entry.Key] = entry.Value;
      }

      return result;
    }

    public Dictionary<string, int> GetArrays(string prefix = null) {
      var result = new Dictionary<string, int>();

      if (ParentWrapper != null) {
        var parentValues = ParentWrapper.GetArrays(JoinKeys(KeyBase, prefix));
        if (string.IsNullOrEmpty(KeyBase))
          return parentValues;
        else {
          foreach (var entry in parentValues)
            result[entry.Key.Substring(KeyBase.Length + 1)] = entry.Value;

          return result;
        }
      }

      if (string.IsNullOrEmpty(prefix))
        return arrays;

      if (!prefix.EndsWith("."))
        prefix += ".";

      foreach (var entry in arrays) {
        if (entry.Key.StartsWith(prefix))
          result[entry.Key] = entry.Value;
      }

      return result;
    }

    public Dictionary<string, string> GetAllMetaData() {
      return metaData;
    }
    #endregion

    #region Change Management
    [JsonIgnore]
    public bool TrackChanges {
      get {
        if (ParentWrapper != null)
          return ParentWrapper.TrackChanges;

        return fTrackChanges;
      }
      set {
        fTrackChanges = value;
      }
    }

    private void UpdateChangeList(char updateType, string key, TypedBasicObject oldValue, TypedBasicObject newValue) {
      if (!TrackChanges)
        return;

      if (fChanges.ContainsKey(key)) {
        var change = fChanges[key];
        change.Update(newValue);
        if (change.UpdateType == 'X')
          fChanges.Remove(key);
      } else {
        fChanges[key] = new JsonWrapperChange() { UpdateType = updateType, Key = key, OldValue = oldValue, NewValue = newValue };
      }
    }

    private JsonWrapper UndoChange(JsonWrapperChange change) {
      if (change == null)
        return this;

      if (change.UpdateType == 'A') {
        string key = change.Key;
        if (key.EndsWith("[]"))
          key = key.Substring(0, key.Length - 2);

        SetArrayLength(key, (int)change.OldValue.GetAsInt(0));
      } else
        Put(change.Key, change.OldValue);

      return this;
    }

    public JsonWrapper UndoChange(string key) {
      if (ParentWrapper != null) {
        ParentWrapper.UndoChange(JoinKeys(KeyBase, key));
        return this;
      }

      return UndoChange(GetChange(key));
    }

    public JsonWrapper UndoChanges(string prefix = null) {
      if (ParentWrapper != null) {
        ParentWrapper.UndoChanges(JoinKeys(KeyBase, prefix));
        return this;
      }

      var changeList = new Dictionary<string, JsonWrapperChange>(GetChanges(prefix));
      foreach (var change in changeList.Values)
        UndoChange(change);

      return this;
    }

    public Dictionary<string, JsonWrapperChange> GetChanges(string prefix = null) {
      var result = new Dictionary<string, JsonWrapperChange>();

      if (ParentWrapper != null) {
        var parentValues = ParentWrapper.GetChanges(JoinKeys(KeyBase, prefix));
        if (string.IsNullOrEmpty(KeyBase))
          return parentValues;
        else {
          foreach (var entry in parentValues)
            result[entry.Key.Substring(KeyBase.Length + 1)] = entry.Value;

          return result;
        }
      }

      if (string.IsNullOrEmpty(prefix))
        return fChanges;

      if (!prefix.EndsWith("."))
        prefix += ".";

      foreach (var entry in fChanges) {
        if (entry.Key.StartsWith(prefix))
          result[entry.Key] = entry.Value;
      }

      return result;
    }

    public bool HasChanges(string prefix = null) {
      if (ParentWrapper != null)
        return ParentWrapper.HasChanges(JoinKeys(KeyBase, prefix));

      return (!TrackChanges && GetValues(prefix).Count > 0) || (GetChanges(prefix).Count > 0);
    }

    public bool HasChange(string key) {
      if (ParentWrapper != null)
        return ParentWrapper.HasChange(JoinKeys(KeyBase, key));

      return !TrackChanges || (GetChange(key) != null);
    }

    public JsonWrapperChange GetChange(string key) {
      return fChanges.ContainsKey(key) ? fChanges[key] : null;
    }

    public object GetOriginalValue(string keyField) {
      if (!TrackChanges)
        return GetRaw(keyField);

      var change = GetChange(keyField);
      if (change != null)
        return change.OldValue;

      return GetRaw(keyField);
    }

    public void ClearChanges(string prefix = null) {
      if (ParentWrapper != null) {
        ParentWrapper.ClearChanges(JoinKeys(KeyBase, prefix));
        return;
      }

      var newChanges = new Dictionary<string, JsonWrapperChange>();

      if (!string.IsNullOrEmpty(prefix)) {
        if (!prefix.EndsWith("."))
          prefix += ".";

        foreach (var change in fChanges)
          if (!change.Key.StartsWith(prefix))
            newChanges[change.Key] = change.Value;
      }

      fChanges = newChanges;
    }

    public string SummarizeChanges() {
      StringBuilder sb = new StringBuilder("[");

      foreach (var change in fChanges.Values) {
        if (change.Key.StartsWith("audit."))
          continue;

        if (sb.Length > 1)
          sb.Append(", ");
        sb.Append("(").Append(change.Key).Append(") ").Append(change.OldValue).Append(" -> ").Append(change.NewValue);
      }
      sb.Append("]");

      return sb.ToString();
    }
    #endregion

    #region IDs
    public string GetId() {
      string id = Get("id");
      if (string.IsNullOrEmpty(id)) {
        id = GetIdCode();

        id = id + '-' + System.Guid.NewGuid().ToString().Replace("-", "");
        Put("id", new TypedBasicObject(id));
      }

      return id;
    }

    public void SetId(string id) {
      if (string.IsNullOrEmpty(id) || (id.Length < 2)) {
        ClearValue("id");
      } else {
        SetIdCode(id.Substring(0, 2));
        Put("id", new TypedBasicObject(id));
      }
    }

    public string GetIdCode() {
      string result = GetMetaData("idcode");
      return string.IsNullOrEmpty(result) ? "XX" : result;
    }

    public void SetIdCode(string idCode) {
      SetMetaData("idcode", idCode);
    }

    #endregion

    #region Get Value
    public TypedBasicObject GetRaw(string key) {
      if (key == null)
        return null;

      if (ParentWrapper != null)
        return ParentWrapper.GetRaw(JoinKeys(KeyBase, key));

      if (values.ContainsKey(key))
        return values[key];
      else
        return null;
    }

    public bool HasValue(string key) {
      var value = GetRaw(key);
      return (value != null) && value.HasValue();
    }

    public string Get(string key, string defValue = null) {
      TypedBasicObject rawValue = GetRaw(key);
      if (rawValue != null)
        return rawValue.GetAsString(defValue);

      if (GetArrayLength(key) > 0)
        return GetArrayAsCSV(key, " | ");

      return defValue;
    }

    public int? GetAsInt(string key, int? defValue = null) {
      TypedBasicObject rawValue = GetRaw(key);
      return (rawValue != null) ? rawValue.GetAsInt(defValue) : defValue;
    }

    public bool GetAsBool(string key, bool defValue = false) {
      TypedBasicObject rawValue = GetRaw(key);
      return (rawValue != null) ? rawValue.GetAsBool(defValue) : defValue;
    }

    public decimal? GetAsDecimal(string key, decimal? defValue = null) {
      TypedBasicObject rawValue = GetRaw(key);
      return (rawValue != null) ? rawValue.GetAsDecimal(defValue) : defValue;
    }

    public double? GetAsDouble(string key, double? defValue = null) {
      TypedBasicObject rawValue = GetRaw(key);
      return (rawValue != null) ? rawValue.GetAsDouble(defValue) : defValue;
    }

    public long? GetAsLong(string key, long? defValue = null) {
      TypedBasicObject rawValue = GetRaw(key);
      return (rawValue != null) ? rawValue.GetAsLong(defValue) : defValue;
    }

    public DateTime? GetAsDate(string key, DateTime? defValue = null) {
      TypedBasicObject rawValue = GetRaw(key);
      return (rawValue != null) ? rawValue.GetAsDate(defValue) : defValue;
    }

    public string GetMultiKeyValue(string multiKey) {
      if (multiKey.IndexOf('-') < 0)
        return Get(multiKey);

      StringBuilder result = new StringBuilder();
      foreach (string part in multiKey.Split('-')) {
        if (result.Length > 0)
          result.Append('-');
        result.Append(Get(part));
      }

      return result.ToString();
    }
    #endregion

    #region Set Value
    public JsonWrapper Put(string key, TypedBasicObject value) {
      if (ParentWrapper != null) {
        ParentWrapper.Put(JoinKeys(KeyBase, key), value);
        return this;
      }

      if (values.ContainsKey(key)) {
        TypedBasicObject currentValue = values[key];
        if ((value != null) && (value.IsNotNull())) {
          if (!currentValue.SameValue(value)) {
            UpdateChangeList('U', key, currentValue, value);
            values[key] = value;
          }
        } else {
          if (currentValue.IsNotNull())
            UpdateChangeList('R', key, currentValue, null);
          values.Remove(key);
        }
      } else {
        if ((value != null) && (value.IsNotNull())) {
          UpdateChangeList('N', key, null, value);
          values[key] = value;
        }
      }

      return this;
    }

    public JsonWrapper Put(string key, string value) {
      return Put(key, new TypedBasicObject(value));
    }

    public JsonWrapper Put(string key, Int32 value) {
      return Put(key, new TypedBasicObject(value));
    }

    public JsonWrapper Put(string key, Double value) {
      return Put(key, new TypedBasicObject(value));
    }

    public JsonWrapper Put(string key, DateTime value) {
      return Put(key, new TypedBasicObject(value));
    }

    public JsonWrapper Put(string key, bool value) {
      return Put(key, new TypedBasicObject(value));
    }

    public JsonWrapper Put(string key, object value) {
      return Put(key, new TypedBasicObject(value));
    }

    public JsonWrapper ClearValue(string key) {
      return Put(key, (TypedBasicObject)null);
    }
    #endregion

    #region Arrays
    public int GetArrayLength(string key) {
      if (ParentWrapper != null)
        return ParentWrapper.GetArrayLength(JoinKeys(KeyBase, key));

      if (arrays.ContainsKey(key))
        return arrays[key];

      return 0;
    }

    public void SetArrayLength(string key, int length) {
      if (ParentWrapper != null) {
        ParentWrapper.SetArrayLength(JoinKeys(KeyBase, key), length);
        return;
      }

      int current = GetArrayLength(key);
      if (current == length)
        return;

      if (length > 0)
        arrays[key] = length;
      else if (arrays.ContainsKey(key))
        arrays.Remove(key);

      UpdateChangeList('A', key + "[]", new TypedBasicObject(current), new TypedBasicObject(length));
    }

    public JsonWrapper ExtendArray(string key) {
      int length = GetArrayLength(key);
      SetArrayLength(key, length + 1);

      return new JsonWrapper(this, key + "[" + length + "]");
    }

    public void ExtendArray(string key, List<JsonWrapper> list) {
      for (var i = 0; i < list.Count; i++) {
        var tempObj = ExtendArray(key);
        tempObj.CopyDataFrom(list[i], "");
      }
    }

    public string GetArrayAsCSV(string key) {
      return GetArrayAsCSV(key, ", ");
    }

    public string GetArrayAsCSV(string key, string delimeter) {
      if (key == null)
        return null;

      int length = GetArrayLength(key);
      if (length <= 0)
        return Get(key);

      string value = null;
      for (int i = 0; i < length; i++) {
        string current = Get(string.Format("{0}[{1}]", key, i));
        if (!string.IsNullOrEmpty(current))
          value = StringUtils.Concat(value, current, delimeter);
      }

      return value;
    }

    public List<JsonWrapper> GetArrayAsObjects(string key) {
      var result = new List<JsonWrapper>();
      for (int i = 0; i < GetArrayLength(key); i++)
        result.Add(new JsonWrapper(this, key + "[" + i + "]"));

      return result;
    }

    public JsonWrapper SetArrayAsObjects(string key, List<JsonWrapper> items) {
      ClearAll(key);
      if ((items?.Count ?? 0) == 0)
        return this;

      ExtendArray(key, items);

      return this;
    }

    public List<string> GetArrayAsList(string key, bool suppressNulls = true) {
      var result = new List<string>();

      for (int i = 0; i < GetArrayLength(key); i++) {
        string value = Get(key + '[' + i + ']');
        if (!(suppressNulls && string.IsNullOrEmpty(value)))
          result.Add(value);
      }

      return result;
    }

    public JsonWrapper SetArrayAsList(string key, List<string> values) {
      for (int i = values.Count; i < GetArrayLength(key); i++)
        ClearValue(key + '[' + i + ']');

      SetArrayLength(key, values.Count);
      for (int i = 0; i < values.Count; i++)
        Put(key + '[' + i + ']', values[i]);

      return this;
    }

    public bool ArrayContains(string key, string value) {
      if (string.IsNullOrEmpty(value))
        return false;

      for (int i = 0; i < GetArrayLength(key); i++) {
        if (value.Equals(Get(key + '[' + i + ']')))
          return true;
      }

      return false;
    }

    public void AddToArray(string key, string value) {
      if (ArrayContains(key, value))
        return;

      var currentLength = GetArrayLength(key);
      Put(key + '[' + currentLength + ']', value);
      SetArrayLength(key, currentLength + 1);
    }

    public void RemoveFromArray(string key, string value) {
      if (string.IsNullOrEmpty(value))
        return;

      var currentLength = GetArrayLength(key);
      for (int i = 0; i < currentLength; i++) {
        string arrayKey = key + '[' + i + ']';
        if (value.Equals(Get(arrayKey))) {
          ClearValue(arrayKey);

          if (i == (currentLength - 1))
            SetArrayLength(key, currentLength - 1);

          break;
        }
      }
    }

    public void CollapseArray(string key) {
      var arrayValues = GetArrayAsList(key, true);
      var currentLength = GetArrayLength(key);

      if (arrayValues.Count == currentLength)
        return;

      SetArrayLength(key, arrayValues.Count);

      for (int i = 0; i < arrayValues.Count; i++)
        this.values[key + '[' + i + ']'] = new TypedBasicObject(arrayValues[i]);

      for (int i = arrayValues.Count; i < currentLength; i++) {
        string arrayKey = key + '[' + i + ']';
        if (values.ContainsKey(arrayKey))
          values.Remove(arrayKey);
      }
    }

    public void PruneArray(string key) {
      for (int i = GetArrayLength(key) - 1; i >= 0; i--) {
        if (GetRaw(key + '[' + i + ']') != null) {
          SetArrayLength(key, i + 1);
          return;
        }
      }

      SetArrayLength(key, 0);
    }
    #endregion

    #region Meta Data
    public string GetMetaData(string key) {
      if (key == null)
        return null;

      if (metaData.ContainsKey(key))
        return metaData[key];
      else
        return null;
    }

    public void SetMetaData(string key, string value) {
      if (string.IsNullOrEmpty(key))
        return;

      metaData[key] = value;
    }
    #endregion

    #region Json Parsing
    public JsonWrapper ParseJson(string json) {
      if (string.IsNullOrEmpty(json))
        return this;

      try {
        JObject root = JObject.Parse(json);
        return ParseJson(root);
      } catch (Exception ex) {
        if (THROW_PARSE_EXCEPTIONS)
          throw;
        else {
          DatusAratorLogger.Log(DatusAratorLogLevel.ERROR, "Invalid Json: " + ex.ToString());
          return this;
        }
      }
    }

    public JsonWrapper ParseJson(JToken root) {
      Clear();

      WalkTree(root, "");

      return this;
    }

    private void WalkTree(JToken current, string path) {
      if (current.Type == JTokenType.Property) {
        WalkTree(current.First, path);
      } else if (current.Type == JTokenType.String)
        Put(path, new TypedBasicObject(current.Value<string>()));
      else if (current.Type == JTokenType.Integer)
        Put(path, new TypedBasicObject(current.Value<long>()));
      else if (current.Type == JTokenType.Float)
        Put(path, new TypedBasicObject(current.Value<Double>()));
      else if (current.Type == JTokenType.Boolean)
        Put(path, new TypedBasicObject(current.Value<Boolean>()));
      else if (current.Type == JTokenType.Date)
        Put(path, new TypedBasicObject(current.Value<DateTime>()));
      else if (current.Type == JTokenType.Null)
        ClearValue(path);
      else if (current.Type == JTokenType.Array) {
        int i = 0;
        foreach (var child in current.Children()) {
          WalkTree(child, string.Format("{0}[{1}]", path, i++));
          arrays[path] = i;
        }
      } else {
        foreach (var child in current.Children()) {
          WalkTree(child, StringUtils.Concat(path, ((JProperty)child).Name, "."));
        }
      }
    }
    #endregion

    #region Output Json string
    public override string ToJsonString(bool indented = false) {
      JsonWrapperSerializer serializer = new JsonWrapperSerializer(GetValues(), GetArrays(), null);
      var json = serializer.BuildJsonString(null);
      return indented ? JsonUtils.FormatJsonTabbed(json) : json;
    }

    public string ToJsonStringFiltered(List<string> map, List<string> hiddenFields = null) {
      JsonWrapperSerializer serializer = new JsonWrapperSerializer(GetValues(), GetArrays(), hiddenFields);

      return serializer.BuildJsonString(map);
    }
    #endregion

    #region Merge
    public void MergeWrapper(JsonWrapper source, bool fullMerge = true, List<string> readOnlyFields = null) {
      if (readOnlyFields == null)
        readOnlyFields = new List<string>();

      if (fullMerge) {
        var toBeRemoved = new List<string>();

        foreach (var key in GetValues().Keys)
          if (!readOnlyFields.Contains(key) && !(source.values.ContainsKey(key)))
            toBeRemoved.Add(key);
        foreach (string key in toBeRemoved)
          ClearValue(key);

        toBeRemoved.Clear();
        foreach (string key in GetArrays().Keys)
          if (source.GetArrayLength(key) == 0)
            toBeRemoved.Add(key);
        foreach (string key in toBeRemoved)
          SetArrayLength(key, 0);
      }

      foreach (var entry in source.GetValues())
        if (!readOnlyFields.Contains(entry.Key))
          Put(entry.Key, entry.Value);

      foreach (var entry in source.GetArrays())
        SetArrayLength(entry.Key, entry.Value);
    }
    #endregion

    #region Helper Functions
    public bool HasData() {
      if (ParentWrapper != null) {
        return GetFields().Count > 0;
      }

      return (values.Count > 0);
    }

    public JsonWrapper CopyDataFrom(object o, string prefix) {
      var source = new JsonWrapper(JsonUtils.ObjectToJson(o));
      return CopyDataFrom(source, prefix);
    }

    public JsonWrapper CopyDataFrom(JsonWrapper source, string prefix) {
      if (source == null)
        return this;

      foreach (var entry in source.GetValues())
        Put(StringUtils.Concat(prefix, entry.Key, "."), entry.Value);

      foreach (var entry in source.GetArrays())
        SetArrayLength(StringUtils.Concat(prefix, entry.Key, "."), entry.Value);

      return this;
    }

    public JsonWrapper MapDataFrom(object o, string prefix, Dictionary<string, string> map) {
      var source = new JsonWrapper(JsonUtils.ObjectToJson(o));
      return MapDataFrom(source, prefix, map);
    }

    public JsonWrapper MapDataFrom(JsonWrapper source, string prefix, Dictionary<string, string> map) {
      if (source == null)
        return this;

      if ((map == null) || (map.Count == 0)) {
        CopyDataFrom(source, prefix);
        return this;
      }

      foreach (var key in map.Keys) {
        if (string.IsNullOrEmpty(map[key]))
          continue;

        TypedBasicObject value = source.GetRaw(key);
        if ((value != null) && value.IsNotNull() && !"NULL".Equals(value.GetAsString().ToUpper()))
          Put(StringUtils.Concat(prefix, map[key], "."), value);
      }

      return this;
    }

    public static List<JsonWrapper> ParseJsonArray(string jsonArray) {
      var result = new List<JsonWrapper>();

      var root = JArray.Parse(jsonArray);
      if (root.Type == JTokenType.Array) {
        foreach (var child in root.Children()) {
          JsonWrapper wrapper = new JsonWrapper();
          wrapper.WalkTree(child, "");

          result.Add(wrapper);
        }
      }

      return (result.Count > 0) ? result : null;
    }

    public static List<T> ToList<T>(List<JsonWrapper> items) where T : DotPathObject {
      if ((items?.Count ?? 0) == 0)
        return null;

      var result = new List<T>();
      foreach (var item in items)
        result.Add(item.ToObject<T>());

      return result;
    }

    public static List<JsonWrapper> FromList(IEnumerable items) {
      var result = new List<JsonWrapper>();

      if (items == null)
        return result;

      foreach (object item in items)
        result.Add(JsonWrapper.FromObject(item));

      return result;
    }

    public static Dictionary<string, JsonWrapper> FromListToDictionary(IEnumerable items, string key) {
      var result = new Dictionary<string, JsonWrapper>();

      if (items == null)
        return result;

      foreach (object item in items) {
        var wrapper = FromObject(item);
        result[wrapper.Get(key)] = wrapper;
      }

      return result;
    }
    #endregion

    #region Extra Data
    private object fExtraData;
    public T GetExtraData<T>() {
      return (T)fExtraData;
    }

    public JsonWrapper SetExtraData(object extraData) {
      this.fExtraData = extraData;
      return this;
    }
    #endregion
  }

  public class JsonWrapperPropertyInfo : PropertyInfo {
    private readonly string fKey;
    private readonly Type fType;

    public JsonWrapperPropertyInfo(string key, Type type) {
      this.fKey = key;
      this.fType = type;
    }

    // Isn't used in my code
    public override PropertyAttributes Attributes {
      get {
        return new PropertyAttributes();
      }
    }

    public override bool CanRead { get { return true; } }
    public override bool CanWrite { get { return true; } }

    public override string Name { get { return fKey; } }

    public override Type DeclaringType { get { return fType; } }

    public override Type PropertyType { get { return fType; } }
    public override Type ReflectedType { get { return fType; } }

    public override MethodInfo[] GetAccessors(bool nonPublic) {
      throw new NotImplementedException();
    }

    public override object[] GetCustomAttributes(bool inherit) {
      throw new NotImplementedException();
    }

    public override object[] GetCustomAttributes(Type attributeType, bool inherit) {
      throw new NotImplementedException();
    }

    public override MethodInfo GetGetMethod(bool nonPublic) {
      throw new NotImplementedException();
    }

    public override ParameterInfo[] GetIndexParameters() {
      throw new NotImplementedException();
    }

    public override MethodInfo GetSetMethod(bool nonPublic) {
      throw new NotImplementedException();
    }

    public override object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture) {
      return (obj as JsonWrapper).GetValue(fKey);
    }

    public override bool IsDefined(Type attributeType, bool inherit) {
      throw new NotImplementedException();
    }

    public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture) {
      (obj as JsonWrapper).SetValue(fKey, value);
    }
  }
}
