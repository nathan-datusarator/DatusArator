using System.Speech.Synthesis;
using Markdig;
using Markdig.Extensions.Footnotes;
using Markdig.Extensions.Tables;
using Markdig.Extensions.TaskLists;
using Markdig.Extensions.Yaml;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace SayMyNewsReally.Reading;

/// <summary>
/// Turns Markdown into speech segments: reads what a person would read on the rendered page
/// and leaves out the syntax (#, **, [](), table pipes, front matter, HTML).
/// </summary>
public sealed class MarkdownScript {
  private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
    .UseAdvancedExtensions()
    .UseYamlFrontMatter()
    .Build();

  private static readonly Dictionary<string, string> LanguageNames = new(StringComparer.OrdinalIgnoreCase) {
    ["cs"] = "C#", ["csharp"] = "C#", ["js"] = "JavaScript", ["javascript"] = "JavaScript",
    ["ts"] = "TypeScript", ["typescript"] = "TypeScript", ["py"] = "Python", ["python"] = "Python",
    ["sh"] = "shell", ["bash"] = "shell", ["ps1"] = "PowerShell", ["powershell"] = "PowerShell",
    ["sql"] = "SQL", ["json"] = "JSON", ["xml"] = "XML", ["html"] = "HTML", ["css"] = "CSS",
    ["yaml"] = "YAML", ["yml"] = "YAML",
  };

  private readonly bool _readCode;
  private readonly List<SpokenSegment> _segments = [];
  private (string Text, int Start, int Length)? _pendingPrefix;

  private MarkdownScript(bool readCode) => _readCode = readCode;

  public static List<SpokenSegment> Build(string markdown, bool readCode) {
    var script = new MarkdownScript(readCode);
    script.Block(Markdown.Parse(markdown, Pipeline));
    return script._segments;
  }

  private void Block(Block block) {
    switch (block) {
      case YamlFrontMatterBlock or HtmlBlock or FootnoteGroup or LinkReferenceDefinitionGroup:
        return;
      case HeadingBlock heading:
        Leaf(heading.Inline, PromptBreak.Medium);
        return;
      case ParagraphBlock paragraph:
        Leaf(paragraph.Inline, PromptBreak.Small);
        return;
      case CodeBlock code:
        Code(code);
        return;
      case ThematicBreakBlock:
        PauseLonger(PromptBreak.Large);
        return;
      case Table table:
        foreach (var row in table.OfType<TableRow>()) Row(row);
        PauseLonger(PromptBreak.Medium);
        return;
      case ListBlock list:
        foreach (var item in list.OfType<ListItemBlock>()) {
          // "1." or "1)" in the source.
          if (list.IsOrdered) _pendingPrefix = ($"{item.Order}. ", item.Span.Start, item.Order.ToString().Length + 1);
          foreach (var child in item) Block(child);
          _pendingPrefix = null;
        }
        PauseLonger(PromptBreak.Medium);
        return;
      case ContainerBlock container:
        foreach (var child in container) Block(child);
        return;
    }
  }

  private void Leaf(ContainerInline? inlines, PromptBreak pause) {
    var segment = new SpokenSegment { PauseAfter = pause };
    if (_pendingPrefix is { } prefix) {
      segment.Append(prefix.Text, prefix.Start, prefix.Length);
      _pendingPrefix = null;
    }
    Inlines(inlines, segment);
    Add(segment);
  }

  private void Row(TableRow row) {
    var segment = new SpokenSegment { PauseAfter = PromptBreak.ExtraSmall };
    foreach (var cell in row.OfType<TableCell>()) {
      segment.AppendSpacer(", ");
      foreach (var paragraph in cell.OfType<ParagraphBlock>()) Inlines(paragraph.Inline, segment);
    }
    Add(segment);
  }

  private void Code(CodeBlock code) {
    if (!_readCode) {
      var info = (code as FencedCodeBlock)?.Info?.Trim() ?? "";
      var language = LanguageNames.GetValueOrDefault(info, info);
      var spoken = language.Length == 0 ? "Code block skipped." : $"{language} code block skipped.";
      var segment = new SpokenSegment();
      segment.Append(spoken, code.Span.Start, code.Span.Length);
      Add(segment);
      return;
    }

    var lines = code.Lines;
    for (var i = 0; i < lines.Count; i++) {
      var slice = lines.Lines[i].Slice;
      var text = slice.ToString();
      if (text.Trim().Length == 0) continue;
      var segment = new SpokenSegment { PauseAfter = PromptBreak.ExtraSmall };
      segment.Append(text, slice.Start, slice.Length);
      Add(segment);
    }
    PauseLonger(PromptBreak.Small);
  }

  private static void Inlines(ContainerInline? container, SpokenSegment segment) {
    if (container is null) return;
    foreach (var inline in container) Inline(inline, segment);
  }

  private static void Inline(Inline inline, SpokenSegment segment) {
    switch (inline) {
      case LiteralInline literal:
        segment.AppendProse(literal.Content.ToString(), literal.Span.Start, literal.Span.Length);
        break;
      case CodeInline code:
        segment.Append(code.Content, code.Span.Start, code.Span.Length);
        break;
      case LineBreakInline:
        segment.AppendSpacer(" ");
        break;
      case HtmlEntityInline entity:
        segment.Append(entity.Transcoded.ToString(), entity.Span.Start, entity.Span.Length);
        break;
      case TaskList task:
        segment.Append(task.Checked ? "Done: " : "To do: ", task.Span.Start, task.Span.Length);
        break;
      case AutolinkInline auto:
        segment.Append(auto.IsEmail ? "email address" : SpokenSegment.DescribeUrl(auto.Url), auto.Span.Start, auto.Span.Length);
        break;
      case LinkInline { IsImage: true } image:
        if (image.FirstChild is null) break;
        segment.Append("Image: ", image.Span.Start, 0);
        Inlines(image, segment);
        segment.AppendSpacer(". ");
        break;
      case LinkInline link when link.IsAutoLink || link.FirstChild is null || PlainText(link) == link.Url:
        segment.Append(SpokenSegment.DescribeUrl(link.Url ?? ""), link.Span.Start, link.Span.Length);
        break;
      case FootnoteLink:
        break;
      case ContainerInline container:
        Inlines(container, segment);
        break;
    }
  }

  private static string PlainText(ContainerInline container) =>
    string.Concat(container.Descendants<LiteralInline>().Select(l => l.Content.ToString()));

  private void Add(SpokenSegment segment) {
    if (!segment.IsBlank) _segments.Add(segment);
  }

  private void PauseLonger(PromptBreak pause) {
    if (_segments.Count > 0 && _segments[^1].PauseAfter < pause) _segments[^1].PauseAfter = pause;
  }
}
