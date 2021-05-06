using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using DatusArator.Core.Util;

namespace DatusArator.Core.Json {
  public class JsonWrapperSerializer : IComparer<string> {
    public static bool WRITE_EMPTY_STRINGS = false;

    private List<string> jsonMap;
    private Dictionary<string, List<string>> arraysOfObject;

    private readonly Dictionary<string, TypedBasicObject> values;
    private readonly Dictionary<string, int> arrays;
    private readonly List<string> hiddenFields;

    public JsonWrapperSerializer(Dictionary<string, TypedBasicObject> values, Dictionary<string, int> arrays, List<string> hiddenFields) {
      this.values = values;
      this.arrays = arrays;
      this.hiddenFields = hiddenFields;
    }

    public string BuildJsonString(List<string> map) {
      CompileMap(map);

      StringBuilder outString = new StringBuilder();

      using (JsonWriter gen = new JsonTextWriter(new StringWriter(outString))) {
        gen.StringEscapeHandling = StringEscapeHandling.EscapeNonAscii; 
        gen.WriteStartObject();

        var currentStack = new List<string>();
        foreach (var key in jsonMap)
          currentStack = ToJsonStringWriteValue(gen, "", key, currentStack);

        foreach (var _ in currentStack)
          gen.WriteEndObject();

        gen.WriteEndObject();
      }

      return outString.ToString();
    }

    private List<string> ToJsonStringWriteValue(JsonWriter gen, string path, string key, List<string> currentStack) {
      string lookupKey = StringUtils.Concat(path, key, ".");

      if (!arrays.ContainsKey(lookupKey) && !values.ContainsKey(lookupKey))
        return currentStack;

      string outKey;
      if (key.IndexOf('.') > 0) {
        List<string> newStack = key.Split('.').ToList();
        outKey = newStack[newStack.Count - 1];
        newStack.RemoveAt(newStack.Count - 1);

        Boolean unmatched = false;
        for (int i = 0; i < newStack.Count; i++) {
          string checkValue = newStack[i];
          if (unmatched || (currentStack.Count <= i) || !checkValue.Equals(currentStack[i])) {
            if (!unmatched) {
              for (int j = i; j < currentStack.Count; j++)
                gen.WriteEndObject();
            }

            unmatched = true;

            gen.WritePropertyName(checkValue);
            gen.WriteStartObject();
          }
        }

        if (!unmatched)
          for (int i = newStack.Count; i < currentStack.Count; i++)
            gen.WriteEndObject();

        currentStack = newStack;
      } else {
        outKey = key;

        foreach (var _ in currentStack)
          gen.WriteEndObject();

        currentStack = new List<string>();
      }

      if (arraysOfObject.ContainsKey(StringUtils.RemoveArrayReferences(lookupKey))) {
        int max = arrays[lookupKey];
        
        gen.WritePropertyName(outKey);
        gen.WriteStartArray();

        for (int i = 0; i < max; i++) {
          gen.WriteStartObject();

          var props = arraysOfObject[StringUtils.RemoveArrayReferences(lookupKey)];

          var nestedStack = new List<string>();
          foreach (var prop in props) {
            nestedStack = ToJsonStringWriteValue(gen, lookupKey + '[' + i + ']', prop, nestedStack);
          }

          foreach (var item in nestedStack)
            gen.WriteEndObject();

          gen.WriteEndObject();
        }

        gen.WriteEndArray();
      } else if (arrays.ContainsKey(lookupKey)) {
        WriteJsonArray(gen, lookupKey, outKey);
      } else {
        TypedBasicObject raw = GetValue(lookupKey);

        bool keepValue = WRITE_EMPTY_STRINGS ? raw.IsNotNull() : raw.HasValue();

        if (keepValue) {
          gen.WritePropertyName(outKey);
          WriteJsonValue(gen, raw);
        }
      }

      return currentStack;
    }

    private void WriteJsonArray(JsonWriter gen, string lookupKey, string outKey) {
      int max = arrays[lookupKey];

      if (outKey != null)
        gen.WritePropertyName(outKey);
      
      gen.WriteStartArray();

      for (int i = 0; i < max; i++) {
        string subKey = lookupKey + '[' + i + ']';
        if (arrays.ContainsKey(subKey)) {
          WriteJsonArray(gen, subKey, null);
        } else {
          var raw = GetValue(subKey);

          if (raw == null)
            gen.WriteNull();
          else {
            WriteJsonValue(gen, raw);
          }
        }
      }

      gen.WriteEndArray();
    }

