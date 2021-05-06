using System.ComponentModel;

namespace DatusArator.HTML.Extractor.Tasks.Settings {
  public class AssignDataSettings {
    [CategoryAttribute("Settings")]
    public string Source { get; set; } = "[Current]";

    [CategoryAttribute("Settings")]
    public string Attribute { get; set; } = "src";

    [CategoryAttribute("Output")]
    public string Target { get; set; } = "[CurrentOut]";

    [CategoryAttribute("Output")]
    public string TargetField { get; set; } = "Thumbnail";

    [CategoryAttribute("Output")]
    public string TargetMapper { get; set; } = "CleanUrl";
  }
}
