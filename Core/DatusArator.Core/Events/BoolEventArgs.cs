using System;

namespace DatusArator.Core.Events {
  public class BoolEventArgs : EventArgs {
    public bool Value { get; set; }

    public BoolEventArgs(bool value) {
      this.Value = value;
    }
  }

  public class HandledEventArgs : EventArgs {
    public bool Handled { get; set; } = false;
  }
}
