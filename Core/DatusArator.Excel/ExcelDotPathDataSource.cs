using DatusArator.Core;
using DatusArator.Core.DAO;
using DatusArator.Core.DotPath.DataSource;
using DatusArator.Core.Json;
using DatusArator.Core.Util;
using System;
using System.Collections.Generic;
using System.Data;

namespace DatusArator.Excel {
  public class ExcelDotPathDataSource : DictionaryDotPathDataSource<JsonWrapper> {
    public ExcelDotPathDataSource(string fileName, string schemaFile, string displayName, string tableName, string keyField, params string[] indexes) : base(keyField, indexes) {
      this.LinkType = typeof(JsonWrapper);
      this.DisplayName = displayName;
      this.ClassName = tableName;

      var progressPrefix = "[" + DisplayName + "]";
      GeneralUtils.GlobalProgress?.UpdateProgress(progressPrefix, 0, 100);

      Schema = new Schema {
        TableName = tableName
      };

      if (!string.IsNullOrEmpty(schemaFile)) {
        var schemaData = ExcelUtils.ToDataSet(schemaFile);
        if (schemaData != null)
          Schema.Build(schemaData.Tables[0]);

        if ((Schema.Fields.Count > 0) && (keyField == "XX"))
          return;
      }

      GeneralUtils.GlobalProgress?.UpdateProgress(progressPrefix + " Reading File", 0, 100);
      var dataSet = ExcelUtils.ToDataSet(fileName);
      if ((dataSet == null) || (dataSet.Tables.Count < 1))
        return;

      GeneralUtils.GlobalProgress?.UpdateProgress(progressPrefix + " Updating Schema", 0, 100);

      int schemaTable = -1;
      if (Schema.Fields.Count == 0) {
        int i = 0;
        foreach (DataTable table in dataSet.Tables) {
          if (table.TableName == "Schema") {
            Schema.Build(table);
            schemaTable = i;
            break;
          }

          i++;
        }

        // Still none
        if (Schema.Fields.Count == 0) {
          Schema.BuildFromData(dataSet.Tables[0]);
        }
      }

      if (keyField == "XX")
        return;

      if ((schemaTable == 0) && (dataSet.Tables.Count == 1))
        return;

      var schemaDict = new Dictionary<string, SchemaField>();
      foreach (var field in Schema.Fields)
        schemaDict[field.Field] = field;

      var dataTable = dataSet.Tables[(schemaTable == 0) ? 1 : 0];
      var rows = dataTable.Rows;
      GeneralUtils.GlobalProgress?.UpdateProgress(progressPrefix + " Reading Data : 0 of " + (rows.Count - 1), 0, rows.Count - 1);

      object[] header = new object[dataTable.Columns.Count];
      int col = 0;
      foreach (DataColumn column in dataTable.Columns) {
        header[col++] = column.ColumnName;
      }

      for (int i = 0; i < rows.Count; i++) {
        var wrapper = new JsonWrapper {
          ClassName = tableName
        };

        for (int j = 0; j < header.Length; j++) {
          var fieldName = header[j].ToString();
          wrapper[fieldName] = CheckValueType(schemaDict, fieldName, rows[i][j]);
        }

        Put(wrapper);

        if (i % 100 == 0)
          GeneralUtils.GlobalProgress?.UpdateProgress(progressPrefix + " Reading Data : " + i + " of " + (rows.Count - 1), i, rows.Count - 1);
      }
    }

    public object CheckValueType(Dictionary<string, SchemaField> dict, string fieldName, object value) {
      if ((value == null) || (value == DBNull.Value) || ((value as string) == "") || ((value as string) == "NULL")) return null;
      if (!dict.ContainsKey(fieldName)) return value;

      var field = dict[fieldName];
      var destType = field.ActualType;
      if (destType == null) return value;

      return GeneralUtils.ChangeType(value, destType);
    }
  }
}
