using DatusArator.HTML.Extractor.Tasks.Settings;

namespace DatusArator.HTML.Extractor.Tasks {
  public class EndLoopTask : BaseExtractorTaskDefinition<EndLoopSettings> {
    public static void Register() {
      ExtractorTaskTypeManager.Register("End Loop", new EndLoopTask());
    }

    public override void Execute() {
      OnExecuteFinished?.Invoke(null, true);
    }

    public override string ToString() {
      return "End Loop";
    }
  }
}
