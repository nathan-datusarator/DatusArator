using DatusArator.Core.Json;
using System;

namespace DatusArator.HTML.Extractor.Tasks {
  public abstract class BaseExtractorTaskDefinition<T> : IExtractorTaskDefinition where T : new() {
    public Action<IExtractorTaskDefinition, bool> OnExecuteFinished { get; set; }
    public Action<IExtractorTaskDefinition> OnLoopIterateFinished { get; set; }

    public ExtractorScript Engine { get; private set; }
    protected T Settings { get; private set; }

    public object InitSettings(ExtractorScript engine, string json) {
      Engine = engine;

      if (string.IsNullOrEmpty(json) || !json.StartsWith("{"))
        Settings = new T();
      else
        Settings = JsonUtils.JsonToObject<T>(json);

      return Settings;
    }

    public abstract void Execute();

    public virtual bool LoopIterate() {
      return false;
    }

    public IExtractorTaskDefinition Clone() {
      return (IExtractorTaskDefinition)MemberwiseClone();
    }
  }
}
