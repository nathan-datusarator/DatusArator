using DatusArator.HTML.Extractor.Tasks.Settings;

namespace DatusArator.HTML.Extractor.Tasks {
  public class SubScriptTask : BaseExtractorTaskDefinition<SubScriptSettings> {
    public static void Register() {
      ExtractorTaskTypeManager.Register("Run Sub Script", new SubScriptTask());
    }

    public override void Execute() {
      OnExecuteFinished?.Invoke(null, false);
    }

    public override string ToString() {
      return "Sub Script";
    }
  }
}
