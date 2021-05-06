using System.ComponentModel;

namespace DatusArator.HTML.Extractor.Tasks.Settings {
  public class FindNodeSettings {
    [CategoryAttribute("Settings")]
    public string Source { get; set; } = "[Item]";

    [CategoryAttribute("Settings")]
    public string SubNode { get; set; } = "";

    [CategoryAttribute("Settings")]
    public string Query { get; set; } = "img";

    [CategoryAttribute("Settings")]
    public int Offset { get; set; } = 0;

    [CategoryAttribute("Output")]
    public string CacheOut { get; set; } = "[Current]";
  }
}
