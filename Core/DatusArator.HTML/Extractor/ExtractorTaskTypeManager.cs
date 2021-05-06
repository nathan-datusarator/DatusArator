using DatusArator.HTML.Extractor.Tasks;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace DatusArator.HTML.Extractor {
  public class ExtractorTaskTypeManager {
    public static void Initialize() {
      LoadWebPageTask.Register();
      CreateObjectTask.Register();
      CreateListTask.Register();
      FindNodeTask.Register();
      AssignDataTask.Register();
      StartLoopTask.Register();
      EndLoopTask.Register();
      SubScriptTask.Register();
      CustomActionTask.Register();
    }

    private readonly static SortedDictionary<string, IExtractorTaskDefinition> fData = new SortedDictionary<string, IExtractorTaskDefinition>();

    public static void Register(string title, IExtractorTaskDefinition definition) {
      fData.Add(title, definition);
    }

    public static List<string> TaskNames() {
      var result = new List<string>();
      foreach (var item in fData.Keys)
        result.Add(item as string);

      return result;
    }

    public static IExtractorTaskDefinition TaskType(string text) {
      return fData.ContainsKey(text) ? fData[text] : null; 
    }
  }
}