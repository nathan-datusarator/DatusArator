using DatusArator.Core.DotPath;
using DatusArator.Core.Json;
using DatusArator.Core.Util;
using System;
using System.Collections.Generic;
using System.Data;

namespace DatusArator.Core.DAO {
  public class Schema : DotPathObject {
    public string TableName { get; set; }
    public string DisplayName { get; set; }
    public string[] Shard { get; set; } = new string[0];
    public string[] KeyFields { get; set; } = new string[0];

    public List<SchemaField> Fields { get; set; } = new List<SchemaField>();

    public string PrimaryKey { get; set; }
    public SchemaField GetPrimaryKey() { return FindField(PrimaryKey) ?? Fields?[0]; }

    public List<EditGroupDef> EditGroupDefs { get; set; } = new List<EditGroupDef>();
    public int? EditFormWidth { get; set; }
    public int? EditFormHeight { get; set; }

    public Schema() : base() {
      ClassName = "Schema";
    }

    public void Build(DataTable dataTable) {
      bool foundFieldName = false;

      object[] header = new object[dataTable.Columns.Count];
      int col = 0;
      foreach (DataColumn column in dataTable.Columns) {
        header[col++] = column.ColumnName;
        if (column.ColumnName.Equals("Field Name", StringComparison.CurrentCultureIgnoreCase)) {
          foundFieldName = true;
        }
      }

      if (!foundFieldName)
        return;

      var builder = new SchemaBuilder(this);

      int order = 1;
      var rows = dataTable.Rows;
      for (int i = 0; i < rows.Count; i++)
        order = LoadSchemaAddField(builder, header, rows[i].ItemArray, order);

      Fields.Sort();
    }

    private static int LoadSchemaAddField(SchemaBuilder builder, object[] header, object[] itemArray, int order) {
      var temp = new JsonWrapper();

      bool visibleSeen = false;
      for (int i = 0; i < header.Length; i++) {
        if (i >= itemArray.Length)
          break;

        var field = header[i].ToString();
        visibleSeen = visibleSeen || "Visible".Equals(field, StringComparison.InvariantCultureIgnoreCase);

        var value = itemArray[i]?.ToString();
        if (string.IsNullOrEmpty(value) || "NULL".Equals(value))
          continue;

        temp[field] = value;
      }

      int? itemOrder = temp.GetAsInt("Order", null);

      if (temp.GetAsBool("Exclude"))
        return order;

      builder.Field(temp.Get("Field Name")).Display(temp.Get("Display Name")).Type(temp.Get("Type")?.ToUpper())
        .GridDisplayFormat(temp.Get("Display Format")).GridGroupFormat(temp.Get("Group Format")).Notes(temp.Get("Notes"))
        .GridInterval(temp.GetAsInt("Interval", null))
        .GridIntervalMax(temp.GetAsInt("Interval Max", null))
        .GridIntervalFormat(temp.Get("Interval Format"));

      if (!visibleSeen)
        builder.Order(itemOrder);
      else
        builder.Order(temp.GetAsBool("Visible") ? order++ : -1);

      if (temp.GetAsBool("Primary"))
        builder.PrimaryKey();

      if (builder.HasField())
        builder.Commit();

      return order;
    }

    public void BuildFromData(DataTable dataTable) {
      var rows = dataTable.Rows;

      object[] header = new object[dataTable.Columns.Count];
      int col = 0;
      foreach (DataColumn column in dataTable.Columns)
        header[col++] = column.ColumnName;

      var builder = new SchemaBuilder(this);

      for (int i = 0; i < header.Length; i++) {
        builder.Field(header[i].ToString()).Type(BuildFromDataType(rows, i)).Order(i + 1);
        builder.Commit();
      }

      Fields.Sort();
    }

    private string BuildFromDataType(DataRowCollection rows, int col) {
      string result = null;
      for (int i = 0; i < rows.Count; i++) {
        var cell = rows[i][col];
        if ((cell == null) || (cell.ToString() == "") || (cell.ToString() == "NULL"))
          continue;

        var check = MapType(cell.GetType());
        if (result == null)
          result = check;
        else if (check != result)
          return "STRING";
      }

      return result ?? "STRING";
    }

    private string MapType(Type type) {
      switch (type.ToString()) {
        case "System.DateTime": return "DATE";
        case "System.Double": return "DOUBLE";
        case "System.String": return "STRING";
        default:
          Console.WriteLine("Unknown type in Schema.MapType: " + type.ToString());
          return "STRING";
      }
    }

    public Schema SubSchema(string linkName) {
      var result = Clone<Schema>();

      foreach (var field in result.Fields) {
        field.Display = field.Display ?? StringUtils.SplitCamelCase(field.Field);
        field.Field = linkName + "." + field.Field;
      }

      return result;
    }

    public SchemaField FindField(string fieldName) {
      if (string.IsNullOrEmpty(fieldName))
        return null;

      foreach(var field in Fields) {
        if (field.Field.Equals(fieldName, StringComparison.InvariantCultureIgnoreCase))
          return field;
      }

      return null;
    }
  }

  public class EditGroupDef {
    public string Caption { get; set; }
    public string ColDefs { get; set; }
    public string RowDefs { get; set; }
    public bool CaptionHidden { get; set; }

    public EditGroupDef() { }
    public EditGroupDef(string caption, string colDefs = null, string rowDefs = null, bool captionHidden = false) {
      Caption = caption;
      ColDefs = colDefs;
      RowDefs = rowDefs;
      CaptionHidden = captionHidden;
    }
  }
}
