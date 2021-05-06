using System.ComponentModel;

namespace DatusArator.HTML.Extractor.Tasks.Settings {
  public class StartLoopSettings {
    [CategoryAttribute("Settings")]
    public string Source { get; set; } = "[HtmlParser]";

    [CategoryAttribute("Settings")]
    public string SubNode { get; set; } = "";

    [CategoryAttribute("Settings")]
    public string Query { get; set; } = "div._401d";

    [CategoryAttribute("Output")]
    public string Iterand { get; set; } = "[Item]";
  }
}
