using DatusArator.HTML.Extractor.Tasks.Settings;
using System;

namespace DatusArator.HTML.Extractor.Tasks {
  public class StartLoopTask : BaseExtractorTaskDefinition<StartLoopSettings> {
    public static void Register() {
      ExtractorTaskTypeManager.Register("Start Loop", new StartLoopTask());
    }

    public override void Execute() {
      OnExecuteFinished?.Invoke(this, false);
    }

    public int iterations = 0;
    public override bool LoopIterate() {
      var loopContinue = iterations++ < 2;

      return loopContinue;
    }

    public override string ToString() {
      return "Start Loop";
    }
  }
}
