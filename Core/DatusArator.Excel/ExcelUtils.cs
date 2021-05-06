using ExcelDataReader;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace DatusArator.Excel {
  public class ExcelUtils {
    public static DataSet ToDataSet(string fileName, bool useHeaderRow = true) {
      if (!File.Exists(fileName))
        return null;

      bool isXLSX = fileName.EndsWith("xlsx");

      using (FileStream stream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
        IExcelDataReader excelReader = isXLSX ? ExcelReaderFactory.CreateOpenXmlReader(stream) : ExcelReaderFactory.CreateBinaryReader(stream);
        DataSet result = excelReader.AsDataSet(new ExcelDataSetConfiguration() {
          ConfigureDataTable = (_) => new ExcelDataTableConfiguration() {
            UseHeaderRow = useHeaderRow
          }
        });

        return result;
      }
    }

    public static List<string[]> ToList(string fileName, bool useHeaderRow = true) {
      if (!File.Exists(fileName))
        return null;

      List<string[]> result = new List<string[]>();
      using (var ds = ToDataSet(fileName, useHeaderRow)) {
        var table = ds.Tables[0];
        foreach (DataRow row in table.Rows) {
          var data = new string[table.Columns.Count];
          for (int i = 0; i < table.Columns.Count; i++) {
            data[i] = row[i]?.ToString();
          }
          result.Add(data);
        }
      }

      return result;
    }

    public static Dictionary<string, List<string[]>> ToDictionary(string fileName, bool useHeaderRow = true) {
      if (!File.Exists(fileName))
        return null;

      var result = new Dictionary<string, List<string[]>>();
      using (var ds = ToDataSet(fileName, useHeaderRow)) {
        for (int j = 0; j < ds.Tables.Count; j++) {
          var table = ds.Tables[j];
          var subResult = new List<string[]>();
          foreach (DataRow row in table.Rows) {
            var data = new string[table.Columns.Count];
            for (int i = 0; i < table.Columns.Count; i++) {
              data[i] = row[i]?.ToString();
            }
            subResult.Add(data);
          }

          result[table.TableName] = subResult;
        }
      }

      return result;
    }
  }
}
