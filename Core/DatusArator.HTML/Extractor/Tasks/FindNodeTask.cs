using DatusArator.HTML.Extractor.Tasks.Settings;

namespace DatusArator.HTML.Extractor.Tasks {
  public class FindNodeTask : BaseExtractorTaskDefinition<FindNodeSettings> {
    public static void Register() {
      ExtractorTaskTypeManager.Register("Find Node", new FindNodeTask());
    }

    public override void Execute() {
      OnExecuteFinished?.Invoke(null, false);
    }

    public override string ToString() {
      return "Find Node";
    }
  }
}
