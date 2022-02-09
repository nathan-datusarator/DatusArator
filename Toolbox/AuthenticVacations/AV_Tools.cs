using System;
using System.Collections.Generic;
using System.Xml;

using DatusArator.Core.Json;
using DatusArator.Core.Util;
using DatusArator.Excel;

namespace Toolbox.AuthenticVacations {
  public static class AV_Tools {
    public static readonly string DIRECTORY = @"d:\Tools\quasar\av_reserve\src\assets\reference\";
    public static readonly string FILE_NAME =
      @"TRIP PLANNER APP Templates & Tours.xlsx";

    public readonly static string nl = Environment.NewLine;

    public static string CompileInterests() {
      var allData = ExcelUtils.ToDictionary(DIRECTORY + FILE_NAME, false);
      var data = allData["Interests TABLE"];

      var wrapper = CompileInterests(data);

      return wrapper.ToJsonString(true);
    }

    private static JsonWrapper CompileInterests(List<string[]> data) {
      var colMap = BuildColMap(data[0]);
      var wrapper = new JsonWrapper();

      for (var i = 1; i < data.Count; i++)
        ParseRow(data[i]);

      return wrapper;

      void ParseRow(string[] row) {
        var key = row[0].Trim();
        for (int i = 1; i < row.Length; i++) {
          if (!string.IsNullOrEmpty(row[i]))
            wrapper.AddToArray(colMap[i], key);
        }
      }
    }

    public static string CompileTours() {
      var allData = ExcelUtils.ToDictionary(DIRECTORY + FILE_NAME, false);
      var data = allData["ALL TOURS"];

      var fieldMap = new Dictionary<string, string> {
        ["Brewer"] = "Breweries & Distilleries"
      };

      var colMap = BuildColMap(data[0], fieldMap);
      var wrapper = new JsonWrapper();

      var interestsMap = new Dictionary<string, List<string>>();

      for (var i = 1; i < data.Count; i++)
        ParseRow(data[i]);

      //var interests = CompileInterests(allData["Interests TABLE"]);
      //interestsMap["Scandinavia"] = interests.GetArrayAsList("Scandinavia");

      foreach (var item in interestsMap) {
        item.Value.Sort();

        foreach (var entry in item.Value)
          wrapper.AddToArray("interests." + item.Key, entry);
      }

      return wrapper.ToJsonString(true);

      void ParseRow(string[] row) {
        if (string.IsNullOrEmpty(row[4].Trim()))
          return;

        var region = row[4].Trim();
        var isUSA = false;
        if (region == "USA") {
          isUSA = true;
          region = row[5].Trim();
          if (region.StartsWith("Calif"))
            region = "Wine Country";
        }

        JsonWrapper result = null;
        JsonWrapper index = null;

        var id = row[0].Trim();
        var isGuided = !string.IsNullOrEmpty(row[2]);

        if (!string.IsNullOrEmpty(id) && !id.StartsWith("X", StringComparison.InvariantCultureIgnoreCase)) {
          var name = row[1].Trim();
          var nights = StringUtils.SafeStrToInt(row[3].Trim(), 21).Value;

          if (wrapper.HasValue("index." + id + ".name")) {
            Console.WriteLine("Duplicate Tour Found: " + id);
            return;
          }

          var prefix = isGuided ? "guided" : "tours";
          result = wrapper.ExtendArray(prefix + "." + region);
          result["id"] = id;
          result["name"] = name;
          result["nights"] = nights;

          index = new JsonWrapper(wrapper, "index." + id);
          index["name"] = name;
          index["destination"] = isUSA ? "United States" : region;
          index["subDestination"] = isUSA ? region : "";
          index["nights"] = nights;
        }

        if (!isGuided) {
          for (int i = 9; i < row.Length; i++) {
            if (!string.IsNullOrEmpty(row[i])) {
              result?.AddToArray("interests", colMap[i]);
              index?.AddToArray("interests", colMap[i]);

              AddToInterestsMap(region, colMap[i]);
            }
          }
        }
      }

      void AddToInterestsMap(string region, string interest) {
        if (!interestsMap.ContainsKey(region))
          interestsMap[region] = new List<string>();

        if (!interestsMap[region].Contains(interest))
          interestsMap[region].Add(interest);
      }
    }

    private static Dictionary<int, string> BuildColMap(string[] row, Dictionary<string, string> fieldMap = null) {
      var colMap = new Dictionary<int, string>();
      for (int i = 0; i < row.Length; i++)
        if (!string.IsNullOrEmpty(row[i])) {
          colMap[i] = MapValue(row[i]);
        }

      return colMap;

      string MapValue(string value) {
        if (fieldMap != null) {
          foreach (var item in fieldMap) {
            if (value.StartsWith(item.Key))
              return item.Value;
          }
        }

        return value.Trim();
      }
    }

