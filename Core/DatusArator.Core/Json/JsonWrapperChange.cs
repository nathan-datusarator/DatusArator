using DatusArator.Core;
using System;

namespace DatusArator.Core.Json {
  public class JsonWrapperChange {
    public char UpdateType { get; set; }
    public String Key { get; set; }
    public TypedBasicObject OldValue { get; set; }
    public TypedBasicObject NewValue { get; set; }

    internal void Update(TypedBasicObject newValue) {
      if (newValue == null) {
        if (OldValue == null)
          UpdateType = 'X';
        else {
          UpdateType = 'R';
          NewValue = null;
        }
      } else if (newValue.SameValue(OldValue)) {
        UpdateType = 'X';
      } else {
        if (UpdateType != 'A')
          UpdateType = (OldValue != null) ? 'U' : 'N';

        NewValue = newValue;
      }
    }
  }
}
