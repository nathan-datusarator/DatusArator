using System.ComponentModel;

namespace DatusArator.HTML.Extractor.Tasks.Settings {
  public class CustomActionSettings {
    [CategoryAttribute("Settings")]
    public string Action { get; set; } = "Action Name";
  }
}