    public static string BuildWorldMap() {
      var xml = FileUtils.FileToString(DIRECTORY + @"map_templates\AV-world-map.svg");

      var doc = new XmlDocument();
      try {
        doc.LoadXml(xml);

        var result = new JsonWrapper();
        result.Put("meta.name", "World");

        var svg = doc.GetElementsByTagName("svg");
        result.Put("meta.viewbox", svg[0].Attributes["viewBox"].Value);

        foreach (XmlNode g in doc.GetElementsByTagName("g")) {
          if (g.Attributes["id"] == null)
            continue;

          var region = g.Attributes["id"].Value;
          if (!"Other".Equals(region))
            result.AddToArray("meta.regions", region);

          var sub = result.ExtendArray("data." + region);
          foreach (XmlNode node in g.ChildNodes) {
            if (node.Name.StartsWith("#"))
              continue;

            var subItem = sub.ExtendArray(node.Name);
            switch (node.Name) {
              case "path":
                AddAttribute(node, subItem, "d");
                AddAttribute(node, subItem, "transform");
                break;
              case "polygon":
                AddAttribute(node, subItem, "points");
                break;
              case "ellipse":
                AddAttribute(node, subItem, "cx");
                AddAttribute(node, subItem, "cy");
                AddAttribute(node, subItem, "rx");
                AddAttribute(node, subItem, "ry");
                break;
              default:
                Console.WriteLine("Missing type: " + node.Name);
                break;
            }
          }
        }

        return result.ToJsonString(true);
      } catch (Exception ex) {
        return "Exception: " + ex.Message;
      }
    }

    public static string BuildUSMap() {
      var xml = FileUtils.FileToString(DIRECTORY + @"map_templates\AV-USA-map.svg");

      var doc = new XmlDocument();
      try {
        doc.LoadXml(xml);

        var result = new JsonWrapper();
        result.Put("meta.name", "United States");

        var svg = doc.GetElementsByTagName("svg");
        result.Put("meta.viewbox", svg[0].Attributes["viewBox"].Value);

        foreach (XmlNode g in doc.GetElementsByTagName("g")) {
          if (g.Attributes["id"] == null)
            continue;

          var region = g.Attributes["id"].Value;
          if (!"Other".Equals(region))
            result.AddToArray("meta.regions", region);

          var sub = result.ExtendArray("data." + region);
          foreach (XmlNode node in g.ChildNodes) {
            if (node.Name.StartsWith("#"))
              continue;

            var subItem = sub.ExtendArray(node.Name);
            switch (node.Name) {
              case "path":
                AddAttribute(node, subItem, "d");
                AddAttribute(node, subItem, "transform");
                break;
              case "polygon":
                AddAttribute(node, subItem, "points");
                break;
              case "ellipse":
                AddAttribute(node, subItem, "cx");
                AddAttribute(node, subItem, "cy");
                AddAttribute(node, subItem, "rx");
                AddAttribute(node, subItem, "ry");
                break;
              default:
                Console.WriteLine("Missing type: " + node.Name);
                break;
            }
          }
        }

        return result.ToJsonString(true);
      } catch (Exception ex) {
        return "Exception: " + ex.Message;
      }
    }

    public static string BuildUKMap() {
      var xml = FileUtils.FileToString(DIRECTORY + @"map_templates\AV-Ireland-UK-map.svg");

      var doc = new XmlDocument();
      try {
        doc.LoadXml(xml);

        var result = new JsonWrapper();
        result.Put("meta.name", "UK-Ireland");

        var svg = doc.GetElementsByTagName("svg");
        result.Put("meta.viewbox", svg[0].Attributes["viewBox"].Value);

        var list = doc.GetElementsByTagName("g");
        foreach (XmlNode g in list) {
          var region = g.Attributes["id"].Value;
          result.AddToArray("meta.regions", region);

          var sub = result.ExtendArray("data." + region);
          foreach (XmlNode node in g.ChildNodes) {
            if (node.Name.StartsWith("#"))
              continue;

            var subItem = sub.ExtendArray(node.Name);
            switch (node.Name) {
              case "path":
                AddAttribute(node, subItem, "d");
                break;
              case "polygon":
                AddAttribute(node, subItem, "points");
                break;
              case "ellipse":
                AddAttribute(node, subItem, "cx");
                AddAttribute(node, subItem, "cy");
                AddAttribute(node, subItem, "rx");
                AddAttribute(node, subItem, "ry");
                break;
              default:
                Console.WriteLine("Missing type: " + node.Name);
                break;
            }
          }
        }

        return result.ToJsonString(true);
      } catch (Exception ex) {
        return "Exception: " + ex.Message;
      }
    }

    private static void AddAttribute(XmlNode node, JsonWrapper result, string attr) {
      result[attr] = node.Attributes[attr]?.Value;
    }

    public static string MapDestinations(string text) {
      var result = new JsonWrapper();

      foreach (var line in text.Split('\n')) {
        var parts = line.Split('\t');
        var dest = parts[1].Trim();
        dest = dest.Substring(0, dest.Length - 1);
        dest = dest.Substring(dest.LastIndexOf('/') + 1);

        result[dest] = (parts[1].Contains("usa") ? "US:" : "") + parts[0];
      }

      return result.ToJsonString(true);
    }
  }
}
