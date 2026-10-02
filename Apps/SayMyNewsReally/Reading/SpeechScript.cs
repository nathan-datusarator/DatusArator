using System.Speech.Synthesis;
using System.Text.RegularExpressions;

namespace SayMyNewsReally.Reading;

public enum TextMode { Auto, Markdown, PlainText }

public static partial class SpeechScript {
  public static (List<SpokenSegment> Segments, bool AsMarkdown) Build(string text, TextMode mode, bool readCode) {
    var asMarkdown = mode switch {
      TextMode.Markdown => true,
      TextMode.PlainText => false,
      _ => LooksLikeMarkdown(text),
    };
    return (asMarkdown ? MarkdownScript.Build(text, readCode) : PlainText(text), asMarkdown);
  }

  /// <summary>
  /// Cheap sniff: one strong signal (heading, fence, link, bold, table) or a couple of list
  /// lines. A false positive is harmless because ordinary prose reads the same either way.
  /// </summary>
  public static bool LooksLikeMarkdown(string text) {
    if (StrongMarkdown().IsMatch(text)) return true;
    return ListLine().Matches(text).Count >= 2;
  }

  /// <summary>
  /// One segment per line, rejoining hard-wrapped lines (a line with no closing punctuation
  /// followed by one starting in lower case) so sentences are not broken up.
  /// </summary>
  private static List<SpokenSegment> PlainText(string text) {
    var segments = new List<SpokenSegment>();
    var current = new SpokenSegment();
    var lineStart = 0;

    while (lineStart <= text.Length) {
      var newline = text.IndexOf('\n', lineStart);
      var lineEnd = newline < 0 ? text.Length : newline;
      var line = text[lineStart..lineEnd].TrimEnd('\r');
      var trimmed = line.Trim();

      if (trimmed.Length == 0) {
        Flush(PromptBreak.Medium);
      } else {
        var start = lineStart + line.IndexOf(trimmed, StringComparison.Ordinal);
        if (!current.IsBlank && !IsContinuation(current.Text, trimmed)) Flush(PromptBreak.Small);
        current.AppendSpacer(" ");
        current.AppendProse(trimmed, start, trimmed.Length);
      }

      if (newline < 0) break;
      lineStart = newline + 1;
    }
    Flush(PromptBreak.Small);
    return segments;

    void Flush(PromptBreak pause) {
      if (current.IsBlank) return;
      current.PauseAfter = pause;
      segments.Add(current);
      current = new SpokenSegment();
    }
  }

  private static bool IsContinuation(string sofar, string nextLine) =>
    !".!?:;\"”')]".Contains(sofar.TrimEnd()[^1]) && char.IsLower(nextLine[0]);

  [GeneratedRegex(@"^#{1,6}\s|^\s*(```|~~~)|\[[^\]\n]+\]\([^)\n]+\)|\*\*[^*\n]+\*\*|^\s*\|.*\|\s*$", RegexOptions.Multiline)]
  private static partial Regex StrongMarkdown();

  [GeneratedRegex(@"^\s*(?:[-*+]|\d+\.)\s+\S", RegexOptions.Multiline)]
  private static partial Regex ListLine();
}
