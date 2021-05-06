using DatusArator.HTML.Extractor.Tasks.Settings;

namespace DatusArator.HTML.Extractor.Tasks {
  public class CustomActionTask : BaseExtractorTaskDefinition<CustomActionSettings> {
    public static void Register() {
      ExtractorTaskTypeManager.Register("Custom Action", new CustomActionTask());
    }

    public override void Execute() {
      OnExecuteFinished?.Invoke(null, false);
    }

    public override string ToString() {
      return "Custom Action";
    }
  }
}
