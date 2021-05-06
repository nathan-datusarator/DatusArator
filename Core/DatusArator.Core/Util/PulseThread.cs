using System;
using System.Threading;

namespace DatusArator.Core.Util {
  public class PulseThread {
    public bool Aborted { get; set; }
    public int Rate { get; set; }

    public event EventHandler Pulse;

    public void Start() {
      while (true) {
        Thread.Sleep(Rate);
        Pulse?.Invoke(this, EventArgs.Empty);

        if (Aborted)
          break;
      }
    }

    public void Abort() {
      Aborted = true;
    }
  }
}
