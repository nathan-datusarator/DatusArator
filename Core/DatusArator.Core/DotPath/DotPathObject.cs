using DatusArator.Core.DAO;
using DatusArator.Core.DotPath.Join;
using DatusArator.Core.Json;
using DatusArator.Core.Util;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace DatusArator.Core.DotPath {
  public class DotPathObject {
    // Do not make into a property. Messes many things Up
    [JsonIgnore]
    public string ClassName;   // Entirely my own name.  Used in Join system for generic objects like JsonWrapper

    [JsonIgnore]
    public DotPathObject JoinParent;

    [JsonIgnore]
    public virtual object this[string propertyName] {
      get { return GetValue(propertyName); }
      set { SetValue(propertyName, value); }
    }

    private Schema fSchema;
    [JsonIgnore]
    public virtual Schema Schema {
      get {
        if (fSchema == null) {
          var property = GetType().GetProperty("StaticSchema", BindingFlags.Static | BindingFlags.Public);
          if (property == null)
            property = GetType().GetProperty("STATIC_SCHEMA", BindingFlags.Static | BindingFlags.Public);
          fSchema = property?.GetValue(null) as Schema;
        }

        return fSchema;
      }
    }

    public virtual void AfterDeserialize() {
    }

    public virtual T Clone<T>() where T : new() {
      var json = JsonUtils.ObjectToJson(this);
      return JsonUtils.JsonToObject<T>(json);
    }

    public virtual string ToJsonString(bool indented = false) {
      return JsonUtils.ObjectToJson(this, indented);
    }

    public string MergeData(string source, Dictionary<string, string> keyMap = null) {
      var matches = Regex.Matches(source, @"{.*?}").OfType<Match>().Select(m => m.Groups[0].Value).ToArray();

      var result = source;
      foreach (var match in matches) {
        string key = match.Substring(1, match.Length - 2);
        if (keyMap?.ContainsKey(key) ?? false)
          key = keyMap[key];

        string value = (this[key]?.ToString() ?? "");
        result = result.Replace(match, value);
      }

      return result;
    }

    #region Evaluate
    public virtual object Evaluate(string exp) {
      string filterExp = null;
      var index = exp.IndexOf(":");
      if (index > 0) {
        filterExp = exp.Substring(index + 1);
        filterExp = filterExp.Substring(0, filterExp.Length);

        exp = exp.Substring(0, index);
      }

      // Find Parent Object
      object root = this;
      index = exp.LastIndexOf(".");
      if (index > 0) {
        var rootPath = exp.Substring(0, index);
        exp = exp.Substring(index + 1);

        root = GetValue(rootPath);
        if (root == null)
          return null;
      }

      var filter = string.IsNullOrEmpty(filterExp) ? null : new DotPathFilter(filterExp, null);

      if (root is IList list) {
        var result = new List<string>();
        foreach (object obj in list) {
          var value = Evaluate(obj, exp, filter)?.ToString();
          if (value != null && !result.Contains(value))
            result.Add(value);
        }

        return result.Count > 0 ? StringUtils.Concat(result, ", ") : null;
      } else {
        return Evaluate(root, exp, filter);
      }
    }

    private object Evaluate(object root, string exp, DotPathFilter filter) {
      // Evaluate Filter
      if (filter != null) {
        var filterResult = filter.Evaluate(root);
        if (!filterResult || filter.MissingValues)
          return null;
      }

      if (exp.StartsWith("'") || exp.StartsWith("\""))
        return exp.Substring(1, exp.Length - 2);

      object outValue;
      if (root is DotPathObject)
        outValue = (root as DotPathObject).GetValue(exp);
      else
        outValue = GetValue(root, exp);

      if (outValue is IList list) {
        string result = "";
        foreach (object obj in list)
          result = StringUtils.Concat(result, obj.ToString(), ", ");

        return string.IsNullOrEmpty(result) ? null : result;
      }

      return outValue;
    }
    #endregion

    #region Get / Set
    public virtual object GetValue(string propertyName) {
      var index = propertyName.IndexOf(".");
      if (index > 0) {
        if (GetChildValue(propertyName.Substring(0, index), propertyName.Substring(index + 1), out object result))
          return result;
      } else if (GetChildValue(propertyName, null, out object result))
        return result;

      return GetValue(this, propertyName);
    }

    public static object GetValue(object root, string propertyName) {
      PropertyInfo property = GetPropertyInfo(root, propertyName, false, out object outRoot, out int? outIndex);

      if (property == null) {
        return (root as DotPathObject)?.GetChildObject(propertyName);
      } else {
        if (outIndex != null) {
          var values = (IList)property.GetValue(outRoot);
          return (values != null) && (values.Count > outIndex) ? values[outIndex.Value] : null;
        } else {
          return property.GetValue(outRoot, null);
        }
      }
    }

    public virtual void SetValue(string propertyName, object value) {
      var index = propertyName.IndexOf(".");
      if ((index > 0) && SetChildValue(propertyName.Substring(0, index), propertyName.Substring(index + 1), value))
        return;

      PropertyInfo property = GetPropertyInfo(propertyName, true, out object outRoot, out int? outIndex);
      if (property == null)
        return;

      if (outIndex != null) {
        var values = (IList)property?.GetValue(outRoot);

        if (values == null) {
          property.SetValue(outRoot, Activator.CreateInstance(property.PropertyType));
          values = (IList)property?.GetValue(outRoot);
        }

        for (int i = values.Count; i < outIndex; i++)
          values.Add(null);

        var listType = property.PropertyType.GetGenericArguments()[0];
        values.Add(GeneralUtils.ChangeType(value, listType));
      } else {
        property?.SetValue(outRoot, GeneralUtils.ChangeType(value, property.PropertyType), null);
      }
    }
    #endregion

    #region Reflection
    public virtual Type GetType(string propertyName) {
      PropertyInfo property = GetPropertyInfo(propertyName, false, out object _, out int? outIndex);

      if ((property != null) && (outIndex != null)) {
        return property.PropertyType.GetGenericArguments()[0];
      } else {
        return property?.PropertyType;
      }
    }

    public virtual PropertyInfo GetPropertyInfo(string propertyName, bool createSubObjects, out object outRoot, out int? outIndex) {
      return GetPropertyInfo(this, propertyName, createSubObjects, out outRoot, out outIndex);
    }

    public static PropertyInfo GetPropertyInfo(object root, string propertyName, bool createSubObjects, out object outRoot, out int? outIndex) {
      outRoot = root;
      outIndex = null;

      if (root == null)
        return null;

      var index = propertyName.IndexOf(".");
      if (index > 0) {
        var currentProperty = propertyName.Substring(0, index);
        int? currentPropertyIndex = null;

        if (currentProperty.EndsWith("]")) {
          var arrayIndex = currentProperty.IndexOf("[");

          var arrayData = currentProperty.Substring(arrayIndex);

          currentPropertyIndex = Convert.ToInt32(arrayData.Substring(1, arrayData.Length - 2));
          currentProperty = currentProperty.Substring(0, arrayIndex);
        }

        PropertyInfo info = FindProperty(root, currentProperty);
        if (info == null) {
          return (root as DotPathObject)?.GetChildPropertyInfo(currentProperty, propertyName.Substring(index + 1), createSubObjects, out outRoot, out outIndex);
        } else {
          var current = info.GetValue(root, null);
          if ((current == null) && createSubObjects) {
            var subObject = Activator.CreateInstance(info.PropertyType);
            info.SetValue(root, subObject, null);

            current = info.GetValue(root, null);
          }

          if ((currentPropertyIndex != null) && (current != null)) {
            var values = (IList)current;
            if (createSubObjects) {
              for (int i = values.Count; i <= currentPropertyIndex; i++)
                values.Add(null);
            }

            current = ((values != null) && (values.Count > currentPropertyIndex)) ? values[currentPropertyIndex.Value] : null;

            if ((current == null) && createSubObjects) {
              current = Activator.CreateInstance(info.PropertyType.GetGenericArguments()[0]);
              values[currentPropertyIndex.Value] = current;
            }
          }

          return GetPropertyInfo(current, propertyName.Substring(index + 1), createSubObjects, out outRoot, out outIndex);
        }
      } else {
        if (propertyName.EndsWith("]")) {
          var arrayIndex = propertyName.IndexOf("[");

          var arrayData = propertyName.Substring(arrayIndex);

          outIndex = Convert.ToInt32(arrayData.Substring(1, arrayData.Length - 2));
          propertyName = propertyName.Substring(0, arrayIndex);
        }

        return FindProperty(root, propertyName);
      }
    }

    public static PropertyInfo FindProperty(object root, string currentProperty) {
      PropertyInfo info = root.GetType().GetProperty(currentProperty);
      if (info == null)
        foreach (var property in root.GetType().GetProperties())
          if (property.Name.Equals(currentProperty, StringComparison.InvariantCultureIgnoreCase)) {
            info = property;
            break;
          }

      return info;
    }
    #endregion

    #region Child Objects
    private DotPathObject GetChildObject(string key) {
      var result = DotPathJoins.Get(this, key);
      if (result != null)
        result.JoinParent = this;
      return result;
    }

    public bool GetChildValue(string child, string key, out object value) {
      var childObject = GetChildObject(child);
      if (childObject != null) {
        if (string.IsNullOrEmpty(key))
          value = childObject;
        else
          value = childObject.GetValue(key);
        return true;
      } else {
        value = null;
        return false;
      }
    }

    public bool SetChildValue(string child, string key, object value) {
      var childObject = GetChildObject(child);
      if (childObject != null) {
        childObject.SetValue(key, value);
        return true;
      } else {
        return false;
      }
    }

    protected PropertyInfo GetChildPropertyInfo(string child, string propertyName, bool createSubObjects, out object outRoot, out int? outIndex) {
      outRoot = null;
      outIndex = null;

      return GetChildObject(child)?.GetPropertyInfo(propertyName, createSubObjects, out outRoot, out outIndex);
    }
    #endregion
  }
}
