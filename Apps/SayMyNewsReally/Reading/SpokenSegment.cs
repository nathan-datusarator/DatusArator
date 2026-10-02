using System.Speech.Synthesis;
using System.Text;
using System.Text.RegularExpressions;

namespace SayMyNewsReally.Reading;

/// <summary>
/// One chunk of speech (a heading, paragraph, list item, table row...) plus a map from each
/// spoken character back to the source text, so the word being spoken can be highlighted.
/// </summary>
public sealed partial class SpokenSegment {
  // A stretch of spoken text and the source it came from. When the lengths match the
  // mapping is char-for-char; otherwise (link descriptions, "Code block skipped") the whole
  // spoken run maps to the whole source run.
  private readonly record struct Run(int SpokenStart, int SpokenLength, int SourceStart, int SourceLength) {
    public bool IsLinear => SpokenLength == SourceLength;
  }

  private readonly StringBuilder _text = new();
  private readonly List<Run> _runs = [];

  public string Text => _text.ToString();
  public PromptBreak PauseAfter { get; set; } = PromptBreak.Small;
  public bool IsBlank => _text.ToString().Trim().Length == 0;
  public int SourceStart => _runs.Count == 0 ? 0 : _runs.Min(r => r.SourceStart);
  public int SourceEnd => _runs.Count == 0 ? 0 : _runs.Max(r => r.SourceStart + r.SourceLength);

  /// <summary>Appends spoken text that stands for <paramref name="sourceLength"/> chars of source.</summary>
  public void Append(string text, int sourceStart, int sourceLength) {
    if (text.Length == 0) return;
    _runs.Add(new Run(_text.Length, text.Length, sourceStart, Math.Max(sourceLength, 0)));
    _text.Append(text);
  }

  /// <summary>Appends text that has no source of its own (spaces, separators).</summary>
  public void AppendSpacer(string text) {
    if (_text.Length > 0 && !char.IsWhiteSpace(_text[^1])) _text.Append(text);
  }

  /// <summary>Appends ordinary prose, replacing raw URLs with "link to example.com".</summary>
  public void AppendProse(string text, int sourceStart, int sourceLength) {
    if (text.Length != sourceLength) { Append(text, sourceStart, sourceLength); return; }

    var last = 0;
    foreach (Match m in UrlPattern().Matches(text)) {
      var url = m.Value.TrimEnd('.', ',', ';', ':', '!', '?', '\'', '"');
      Append(text[last..m.Index], sourceStart + last, m.Index - last);
      Append(DescribeUrl(url), sourceStart + m.Index, url.Length);
      last = m.Index + url.Length;
    }
    Append(text[last..], sourceStart + last, text.Length - last);
  }

  /// <summary>Maps a spoken range (from SpeakProgress) to a source range.</summary>
  public (int Start, int End) MapToSource(int spokenStart, int spokenLength) {
    if (_runs.Count == 0) return (0, 0);

    var startRun = RunAt(spokenStart);
    var start = startRun.IsLinear
      ? startRun.SourceStart + Math.Min(spokenStart - startRun.SpokenStart, startRun.SourceLength)
      : startRun.SourceStart;

    var spokenEnd = spokenStart + Math.Max(spokenLength, 1);
    var endRun = RunAt(spokenEnd - 1);
    var end = endRun.IsLinear
      ? endRun.SourceStart + Math.Min(spokenEnd - endRun.SpokenStart, endRun.SourceLength)
      : endRun.SourceStart + endRun.SourceLength;

    return (start, Math.Max(start, end));
  }

  private Run RunAt(int spokenPos) {
    var found = _runs[0];
    foreach (var run in _runs) {
      if (run.SpokenStart > spokenPos) break;
      found = run;
    }
    return found;
  }

  public static string DescribeUrl(string url) {
    var withScheme = url.Contains("://") ? url : "http://" + url;
    if (!Uri.TryCreate(withScheme, UriKind.Absolute, out var uri) || uri.Host.Length == 0) return "link";
    var host = uri.Host.StartsWith("www.", StringComparison.OrdinalIgnoreCase) ? uri.Host[4..] : uri.Host;
    return $"link to {host}";
  }

  [GeneratedRegex(@"\b(?:https?://|www\.)[^\s<>()\[\]]+", RegexOptions.IgnoreCase)]
  private static partial Regex UrlPattern();
}
