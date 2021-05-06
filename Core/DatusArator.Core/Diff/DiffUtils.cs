using System;
using System.Collections.Generic;

namespace DatusArator.Core.Diff {
  public class DiffUtils {
    public static bool VerboseMode { get; set; } = false;

    public static double PercentSame(String string1, String string2, bool useLonger = false) {
      if (string.IsNullOrEmpty(string1) || string.IsNullOrEmpty(string2))
        return 0.0;

      diff_match_patch engine = new diff_match_patch();
      List<Diff> diffs = engine.diff_main(string1, string2);
      engine.diff_cleanupSemantic(diffs);

      if (VerboseMode)
        Console.WriteLine(diffs);

      int chars = 0;
      foreach (Diff diff in diffs) {
        if (diff.operation == Operation.EQUAL)
          chars += diff.text.Length;
      }

      int length = useLonger ? Math.Max(string1.Length, string2.Length) : Math.Min(string1.Length, string2.Length);

      return (chars * 1.0) / length;
    }
  }
}
