using System;
using Newtonsoft.Json;
using DatusArator.Core.DotPath;
using DatusArator.Core.Util;
using System.Collections.Generic;

namespace DatusArator.Core.DAO {
  public class SchemaField : DotPathObject, IComparable<SchemaField> {
    public string Field { get; set; }
    public string Display { get; set; }
    public string Type { get; set; } = "STRING";
    public int? Order { get; set; }
    public string Sequence { get; set; }
    public string Notes { get; set; }

    public bool IsArray { get; set; }
    public bool ExportHide { get; set; }

    public GridOptions Grid { get; set; } = new GridOptions();
    public EditOptions Edit { get; set; } = new EditOptions();

    private Type fActualType;
    [JsonIgnore]
    public Type ActualType {
      get {
        if (fActualType == null)
          fActualType = MapFieldType(Type);

        return fActualType;
      }
    }

    #region Helpers
    public override string ToString() {
      return Caption + (!string.IsNullOrEmpty(Display) ? " [" + Field + "]" : "");
    }

    [JsonIgnore]
    public string Caption { get { return string.IsNullOrEmpty(Display) ? StringUtils.SplitCamelCase(Field) : Display; } }

    [JsonIgnore]
    public bool ShowInGrid { get { return Grid?.Visible ?? (Order >= 0); } }
    [JsonIgnore]
    public int VisibleIndex { get { return ShowInGrid ? (Order ?? 100) : -1; } }

    public SchemaField() : base() {
      ClassName = "SchemaField";
    }

    public bool Matches(string key) {
      if (key == null) return false;
      return key.Equals(Field, StringComparison.InvariantCultureIgnoreCase) || key.Equals(Display, StringComparison.InvariantCultureIgnoreCase);
    }

    public int CompareTo(SchemaField other) {
      int order1 = VisibleIndex;
      int order2 = other.VisibleIndex;

      if (order1 == order2)
        return Caption.CompareTo(other.Caption);
      else if (order1 == -1)
        return 1;
      else if (order2 == -1)
        return -1;
      else
        return order1.CompareTo(order2);
    }

    private Type MapFieldType(string type) {
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
        case "PHOTO":
        case "IMAGE":
          return null;
        case "GUID":
        case "STRING":
          return typeof(string);
        default:
          return typeof(string);
      }
    }
    #endregion
  }

  public class GridOptions {
    public bool? Visible { get; set; }
    public int? Width { get; set; }
    public string DisplayFormat { get; set; }
    public string GroupFormat { get; set; }
    public string SortOrder { get; set; }

    public int? Interval { get; set; }
    public int? IntervalMax { get; set; }
    public string IntervalFormat { get; set; }
  }

  public class EditOptions {
    public bool Required { get; set; }
    public bool ReadOnly { get; set; }
    public bool NotVisible { get; set; }
    public bool Disabled { get; set; }

    public string Group { get; set; }      // Group / Section
    public string RowColSpan { get; set; }      // Display order in that section.  Might be Col-Row
    public int? Width { get; set; }
    public int? Height { get; set; }
    public bool HideCaption { get; internal set; }

    public string Type { get; set; }       // Edit Type.  Data Type is Above
    public string Value { get; set; }      // Default Value

    public string Validate { get; set; }   // Validationg Rule (when I build that engine)
    public string Format { get; set; }
    public int? Warn { get; set; }

    public int? Min { get; set; }
    public int? Max { get; set; }

    public string MapType { get; set; }
    public string MapSrc { get; set; }
    public string MapFilter { get; set; }

    public string Link { get; set; }
    public string LinkSrc { get; set; }
    public string LinkKey { get; set; }

    public Dictionary<string, EditMapValue> MapValues { get; set; } = new Dictionary<string, EditMapValue>();

    public void AddMapValue(string key, string caption, bool notVisible = false) {
      if (MapValues == null)
        MapValues = new Dictionary<string, EditMapValue>();
      MapValues[key] = new EditMapValue(key, caption, notVisible);
    }
  }

  public class EditMapValue {
    public string Key { get; set; }
    public string Caption { get; set; }
    public bool NotVisible { get; set; }

    public EditMapValue() { }
    public EditMapValue(string key, string caption, bool notVisible = false) {
      this.Key = key;
      this.Caption = caption;
      this.NotVisible = notVisible;
    }
  }
}
