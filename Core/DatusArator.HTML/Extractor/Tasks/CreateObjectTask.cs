using DatusArator.HTML.Extractor.Tasks.Settings;

namespace DatusArator.HTML.Extractor.Tasks {
  public class CreateObjectTask : BaseExtractorTaskDefinition<CreateObjectSettings> {
    public static void Register() {
      ExtractorTaskTypeManager.Register("Create Data Object", new CreateObjectTask());
    }

    public override void Execute() {
      OnExecuteFinished?.Invoke(null, false);
    }

    public override string ToString() {
      return "Create List";
    }
  }
}
