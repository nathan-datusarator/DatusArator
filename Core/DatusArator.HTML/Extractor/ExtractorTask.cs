using System;
using System.Text;

// Create Object
// Run Sub Script

namespace DatusArator.HTML.Extractor {
  public class ExtractorTask {
    public string Name { get; set; }

    public string TaskType { get; set; }
    public string TaskData { get; set; }

    public override string ToString() {
      var result = Name;
      if (!string.IsNullOrEmpty(TaskType))
        result += " (" + TaskType + ")";

      return result.ToString();
    }
  }
}
