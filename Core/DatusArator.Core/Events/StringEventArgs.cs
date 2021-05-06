using System;

namespace DatusArator.Core.Events {
  public class StringEventArgs : EventArgs {
    public string Value { get; set; }

    public StringEventArgs(string value) {
      this.Value = value;
    }

    public StringEventArgs() {
    }
  }
}
