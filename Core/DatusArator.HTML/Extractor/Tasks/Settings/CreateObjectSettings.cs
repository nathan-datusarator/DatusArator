using System.ComponentModel;

namespace DatusArator.HTML.Extractor.Tasks.Settings {
  public class CreateObjectSettings {
    [CategoryAttribute("Settings")]
    public string ClassName { get; set; } = "Harvester.Facebook.Scrapers.GraphSearchResult";

    [CategoryAttribute("Output")]
    public string CacheOut { get; set; } = "[CurrentOut]";
  }
}
