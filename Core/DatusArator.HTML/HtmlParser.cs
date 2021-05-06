using DatusArator.Core.Http;
using DatusArator.Core.Json;
using DatusArator.Core.Logging;
using DatusArator.Core.Util;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace DatusArator.HTML {
  public class HtmlParser {
    private HtmlDocument Document;

    public static bool ParseHtml = true;

    public string RawHtml { get; private set; }
    public string ErrorMessage { get; private set; }

    public HtmlParser(string url, string text) {
      Document = new HtmlDocument();

      if (string.IsNullOrEmpty(url) && string.IsNullOrEmpty(text))
        return;

      try {
        if (url != null) {
          RawHtml = WebUtils.GetUri(new Uri(url), true);
        } else {
          RawHtml = text;
        }

        if (ParseHtml) {
          MemoryStream ms = new MemoryStream(Encoding.Default.GetBytes(RawHtml));
          Document.Load(ms);
          ms.Close();
        }
      } catch (Exception ex) {
        ErrorMessage = ex.Message;
        GeneralUtils.RaiseGlobalError("Error in Get url: " + ex.Message + " [" + url + "]");
      }
    }

    #region Direct Loading using HTML Agility Pack
    private static HtmlWeb fWebClient;
    private static int fWebClientUsed = 10000;

    private HtmlWeb GetWebClient() {
      if (fWebClientUsed++ > 50) {
        fWebClient = new HtmlWeb {
          UserAgent = @"Mozilla/5.0 (Windows NT 6.3; Trident/7.0; rv:11.0) like Gecko"
        };

        fWebClientUsed = 0;
      }

      return fWebClient;
    }

    private HtmlDocument LoadFromWeb(string url) {
      var getHtmlWeb = GetWebClient();

      int tries = 0;
      while (tries < 4) {
        try {
          var result = getHtmlWeb.Load(url);
          return result;
        } catch (Exception ex) {
          DatusAratorLogger.Log(this, DatusAratorLogLevel.ERROR, ex.ToString() + " : " + url);
        }
      }

      return null;
    }
    #endregion

    public HtmlNode RootNode { get { return Document?.DocumentNode; } }

    public void Cleanup() {
      Document.LoadHtml("");
      Document = null;

      RawHtml = null;

      GC.Collect();
    }

    #region JsonWrapper
    public JsonWrapper[] Links { get { return SelectNodes("//a", false); } }
    public JsonWrapper[] Inputs { get { return SelectNodes("//input", false); } }
    public JsonWrapper[] Images { get { return SelectNodes("//img", false); } }
    public JsonWrapper[] Divs { get { return SelectNodes("//div", false); } }
    public JsonWrapper[] Spans { get { return SelectNodes("//span", false); } }
    public JsonWrapper[] TableRows { get { return SelectNodes("//tr", false); } }

    public JsonWrapper[] SelectNodes(string pattern, bool isJQuery = true) {
      return SelectNodes(RootNode, pattern, isJQuery);
    }

    public JsonWrapper[] SelectNodes(HtmlNode rootNode, String pattern, bool isJQuery = true) {
      if (String.IsNullOrEmpty(pattern))
        return new JsonWrapper[0];

      if (isJQuery)
        pattern = JqueryToXpath(pattern);

      if (rootNode == null) {
        rootNode = RootNode;
      } else {
        if (!pattern.StartsWith("."))
          pattern = "." + pattern;
      }

      var tags = rootNode.SelectNodes(pattern);
      if (tags != null)
        return (from node in tags
                select new JsonWrapper().Put("inner", CleanValue(node.InnerHtml))
                                        .Put("text", CleanValue(node.InnerText))
                                        .Put("href", WebUtility.HtmlDecode(GetAttributeValue(node.Attributes["href"])))
                                        .Put("id", GetAttributeValue(node.Attributes["id"]))
                                        .Put("link", GetAttributeValue(node.Attributes["link"]))
                                        .Put("content", GetAttributeValue(node.Attributes["content"]))
                                        .Put("title", GetAttributeValue(node.Attributes["title"]))
                                        .Put("name", GetAttributeValue(node.Attributes["name"]))
                                        .Put("class", GetAttributeValue(node.Attributes["class"]))
                                        .Put("src", GetAttributeValue(node.Attributes["src"]))
                                        .Put("alt", GetAttributeValue(node.Attributes["alt"]))
                                        .Put("height", GetAttributeValue(node.Attributes["height"]))
                                        .Put("width", GetAttributeValue(node.Attributes["width"]))
                                        .Put("style", GetAttributeValue(node.Attributes["style"]))
                                        .Put("type", GetAttributeValue(node.Attributes["type"]))
                                        .Put("value", GetAttributeValue(node.Attributes["value"]))
                                        .Put("tag", node.Name)
                                        .SetExtraData(node)
                )
                .ToArray();
      else
        return new JsonWrapper[0];
    }

    // Maybe make more efficient later
    public JsonWrapper SelectFirstNode(string pattern, bool isJQuery = true) {
      var nodes = SelectNodes(RootNode, pattern, isJQuery);
      return (nodes.Length > 0) ? nodes[0] : new JsonWrapper();
    }

    // Maybe make more efficient later
    public JsonWrapper SelectFirstNode(HtmlNode rootNode, string pattern, bool isJQuery = true) {
      var nodes = SelectNodes(rootNode, pattern, isJQuery);
      return (nodes.Length > 0) ? nodes[0] : new JsonWrapper();
    }
    #endregion

    #region All Nodes
    public JsonWrapper[] Nodes { get { return GetNodes(); } }
    public JsonWrapper[] GetNodes(bool recursive = true) {
      return GetNodes(RootNode, recursive);
    }

    public JsonWrapper[] GetNodes(HtmlNode parentNode, bool recursive = true) {
      var nodes = new List<JsonWrapper>(
                   from node in parentNode.ChildNodes
                   where (!String.IsNullOrEmpty(CleanValue(node.InnerHtml)) || (node.Attributes.Count > 0)) && !"#text".Equals(node.Name)
                   select new JsonWrapper().Put("inner", CleanValue(node.InnerHtml))
                                           .Put("href", WebUtility.HtmlDecode(GetAttributeValue(node.Attributes["href"])))
                                           .Put("id", GetAttributeValue(node.Attributes["id"]))
                                           .Put("link", GetAttributeValue(node.Attributes["link"]))
                                           .Put("name", GetAttributeValue(node.Attributes["name"]))
                                           .Put("content", GetAttributeValue(node.Attributes["content"]))
                                           .Put("class", GetAttributeValue(node.Attributes["class"]))
                                           .Put("src", GetAttributeValue(node.Attributes["src"]))
                                           .Put("alt", GetAttributeValue(node.Attributes["alt"]))
                                           .Put("height", GetAttributeValue(node.Attributes["height"]))
                                           .Put("width", GetAttributeValue(node.Attributes["width"]))
                                           .Put("style", GetAttributeValue(node.Attributes["style"]))
                                           .Put("tag", node.Name)
                                           .SetExtraData(node)
                  );

      if (recursive) {
        foreach (var node in parentNode.ChildNodes) {
          if (node.HasChildNodes)
            nodes.AddRange(GetNodes(node));
        }
      }

      return nodes.ToArray();
    }

    public List<string> GetClassList(HtmlNode node) {
      var result = new List<string>();
      foreach (var item in node.GetAttributeValue("class", "").Split(' '))
        result.Add(item);
      return result;
    }

    private bool HasText(HtmlAttribute htmlAttribute) {
      return (htmlAttribute != null) && !String.IsNullOrEmpty(CleanValue(htmlAttribute.Value));
    }

    private string GetAttributeValue(HtmlAttribute htmlAttribute) {
      return htmlAttribute?.Value;
    }

    private string CleanValue(string value) {
      if (value == null)
        return null;

      var regex = new Regex(@"<.*?>", RegexOptions.None);
      value = regex.Replace(value, @" ").Trim();

      value = value.Replace("&nbsp;", " ");

      regex = new Regex(@"(\n|\t|\r| ){2,}", RegexOptions.None);
      return regex.Replace(value, " ").Trim();
    }
    #endregion

    #region Raw Access
    public HtmlNodeCollection SelectNodesRaw(string pattern, bool isJQuery = true) {
      return SelectNodesRaw((HtmlNode)null, pattern, isJQuery);
    }

    public HtmlNodeCollection SelectNodesRaw(JsonWrapper rootNode, string pattern, bool isJQuery = true) {
      if (rootNode == null) return null;
      return SelectNodesRaw(rootNode.GetExtraData<HtmlNode>(), pattern, isJQuery);
    }

    public HtmlNodeCollection SelectNodesRaw(HtmlNode rootNode, string pattern, bool isJQuery = true) {
      if (String.IsNullOrEmpty(pattern))
        return null;

      if (isJQuery)
        pattern = JqueryToXpath(pattern);

      if (rootNode == null) {
        rootNode = RootNode;
      } else {
        if (!pattern.StartsWith("."))
          pattern = "." + pattern;
      }

      return rootNode.SelectNodes(pattern);
    }


    public HtmlNode SelectFirstNodeRaw(string pattern, bool isJQuery = true) {
      var nodes = SelectNodesRaw(pattern, isJQuery);
      return (nodes != null) && (nodes.Count > 0) ? nodes[0] : null;
    }

    public HtmlNode SelectFirstNodeRaw(HtmlNode rootNode, string pattern, bool isJQuery = true) {
      var nodes = SelectNodesRaw(rootNode, pattern, isJQuery);
      return (nodes != null) && (nodes.Count > 0) ? nodes[0] : null;
    }
    #endregion

    #region JQuery to Xpath Parsing
    public static String JqueryToXpath(String exp) {
      if (String.IsNullOrEmpty(exp))
        return "";

      if (exp.IndexOf(' ') > 0) {
        var parts = exp.Split(' ');

        var result = new StringBuilder();
        foreach (var part in parts) {
          result.Append(JqueryToXpath(part));
        }

        return result.ToString();
      }

      // Lots of work to do  
      // TODO Need to handle multiple parts (div.row.header)
      if (exp.IndexOf('.') > 0) {
        var parts = exp.Split('.');
        return "//" + parts[0] + "[" + buildClassString(parts[1]) + "]";
      } else if (exp.IndexOf('#') > 0) {
        var parts = exp.Split('#');
        return "//" + parts[0] + $"[@id='" + parts[1] + $"']";
      } else
        return "//" + exp;
    }

    private static String buildClassString(String className) {
      return "contains(concat(' ', normalize-space(@class), ' '), ' " + className.Trim() + " ')";
    }
    #endregion

    #region Utilities
    public String FindFirstHref(HtmlNode rootNode) {
      var first = SelectFirstNode(rootNode, "a");
      return first.Get("href");
    }

    public string Html() {
      return RootNode.InnerHtml;
    }
    #endregion
  }
}