    private void WriteJsonValue(JsonWriter gen, TypedBasicObject raw) {
      switch (raw.TBOType) {
        case TypedBasicObjectType.DATETIME:
        case TypedBasicObjectType.LONG:
          gen.WriteValue(raw.GetAsLong()); 
          break;
        case TypedBasicObjectType.STRING:
        case TypedBasicObjectType.INTEGER:
        case TypedBasicObjectType.DECIMAL:
        case TypedBasicObjectType.DOUBLE:
        case TypedBasicObjectType.BOOLEAN: 
          gen.WriteValue(raw.Value); 
          break;
      }
    }

    #region Compile Map
    private void CompileMap(List<string> originalMap) {
      var flattenedArrays = GetAllArrays();

      if ((originalMap == null) || (originalMap.Count == 0) || (originalMap[0].Equals("*") && (originalMap.Count == 1)))
        originalMap = GetAllFields(null);

      arraysOfObject = new Dictionary<string, List<string>>();
      jsonMap = new List<string>();

      foreach (var item in originalMap) {
        bool found = false;

        int wildcardPos = item.IndexOf('*');
        if (wildcardPos >= 0) {
          found = true;
          
          var partialKey = item.Substring(0, item.Length - 2);
          var wildCardFields = GetAllFields(partialKey);

          foreach (var key in wildCardFields) {
            AddToMap(key);
            CheckForArray(flattenedArrays, key);
          }
        } else {
          found = CheckForArray(flattenedArrays, item);
        }

        if (!found)
          AddToMap(item);
      }
    }

    private bool CheckForArray(List<string> flattenedArrays, string item) {
      bool found = false;
      foreach (var array in flattenedArrays) {
        if (item.StartsWith(array + '.')) {
          found = true;

          if (!arraysOfObject.ContainsKey(array)) {
            AddToMap(array);
            arraysOfObject[array] = new List<string>();
          }

          var currentProps = arraysOfObject[array];

          string rest = item.Substring(array.Length + 1);
          if (rest.IndexOf('.') > 0) {
            var subRest = rest.Split('.');
            string subValue = "";
            foreach (var subRestItem in subRest) {
              subValue = StringUtils.Concat(subValue, subRestItem, ".");
              if (flattenedArrays.Contains(array + "." + subValue))
                break;
            }

            if (!currentProps.Contains(subValue))
              AddItemToProps(currentProps, subValue);
          } else {
            AddItemToProps(currentProps, rest);
          }
        }
      }

      return found;
    }
    #endregion

    #region Compile Map Helpers
    private void AddToMap(string item) {
      if (string.IsNullOrEmpty(item))
        return;

      bool found = false;
      if (hiddenFields != null) 
        foreach (var key in hiddenFields)
          if (item.Equals(key)) {
            found = true;
            break;
          }

      if (!found)
        jsonMap.Add(item);
    }

    private void AddItemToProps(List<string> currentProps, string item) {
      if ("id".Equals(item))
        currentProps.Insert(0, item);
      else if ("name".Equals(item)) {
        if ((currentProps.Count > 0) && "id".Equals(currentProps[0]))
          currentProps.Insert(1, item);
        else
          currentProps.Insert(0, item);
      } else
        currentProps.Add(item);
    }

    private List<string> GetAllFields(string prefix) {
      var result = new List<string>();

      foreach (var key in values.Keys) {
        if ((prefix == null) || key.StartsWith(prefix))
          result.Add(StringUtils.RemoveArrayReferences(key));
      }

      result.Sort(this);

      return StringUtils.RemoveDuplicates(result);
    }

    private List<string> GetAllArrays() {
      var result = new List<string>();

      foreach (var key in arrays.Keys)
        result.Add(StringUtils.RemoveArrayReferences(key));

      result.Sort(this);

      return StringUtils.RemoveDuplicates(result);
    }

    private TypedBasicObject GetValue(string key) {
      if (values.ContainsKey(key))
        return values[key];

      return null;
    }

    public int Compare(string x, string y) {
      if (x.Equals(y))
        return 0;

      if ("id".Equals(x))
        return -1;
      if ("id".Equals(y))
        return 1;

      if ("name".Equals(x))
        return -1;
      if ("name".Equals(y))
        return 1;

      if (x.StartsWith("meta.")) {
        if (y.StartsWith("meta."))
          return Compare(x.Substring(5), y.Substring(5));
        else
          return -1;
      } else if (y.StartsWith("meta."))
        return 1;

      return x.CompareTo(y);
    }
    #endregion
  }
}
