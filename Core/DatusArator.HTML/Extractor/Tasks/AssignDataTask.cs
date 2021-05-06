using DatusArator.HTML.Extractor.Tasks.Settings;

namespace DatusArator.HTML.Extractor.Tasks {
  public class AssignDataTask : BaseExtractorTaskDefinition<AssignDataSettings> {
    public static void Register() {
      ExtractorTaskTypeManager.Register("Assign Data", new AssignDataTask());
    }

    public override void Execute() {
      OnExecuteFinished?.Invoke(null, false);
    }

    public override string ToString() {
      return "Assign Data";
    }
  }
}
