using DatusArator.HTML.Extractor.Tasks.Settings;

namespace DatusArator.HTML.Extractor.Tasks {
  public class CreateListTask : BaseExtractorTaskDefinition<CreateListSettings> {
    public static void Register() {
      ExtractorTaskTypeManager.Register("Create List", new CreateListTask());
    }

    public override void Execute() {
      OnExecuteFinished?.Invoke(null, false);
    }

    public override string ToString() {
      return "Create List";
    }
  }
}