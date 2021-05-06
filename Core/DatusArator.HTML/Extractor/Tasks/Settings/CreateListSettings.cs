using System.ComponentModel;

namespace DatusArator.HTML.Extractor.Tasks.Settings {
  public class CreateListSettings {
    [CategoryAttribute("Settings")]
    public string ClassName { get; set; } = "Harvester.Facebook.Scrapers.GraphSearchResult";

    [CategoryAttribute("Output")]
    public string CacheOut { get; set; } = "[Results]";
  }
}
