using System;

namespace DatusArator.HTML.Extractor.Tasks {
  public interface IExtractorTaskDefinition {
    Action<IExtractorTaskDefinition, bool> OnExecuteFinished { get; set; }

    object InitSettings(ExtractorScript engine, string json);

    void Execute();
    bool LoopIterate();

    IExtractorTaskDefinition Clone();
  }
}