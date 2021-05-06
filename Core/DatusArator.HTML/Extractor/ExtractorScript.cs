using DatusArator.HTML.Extractor.Tasks;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DatusArator.HTML.Extractor {
  public class ExtractorScript {
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public string Comments { get; set; }

    public List<ExtractorTask> Tasks { get; set; }

    public ExtractorScript() {
      Tasks = new List<ExtractorTask>();
    }

    public Action<ExtractorScript> OnExecuted;

    #region Execute
    public ExtractorEngine Engine { get; private set; }
    private Stack<int> LoopStack;
    private Stack<IExtractorTaskDefinition> StateStack;
    private int Index;

    internal void Execute(ExtractorEngine engine) {
      this.Engine = engine;

      LoopStack = new Stack<int>();
      StateStack = new Stack<IExtractorTaskDefinition>();
      Index = 0;

      ExecuteNextStep();
    }

    private void ExecuteNextStep() {
      var task = Tasks[Index];
      var taskType = ExtractorTaskTypeManager.TaskType(task.TaskType).Clone();
      taskType.InitSettings(this, task.TaskData);
      taskType.OnExecuteFinished += TaskFinished;

      DebugPrint("Execute: " + taskType + " [" + task.TaskData + "]", LoopStack.Count);
      taskType.Execute();
    }

    private void MoveToNextStep(IExtractorTaskDefinition state) {
      if (state != null) {
        LoopStack.Push(Index);
        StateStack.Push(state);
      }

      Index++;

      if (Index < Tasks.Count)
        ExecuteNextStep();
      else
        OnExecuted?.Invoke(this);
    }

    private void TaskFinished(IExtractorTaskDefinition state, bool loopEnd) {
      if (loopEnd) {
        var startLoop = LoopStack.Pop();
        var loopStartState = StateStack.Pop();

        bool continueLoop = loopStartState.LoopIterate();
        if (continueLoop) {
          Index = startLoop;
          MoveToNextStep(loopStartState);
        } else
          MoveToNextStep(null);
      } else {
        MoveToNextStep(state);
      }
    }

    private void DebugPrint(string text, int depth) {
      if (Debugger.IsAttached) {
        Console.WriteLine(new String(' ', depth) + text);
      }
    }
    #endregion
  }
}
