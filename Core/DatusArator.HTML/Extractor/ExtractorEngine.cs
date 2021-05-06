using DatusArator.Core.Json;
using System;
using System.Collections.Generic;

namespace DatusArator.HTML.Extractor {
  public class ExtractorEngine {
    public static Func<string, string, Action<HtmlParser>, bool, int, string, int, bool> OnScrapeWebPage { get; set; }

    public static bool ScrapeWebPage(string url, string caption, Action<HtmlParser> callback, bool highPriority, int scrollToEndTimeout = 4000, string seeMorePattern = null, int maxScrolls = -1) {
      if (OnScrapeWebPage != null)
        return OnScrapeWebPage(url, caption, callback, highPriority, scrollToEndTimeout, seeMorePattern, maxScrolls);
      else {
        callback?.Invoke(null);
        return false;
      }
    }

    #region Script Management (Global)
    private readonly static Dictionary<string, ExtractorScript> fGlobalScripts = new Dictionary<string, ExtractorScript>();
    public static void RegisterGlobalScript(string name, ExtractorScript script) {
      fGlobalScripts[name] = script;
    }
    #endregion

    #region Script Management (Local)
    private readonly Dictionary<string, ExtractorScript> fScripts = new Dictionary<string, ExtractorScript>();

    public void RegisterScript(string name, ExtractorScript script) {
      fScripts[name] = script;
    }
    #endregion

    public ExtractorScript Script(string name) {
      if (fScripts.ContainsKey(name))
        return fScripts[name];

      return fGlobalScripts.ContainsKey(name) ? fGlobalScripts[name] : null;
    }

    #region Data Management (Local)
    private readonly Dictionary<string, object> fData = new Dictionary<string, object>();

    public JsonWrapper Data(string name) {
      if (!fData.ContainsKey(name)) {
        var result = new JsonWrapper();
        fData[name] = result;
      }

      return fData[name] as JsonWrapper;
    }

    public object RawData(string name) {
      return (fData.ContainsKey(name)) ? fData[name] : null;
    }
    #endregion

    public void Execute(string scriptName) {
      var script = Script(scriptName);

      script?.Execute(this);
    }
  }
}