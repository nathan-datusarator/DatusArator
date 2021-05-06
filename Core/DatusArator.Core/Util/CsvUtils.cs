using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Text;

namespace DatusArator.Core.Util {
  public static class CsvUtils {
    public static List<string[]> LoadFile(String fileName) {
      var result = new List<string[]>();

      using (TextFieldParser parser = new TextFieldParser(fileName)) {
        parser.TextFieldType = FieldType.Delimited;
        parser.SetDelimiters(",");
        parser.HasFieldsEnclosedInQuotes = true;

        while (!parser.EndOfData) {
          var data = parser.ReadFields();
          result.Add(data);
        }
      }

      return result;
    }

    public static List<string[]> Parse(string value) {
      var result = new List<string[]>();

      using (TextFieldParser parser = new TextFieldParser(StringUtils.StringToStream(value))) {
        parser.TextFieldType = FieldType.Delimited;
        parser.SetDelimiters(",");
        parser.HasFieldsEnclosedInQuotes = true;

        while (!parser.EndOfData) {
          var data = parser.ReadFields();
          result.Add(data);
        }
      }

      return result;
    }

    public static string[] ParseLine(string value) {
      try {
        using (TextFieldParser parser = new TextFieldParser(StringUtils.StringToStream(value))) {
          parser.TextFieldType = FieldType.Delimited;
          parser.SetDelimiters(",");
          parser.HasFieldsEnclosedInQuotes = true;

          return parser.ReadFields();
        }
      } catch (Exception ex) {
        Console.WriteLine("Parse Error: " + value);
        throw ex;
      }
    }

    public static void WriteFile(List<List<string>> data, string fileName) {
      WriteFile(fileName, data);
    }

    public static void WriteFile(string fileName, List<List<string>> data) {
      var result = "";
      foreach (var row in data) {
        for (int col = 0; col < row.Count; col++) {
          if (col > 0)
            result += ",";
          result += EscapeCol(row[col]);
        }

        result += Environment.NewLine;
      }

      FileUtils.StringToFile(result, fileName);
    }

    public static string BuildLine(string[] row) {
      var result = "";
      for (int col = 0; col < row.Length; col++) {
        if (col > 0)
          result += ",";
        result += EscapeCol(row[col]);
      }
      return result;
    }

    public static string BuildLine(List<string> row) {
      var result = "";
      for (int col = 0; col < row.Count; col++) {
        if (col > 0)
          result += ",";
        result += EscapeCol(row[col]);
      }
      return result;
    }

    private static string EscapeCol(string value) {
      if (value == null)
        return "";

      bool mustQuote = (value.Contains(",") || value.Contains("\"") || value.Contains("\r") || value.Contains("\n") || value.Contains("\t"));
      if (mustQuote) {
        StringBuilder sb = new StringBuilder();
        sb.Append("\"");
        foreach (char nextChar in value) {
          sb.Append(nextChar);
          if (nextChar == '"')
            sb.Append("\"");
        }
        sb.Append("\"");
        return sb.ToString();
      }

      return value;
    }
  }
}
