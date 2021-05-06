using System.ComponentModel;

namespace DatusArator.HTML.Extractor.Tasks.Settings {
  public class LoadWebPageSettings {
    [CategoryAttribute("Output")]
    public string CacheOut { get; set; } = "[HtmlParser]";

    [CategoryAttribute("Output")]
    public bool Parsed { get; set; } = true;

    [CategoryAttribute("Settings")]
    public string Url { get; set; } = "";

    [CategoryAttribute("Settings")]
    public string UrlProvider { get; set; } = "[FacebookUser]:GetUrl:[Url]";

    [CategoryAttribute("Settings")]
    public bool BrowserFetch { get; set; } = false;
  }
}
