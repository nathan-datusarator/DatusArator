using DatusArator.HTML.Extractor.Tasks.Settings;

namespace DatusArator.HTML.Extractor.Tasks {
  public class LoadWebPageTask : BaseExtractorTaskDefinition<LoadWebPageSettings> {
    public static void Register() {
      ExtractorTaskTypeManager.Register("Load Web Site", new LoadWebPageTask());
    }

    public override void Execute() {
      OnExecuteFinished?.Invoke(null, false);
    }

    public override string ToString() {
      return "Load Web Page";
    }
  }
}