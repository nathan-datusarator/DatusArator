using System;

namespace DatusArator.Core.Events {
  public class MessageEventArgs : EventArgs {
    public string Command { get; set; }
    public string Data { get; set; }

    public MessageEventArgs(string cmd, string data = null) {
      this.Command = cmd;
      this.Data = data;
    }

    public MessageEventArgs() {
    }
  }
}
