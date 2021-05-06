using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace DatusArator.Core.Util {
  public static class StringUtils {
    #region Concat
    public static string Concat(string s1, string s2, string join) {
      if (string.IsNullOrEmpty(s1))
        return s2;
      if (string.IsNullOrEmpty(s2))
        return s1;

      return s1 + join + s2;
    }

    public static string Concat(IEnumerable<decimal> values, string connector) {
      if (values == null)
        return null;

      return string.Join(connector, values);
    }

    public static string Concat(IEnumerable<string> strings, string join, bool skipNulls = false) {
      if (strings == null)
        return null;

      if (skipNulls) {
        string result = "";
        foreach (var s in strings)
          result = Concat(result, s, join);

        return result.Trim();
      } else
        return string.Join(join, strings).Trim();
    }

    public static string SplitCamelCase(string input) {
      return Regex.Replace(input, "([A-Z])", " $1", RegexOptions.Compiled).Trim();
    }

    public static string Concat(string[] strings, string join, bool skipNulls = false) {
      if (strings == null)
        return null;

      if (skipNulls) {
        string result = "";
        foreach (var s in strings)
          result = Concat(result, s, join);

        return result.Trim();
      } else
        return string.Join(join, strings).Trim();
    }

    public static string DupString(string value, int count) {
      var result = "";
      for (int i = 0; i < count; i++)
        result += value;
      return result;
    }

    public static string NumberFormat(decimal value, int places) {
      value = Math.Abs(value);

      if (value >= 100m)
        return "#,##0.00";
      if (value >= 10m)
        return "0.0000";
      if (value >= 0.1m)
        return "0.000000";
      else if (value < 0.00001m)
        return "0.00000000";
      else
        return "0." + DupString("0", places);
    }

    public static string CurrencySymbol(string code) {
      switch (code) {
        case "USD": 
          return "$";
        case "EUR":
          return "€";
        default:
          return "";
      }
    }
    #endregion

    public static bool IsTrue(string value) {
      return !string.IsNullOrEmpty(value) && ("TtYy1".IndexOf(value[0]) >= 0);
    }

    public static bool StringToBool(string p) {
      if (string.IsNullOrEmpty(p))
        return false;

      return "TtYy1".Contains(p[0]);
    }

    public static int IndexOf(object[] items, object item) {
      for (int i = 0; i < items.Length; i++)
        if (items[i] == item)
          return i;

      return -1;
    }

    #region SafeStringTo...
    public static double? SafeStrToDouble(string value, double? defaultValue = null, bool checkForCurrency = false) {
      if (string.IsNullOrEmpty(value))
        return defaultValue;

      if (checkForCurrency) {
        if ("$".Contains(value.Substring(0, 1)))
          value = value.Substring(1);
        if (value.EndsWith("%"))
          value = value.Substring(0, value.Length - 1);
        value = value.Replace(",", "");
      }

      if (Double.TryParse(value, out double result))
        return result;

      return defaultValue;
    }

    public static int? SafeStrToInt(string value, int? defaultValue = null, bool checkForCurrency = false) {
      if (string.IsNullOrEmpty(value))
        return defaultValue;

      if (checkForCurrency) {
        if (value[0] == '$')
          value = value.Substring(1);
        if (value.EndsWith("%"))
          value = value.Substring(0, value.Length - 1);
        value = value.Replace(",", "");

        var point = value.IndexOf('.');
        if (point > 0)
          value = value.Substring(0, point);
      }

      if (Int32.TryParse(value, out int result))
        return result;

      return defaultValue;
    }

    public static long? SafeStrToLong(string value, long? defaultValue = null, bool checkForCurrency = false) {
      if (string.IsNullOrEmpty(value))
        return defaultValue;

      if (checkForCurrency) {
        if (value[0] == '$')
          value = value.Substring(1);
        if (value.EndsWith("%"))
          value = value.Substring(0, value.Length - 1);
        value = value.Replace(",", "");

        var point = value.IndexOf('.');
        if (point > 0)
          value = value.Substring(0, point);
      }

      if (long.TryParse(value, out long result))
        return result;

      return defaultValue;
    }

    public static decimal? SafeStrToDecimal(string value, decimal? defaultValue = null, bool checkForCurrency = false) {
      if (string.IsNullOrEmpty(value))
        return defaultValue;

      if (SameString(value, "NaN"))
        return defaultValue;

      if (checkForCurrency) {
        if ("$".Contains(value.Substring(0, 1)))
          value = value.Substring(1);
        if (value.EndsWith("%"))
          value = value.Substring(0, value.Length - 1);
        value = value.Replace(",", "");
      }

      if (decimal.TryParse(value, out decimal result))
        return result;

      if (double.TryParse(value, out double doubleResult))
        return Convert.ToDecimal(doubleResult);

      return defaultValue;
    }

    private readonly static string[] fDateFormats = new string[] { "g", "M/dd/yyyy hh:mm", "M/d/yyyy", "o" };
    private readonly static CultureInfo fEnUS = new CultureInfo("en-US");
    private readonly static DateTimeStyles fDateStyles = DateTimeStyles.AllowInnerWhite;
    public static DateTime? SafeStrToDate(string value, DateTime? defaultValue = null) {
      if (string.IsNullOrEmpty(value))
        return defaultValue;

      value = value.Trim();

      if (DateTime.TryParse(value, out DateTime result))
        return result;

      if (DateTime.TryParseExact(value, fDateFormats, fEnUS, fDateStyles, out result))
        return result;

      if (long.TryParse(value, out long javaTicks))
        return TypedBasicObject.JavaTimeToNet(javaTicks);

      return defaultValue;
    }
    #endregion

    #region Cleaning
    public static string CleanXML(string value) {
      if (value == null)
        return null;

      var regex = new Regex(@"<.*?>", RegexOptions.None);
      value = regex.Replace(value, @" ").Trim();

      value = value.Replace("&nbsp;", " ");

      regex = new Regex(@"(\n|\t|\r| ){2,}", RegexOptions.None);
      return regex.Replace(value, " ").Trim();
    }

    public static string CleanString(string data) {
      if (string.IsNullOrEmpty(data))
        return data;

      StringBuilder result = new StringBuilder();

      foreach (char c in data) {
        if (c == 25)
          // x19
          result.Append('\'');
        else if (c == 19)
          // x13
          result.Append("!!");
        else if (c == 20)
          // x14
          result.Append('-');
        else if (c == 26)
          // x1A
          result.Append("->");
        else if (c == 27)
          // x1B
          result.Append('+');
        else if (c == 28)
          // x1C  - ?
          result.Append("");
        else if (c == 29)
          // x1D  - ?
          result.Append("");
        else if (c == 30)
          // x1E  - ?
          result.Append("");
        else if (c == 31)
          // x1F  - ?
          result.Append("");
        else
          result.Append(c);
      }

      return result.ToString();
    }

    public static string ProperCase(string value) {
      var result = "";
      foreach (var word in value.Split(' ')) {
        if (string.IsNullOrEmpty(word))
          continue;

        var word2 = word.Substring(0, 1).ToUpper();
        if (word.Length > 1)
          word2 += word.Substring(1);

        result = StringUtils.Concat(result, word2, " ");
      }
      return result;
    }

    public static string RemoveReturns(string data) {
      if (string.IsNullOrEmpty(data))
        return data;

      StringBuilder result = new StringBuilder(data.Length);

      bool inReturn = false;
      foreach (char c in data) {
        if ((c == 10) || (c == 13)) {
          if (!inReturn)
            result.Append(' ');
          inReturn = true;
        } else {
          inReturn = false;
          result.Append(c);
        }
      }

      return result.ToString();
    }
    #endregion

    public static decimal? FirstNumber(string value) {
      string[] parts = value.Split(' ');
      foreach (var part in parts) {
        var result = SafeStrToDecimal(part, null, true);
        if (result != null)
          return result;
      }

      return null;
    }

    public static MemoryStream StringToStream(string source) {
      var bytes = Encoding.UTF8.GetBytes(source ?? "");
      var result = new MemoryStream(bytes, 0, bytes.Length, false, true);
      return result;
    }

    public static string StreamToString(Stream stream) {
      if (stream == null)
        return null;

      using (StreamReader reader = new StreamReader(stream)) {
        string text = reader.ReadToEnd();

        return text;
      }
    }

    public static string RemoveArrayReferences(string source) {
      if (string.IsNullOrEmpty(source))
        return source;

      return Regex.Replace(source, "\\[[^]]*\\]", "");
    }

    public static List<string> RemoveDuplicates(List<string> values) {
      if (values == null)
        return null;

      return values.Distinct().ToList();
    }

    public static List<string> MoveToTop(List<string> source, string which) {
      if ((source == null) || (source.Count < 2) || string.IsNullOrEmpty(which))
        return source;

      var result = new List<string>();
      foreach (var item in source) {
        if (which.Equals(item)) {
          result.Add(item);
          break;
        }
      }

      foreach (var item in source) {
        if (!which.Equals(item)) {
          result.Add(item);
        }
      }

      return result;
    }

    public static string SafeToString(TypedBasicObject value, int length) {
      if ((value == null) || !value.HasValue())
        return "null";

      return SafeToString(value.GetAsString(), length);
    }

    public static string SafeToString(string value, int length) {
      if (value == null)
        return "null";

      if (value.Length < length)
        return value;

      return value.Substring(0, length);
    }

    public static string Rot13Plus(string source) {
      if (string.IsNullOrEmpty(source))
        return source;

      string result = "";
      foreach (char c in source) {
        result += Rot13PlusChar(c);
      }

      return result;
    }

    private static char Rot13PlusChar(char c) {
      if (c >= 'a' && c <= 'z')
        return Convert.ToChar((((c - 'a') + 13) % 26) + 'a');
      if (c >= 'A' && c <= 'Z')
        return Convert.ToChar((((c - 'A') + 13) % 26) + 'A');
      if (c >= '0' && c <= '9')
        return Convert.ToChar((((c - '0') + 5) % 10) + '0');
      if (c >= ' ' && c <= '/')
        return Convert.ToChar((((c - ' ') + 8) % 16) + ' ');

      return c;
    }

    public static string Encodify(string source) {
      if (string.IsNullOrEmpty(source))
        return source;

      string result = new string(source.Reverse().ToArray<Char>());
      result = Rot13Plus(result);

      return result;
    }

    public static string RemoveQuotes(string value) {
      if (string.IsNullOrEmpty(value) || (value.Length == 1))
        return value;

      char quote = value[0];
      if ((quote == '"') || (quote == '\'')) {
        if (value[value.Length - 1] == quote)
          return value.Substring(1, value.Length - 2);
        else
          return value.Substring(1, value.Length - 1);
      }

      return value;
    }

    public static List<string> NonEmptyMatches(string value, string pattern) {
      var result = new List<string>();

      var matches = Regex.Matches(value, pattern);
      foreach (var match in matches) {
        var matchToString = match.ToString();
        if (!string.IsNullOrEmpty(matchToString))
          result.Add(matchToString);
      }

      return result;
    }

    public static List<string> Split(string value, string connector, bool trim = false) {
      var result = new List<string>();

      if (string.IsNullOrEmpty(value))
        return result;

      while (value.Length > 0) {
        var index = value.IndexOf(connector);
        if (index < 0) {
          result.Add(trim ? value.Trim() : value);
          break;
        }

        var newValue = value.Substring(0, index);
        result.Add(trim ? newValue.Trim() : newValue);

        value = value.Substring(index + connector.Length);
      }

      return result;
    }

    public static string Join(IEnumerable<string> values, string connector, bool keepEmpty = true) {
      if (values == null)
        return null;

      StringBuilder result = new StringBuilder();
      foreach (var value in values) {
        if (keepEmpty || !string.IsNullOrEmpty(value)) {
          if (result.Length > 0)
            result.Append(connector);

          result.Append(value);
        }
      }

      return result.ToString();
    }

    public static string Join(IEnumerable values, string connector, bool keepEmpty = true) {
      if (values == null)
        return null;

      StringBuilder result = new StringBuilder();
      foreach (var value in values) {
        if (keepEmpty || (value != null)) {
          if (result.Length > 0)
            result.Append(connector);

          result.Append(value?.ToString() ?? "null");
        }
      }

      return result.ToString();
    }

    public static string Join(object[] values, string connector, bool keepEmpty = true) {
      if (values == null)
        return null;

      StringBuilder result = new StringBuilder();
      foreach (var value in values) {
        if (keepEmpty || (value != null)) {
          if (result.Length > 0)
            result.Append(connector);

          result.Append(value?.ToString() ?? "null");
        }
      }

      return result.ToString();
    }

    #region G-Zip
    public static byte[] GZipData(string source) {
      using (var stream = StringToStream(source))
        return GZipData(stream);
    }

    public static byte[] GZipData(Stream source) {
      MemoryStream result = new MemoryStream();

      using (var zipStream = new GZipStream(result, CompressionMode.Compress, false)) {
        source.CopyTo(zipStream);
      }

      return result.ToArray();
    }

    public static byte[] FromGzipData(byte[] bytes) {
      return FromGzipData(new MemoryStream(bytes));
    }

    public static byte[] FromGzipData(Stream source) {
      MemoryStream result = new MemoryStream();
      using (var zipStream = new GZipStream(source, CompressionMode.Decompress)) {
        zipStream.CopyTo(result);
      }

      return result.ToArray();
    }

    public async static Task FromGzipData(Stream source, Func<string, bool> lineRead, Action onComplete = null) {
      await Task.Run(() => {
        using (var zipStream = new GZipStream(source, CompressionMode.Decompress, true)) {
          try {
            int pos = 0;
            byte[] buffer = new byte[512000];

            int bytesRead = 0;
            byte[] readBuffer = new byte[4096 * 8];  // 32K buffer.  We going for big files

            bool stop = false;
            while ((bytesRead = zipStream.Read(readBuffer, 0, readBuffer.Length)) > 0) {
              for (int i = 0; i < bytesRead; i++) {
                var c = readBuffer[i];
                if (c == 13 || c == 10) {
                  if (pos > 0) {
                    buffer[pos] = 0;
                    var s = Encoding.UTF8.GetString(buffer, 0, pos);
                    stop = lineRead.Invoke(s);
                    if (stop)
                      break;
                  }
                  pos = 0;
                } else {
                  buffer[pos++] = c;
                }
              }

              if (stop)
                break;
            }

            if (!stop && (pos > 0)) {
              buffer[pos] = 0;
              var s = Encoding.UTF8.GetString(buffer, 0, pos);
              lineRead.Invoke(s);
            }
          } catch (Exception ex) {
            Console.WriteLine("Exception in FromGzipData: " + ex.Message);
          }
        } 

        onComplete?.Invoke();
      });
    }
    #endregion

    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
    public static string GetHashString(string value) {
      using (MD5 md5 = MD5.Create()) {
        byte[] signatureHash = md5.ComputeHash(Encoding.UTF8.GetBytes(value));
        string signature = signatureHash.Aggregate(
            new StringBuilder(),
            (sb, b) => sb.Append(b.ToString("x2", CultureInfo.InvariantCulture))).ToString();
        return signature;
      }
    }

    public static string ToXML(object value) {
      return ToXML(value, null);
    }

    public static string ToXML(object value, XmlRootAttribute root) {
      var serializer = (root == null ? new XmlSerializer(value.GetType()) : new XmlSerializer(value.GetType(), root));
      var emptyNamepsaces = new XmlSerializerNamespaces();
      emptyNamepsaces.Add(string.Empty, string.Empty);

      var settings = new XmlWriterSettings() {
        Indent = true,
        OmitXmlDeclaration = true
      };

      var stream = new StringWriter();
      using (var writer = XmlWriter.Create(stream, settings)) {
        serializer.Serialize(writer, value, emptyNamepsaces);
        return stream.ToString();
      }
    }

    public static object FromXML(string xml, Type type) {
      var serializer = new XmlSerializer(type);
      return serializer.Deserialize(StringToStream(xml));
    }

    public static string GetResource(Assembly assembly, string resource) {
      var stream = assembly.GetManifestResourceStream(resource);
      return StringUtils.StreamToString(stream);
    }

    public static string GetResource(Type type, string resource) {
      return GetResource(type.Assembly, resource);
    }

    public static string IndentBlock(string value, int spaces) {
      return PrependBlock(value, new String(' ', spaces));
    }

    public static string PrependBlock(string value, string prefix) {
      if (string.IsNullOrEmpty(value))
        return "";

      var result = new StringBuilder();
      foreach (var line in value.Split('\n')) {
        result.Append(prefix).Append(line).Append('\n');
      }

      return result.ToString().TrimEnd();
    }

    public static string Shorten(string value, int maxLength) {
      if ((value == null) || (value.Length <= maxLength))
        return value;

      return (maxLength <= 3) ? value.Substring(0, maxLength) : (value.Substring(0, maxLength - 3) + "...");
    }

    public static bool SameString(string s1, string s2) {
      return s1?.Equals(s2 ?? "", StringComparison.CurrentCultureIgnoreCase) ?? string.IsNullOrEmpty(s2);
    }

    #region Templates 
    // TODO Make this more efficient
    public static string Render(string template, Dictionary<string, string> values) {
      if (values == null)
        return template;

      foreach (var value in values) {
        template = template.Replace("{" + value.Key + "}", value.Value);
      }
      return template;
    }
    #endregion
  }
}
