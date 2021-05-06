using DatusArator.Core.DAO;
using DatusArator.Core.DotPath;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace DatusArator.Core.Json {
  public static class JsonUtils {
    public static string ToJsonArray(string value) {
      var values = new List<String> { value };
      return ToJsonArray(values);
    }

    public static String ToJsonArray(object[] values) {
      JArray array = new JArray((values != null) ? (from value in values select value) : null);
      return array.ToString(Formatting.None);
    }

    public static T Clone<T>(T value) {
      var json = ObjectToJson(value);
      return JsonToObject<T>(json);
    }

    public static List<T> CloneList<T>(List<T> values) {
      var result = new List<T>();
      foreach (T value in values)
        result.Add(Clone(value));
     
      return result;
    }

    public static String ToJsonArray(List<String> values) {
      JArray array = new JArray((values != null) ? (from value in values select value) : null);
      return array.ToString(Formatting.None);
    }

    public static string ToJsonArray(List<JsonWrapper> values, bool addNewLines = false) {
      StringBuilder builder = new StringBuilder();
      builder.Append("[");
      if (values != null) {
        foreach (var value in values) {
          if (builder.Length > 1)
            builder.Append(",");

          if (addNewLines)
            builder.Append(Environment.NewLine);

          builder.Append(value.ToJsonString());
        }
      }
      builder.Append("]");

      return builder.ToString();
    }

    public static List<JsonWrapper> ParseJsonArray(String json) {
      var result = new List<JsonWrapper>();

      JArray root = JArray.Parse(json);
      foreach (var child in root.Children()) {
        var wrapper = new JsonWrapper();
        wrapper.ParseJson(child);

        result.Add(wrapper);
      }

      return result;
    }

    #region Serialization Tools
    private static JsonSerializerSettings fSettings;
    public static JsonSerializerSettings Settings {
      get {
        if (fSettings == null) {
          fSettings = new JsonSerializerSettings {
            DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
            StringEscapeHandling = StringEscapeHandling.EscapeNonAscii,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore
          };
        }

        return fSettings;
      }
    }

    public static string FormatJsonTabbed(string json) {
      return JToken.Parse(json).ToString(Formatting.Indented);
    }

    public static string ObjectToJson(object o, bool indented = false) {
      var result = JsonConvert.SerializeObject(o, Settings);
      return indented ? FormatJsonTabbed(result) : result;
    }

    public static T JsonToObject<T>(string json) {
      if (string.IsNullOrEmpty(json))
        return default;

      var result = JsonConvert.DeserializeObject<T>(json, Settings);
      if (result is DotPathObject)
        (result as DotPathObject).AfterDeserialize();
      if (result is ICollection) {
        foreach (var item in (result as ICollection))
          if (item is DotPathObject)
            (item as DotPathObject).AfterDeserialize();
      }

      return result;
    }

    public static void PopulateObject(string json, object target) {
      JsonConvert.PopulateObject(json, target, Settings);
    }
    #endregion

    public static List<JsonWrapperChange> Compare(object source, object target) {
      var sourceJson = new JsonWrapper(ObjectToJson(source));
      var targetJson = new JsonWrapper(ObjectToJson(target));

      sourceJson.TrackChanges = true;
      sourceJson.CopyDataFrom(targetJson, "");

      return new List<JsonWrapperChange>(sourceJson.GetChanges().Values);
    }

    #region Data Set Conversion
    public static DataSet ConvertToDataSet(IEnumerable<JsonWrapper> wrappers) {
      DataSet dataset = new DataSet();
      DataTable table = new DataTable();
      dataset.Tables.Add(table);

      Schema schema = null;
      foreach (var wrapper in wrappers) {
        if (schema == null)
          schema = BuildColumns(wrapper.Schema, table);

        var row = table.NewRow();
        foreach (var field in schema.Fields) {
          if (wrapper.HasValue(field.Field))
            row[field.Field] = GetWrapperValue(wrapper, field);
        }
      }

      return dataset;
    }

    private static Schema BuildColumns(Schema schema, DataTable table) {
      foreach (var field in schema.Fields) {
        table.Columns.Add(new DataColumn() { Caption = field.Caption, DataType = MapSchemaFieldType(field.Type), ColumnName = field.Field });
      }

      return schema;
    }

    private static object GetWrapperValue(JsonWrapper wrapper, SchemaField field) {
      switch (field.Type) {
        case "BOOLEAN":
        case "BOOL":
          return wrapper.GetAsBool(field.Field);
        case "MONEY":
        case "DECIMAL":
        case "FLOAT":
        case "DOUBLE":
          return wrapper.GetAsDecimal(field.Field, null);
        case "INT":
        case "INTEGER":
          return wrapper.GetAsInt(field.Field, null);
        case "LONG":
          return wrapper.GetAsLong(field.Field, null);
        case "DATE":
        case "DATETIME":
          return wrapper.GetAsDate(field.Field, null);
        default:
          return wrapper[field.Field];
      }
    }

    private static Type MapSchemaFieldType(string type) {
      switch (type) {
        case "BOOLEAN":
        case "BOOL":
          return typeof(bool);
        case "MONEY":
        case "DECIMAL":
        case "FLOAT":
        case "DOUBLE":
          return typeof(decimal);
        case "INT":
        case "INTEGER":
          return typeof(int);
        case "LONG":
          return typeof(long);
        case "DATE":
        case "DATETIME":
          return typeof(DateTime);
        default:
          return typeof(string);
      }
    }
    #endregion

  }
}
