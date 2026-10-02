using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using SayMyNewsReally.Reading;
using SayMyNewsReally.Speech;

namespace SayMyNewsReally;

public partial class MainWindow : Window {
  private enum ReadState { Idle, Reading, Paused }

  private sealed record ModeOption(TextMode Mode, string Label);

  private const string AppTitle = "Say My News Really";
  private const string PlayIcon = "";
  private const string PauseIcon = "";

  private readonly AppSettings _settings = AppSettings.Load();
  private readonly List<ISpeechEngine> _engines = [];
  private ISpeechEngine? _engine;
  private IReadOnlyList<SpokenSegment> _segments = [];
  private ReadState _state = ReadState.Idle;
  private bool _loading = true;

  public MainWindow() {
    InitializeComponent();

    _engines.Add(new SapiEngine());
    try {
      _engines.Add(new OneCoreEngine());
    } catch (Exception) {
      // Older or stripped-down Windows without the OneCore speech runtime: classic voices only.
    }
    foreach (var engine in _engines) {
      engine.SegmentStarted += Engine_SegmentStarted;
      engine.WordReached += Engine_WordReached;
      engine.Finished += () => FinishReading(completed: true);
    }

    // OneCore has its own copies of David and Zira; only list the voices SAPI does not have.
    var voices = new List<VoiceOption>();
    foreach (var voice in _engines.SelectMany(e => e.Voices))
      if (!voices.Any(v => v.Label == voice.Label)) voices.Add(voice);
    voices.Sort((a, b) => string.Compare(a.Label, b.Label, StringComparison.CurrentCultureIgnoreCase));

    cmbVoice.ItemsSource = voices;
    cmbVoice.SelectedItem =
      voices.FirstOrDefault(v => v.Id == _settings.Voice)
      ?? voices.FirstOrDefault(v => v.Id == "sapi:" + _settings.Voice) // saved before OneCore voices existed
      ?? voices.FirstOrDefault(v => v.Label.StartsWith("Zira"))
      ?? voices.FirstOrDefault();

    var modes = new[] {
      new ModeOption(TextMode.Auto, "Auto detect"),
      new ModeOption(TextMode.Markdown, "Markdown"),
      new ModeOption(TextMode.PlainText, "Plain text"),
    };
    cmbMode.ItemsSource = modes;
    cmbMode.SelectedItem = modes.First(m => m.Mode == _settings.Mode);

    sldRate.Value = Math.Clamp(_settings.Rate, -10, 10);
    chkReadCode.IsChecked = _settings.ReadCode;

    _loading = false;
    UpdateRateLabel();
    UpdateTextInfo();
    SetState(ReadState.Idle);

    // "Open with" from Explorer, or a file dragged onto the exe.
    var args = Environment.GetCommandLineArgs();
    if (args.Length > 1 && File.Exists(args[1])) LoadFile(args[1]);
  }

  // ---- Reading ------------------------------------------------------------------------------

  private void ToggleReading() {
    switch (_state) {
      case ReadState.Reading:
        _engine?.Pause();
        SetState(ReadState.Paused);
        break;
      case ReadState.Paused:
        _engine?.Resume();
        SetState(ReadState.Reading);
        break;
      default:
        StartReading();
        break;
    }
  }

  private void StartReading() {
    if (cmbVoice.SelectedItem is not VoiceOption voice) {
      lblStatus.Text = "No voice selected.";
      return;
    }

    var text = txtMain.Text;
    var (segments, asMarkdown) = SpeechScript.Build(text, SelectedMode, chkReadCode.IsChecked == true);
    if (segments.Count == 0) {
      lblStatus.Text = "Nothing to read.";
      return;
    }

    // Start from the cursor when it sits inside the text (after a Stop, it sits on the last word read).
    var caret = txtMain.CaretIndex;
    var first = caret > 0 && caret < text.Length ? segments.FindIndex(s => s.SourceEnd > caret) : 0;
    if (first < 0) first = 0;

    _segments = segments;
    _engine = voice.Engine;
    try {
      _engine.Start(segments, first, voice, (int)sldRate.Value);
    } catch (Exception ex) {
      _engine = null;
      lblStatus.Text = $"Could not start {voice.Label}: {ex.Message}";
      return;
    }

    lblKind.Text = asMarkdown ? "Reading as Markdown" : "Reading as plain text";
    SetState(ReadState.Reading);
  }

  private void StopReading() {
    if (_state == ReadState.Idle) return;
    _engine?.Stop();
    FinishReading(completed: false);
  }

  private void FinishReading(bool completed) {
    if (_state == ReadState.Idle) return;
    _engine = null;
    SetState(ReadState.Idle);
    if (completed) {
      txtMain.Select(0, 0);
      lblStatus.Text = "Finished.";
    } else {
      lblStatus.Text = "Stopped. Read again to continue from the highlighted word.";
    }
    UpdateTextInfo();
  }

  private void Engine_SegmentStarted(int index) {
    if (_state != ReadState.Idle) lblStatus.Text = $"Reading part {index + 1} of {_segments.Count}";
  }

  private void Engine_WordReached(int index, int position, int length) {
    if (_state == ReadState.Idle || index >= _segments.Count) return;
    var (start, end) = _segments[index].MapToSource(position, length);
    if (end > txtMain.Text.Length) return;

    txtMain.Select(start, end - start);
    var line = txtMain.GetLineIndexFromCharacterIndex(start);
    if (line >= 0 && (line < txtMain.GetFirstVisibleLineIndex() || line > txtMain.GetLastVisibleLineIndex() - 1))
      txtMain.ScrollToLine(line);
  }

  private void SetState(ReadState state) {
    _state = state;
    var busy = state != ReadState.Idle;

    (icoRead.Text, lblRead.Text) = state switch {
      ReadState.Reading => (PauseIcon, "Pause"),
      ReadState.Paused => (PlayIcon, "Resume"),
      _ => (PlayIcon, "Read"),
    };
    btnStop.IsEnabled = busy;
    btnPaste.IsEnabled = btnOpen.IsEnabled = !busy;
    cmbVoice.IsEnabled = cmbMode.IsEnabled = chkReadCode.IsEnabled = !busy;
    txtMain.IsReadOnly = busy;
    if (state == ReadState.Paused) lblStatus.Text = "Paused.";
  }

  // ---- Text in --------------------------------------------------------------------------------

  private void SetText(string text, string? source) {
    txtMain.Text = text;
    txtMain.CaretIndex = 0;
    txtMain.ScrollToHome();
    Title = source is null ? AppTitle : $"{Path.GetFileName(source)} · {AppTitle}";
    lblStatus.Text = "";
  }

  private void LoadFile(string path) {
    try {
      SetText(File.ReadAllText(path), path);
    } catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) {
      MessageBox.Show(this, ex.Message, "Could not open file", MessageBoxButton.OK, MessageBoxImage.Warning);
    }
  }

  private void Paste_Click(object sender, RoutedEventArgs e) {
    if (Clipboard.ContainsText()) SetText(Clipboard.GetText(), null);
  }

  private void Open_Click(object sender, RoutedEventArgs e) {
    var dialog = new OpenFileDialog {
      Filter = "Text and Markdown|*.md;*.markdown;*.txt|All files|*.*",
    };
    if (dialog.ShowDialog(this) == true) LoadFile(dialog.FileName);
  }

  private void Text_PreviewDragOver(object sender, DragEventArgs e) {
    if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
    e.Effects = _state == ReadState.Idle ? DragDropEffects.Copy : DragDropEffects.None;
    e.Handled = true;
  }

  private void Text_PreviewDrop(object sender, DragEventArgs e) {
    if (_state != ReadState.Idle || e.Data.GetData(DataFormats.FileDrop) is not string[] { Length: > 0 } files) return;
    LoadFile(files[0]);
    e.Handled = true;
  }

  private void Text_TextChanged(object sender, TextChangedEventArgs e) => UpdateTextInfo();

  private void UpdateTextInfo() {
    if (_loading) return;
    lblPlaceholder.Visibility = txtMain.Text.Length == 0 ? Visibility.Visible : Visibility.Collapsed;
    if (_state != ReadState.Idle) return;

    var text = txtMain.Text;
    lblKind.Text = text.Length == 0 ? "" : SelectedMode switch {
      TextMode.Markdown => "Markdown",
      TextMode.PlainText => "Plain text",
      _ => SpeechScript.LooksLikeMarkdown(text) ? "Markdown (detected)" : "Plain text (detected)",
    };
  }

  // ---- Settings -------------------------------------------------------------------------------

  private TextMode SelectedMode => (cmbMode.SelectedItem as ModeOption)?.Mode ?? TextMode.Auto;

  private void UpdateRateLabel() {
    var rate = (int)sldRate.Value;
    lblRate.Text = rate > 0 ? $"+{rate}" : rate.ToString();
  }

  private void Rate_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
    if (_loading) return;
    // Takes effect from the next paragraph when changed mid-read.
    _engine?.SetRate((int)sldRate.Value);
    UpdateRateLabel();
  }

  private void Mode_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateTextInfo();

  // ---- Window ---------------------------------------------------------------------------------

  private void Read_Click(object sender, RoutedEventArgs e) => ToggleReading();

  private void Stop_Click(object sender, RoutedEventArgs e) => StopReading();

  private void Window_PreviewKeyDown(object sender, KeyEventArgs e) {
    var ctrl = Keyboard.Modifiers == ModifierKeys.Control;
    if (e.Key == Key.F5 || (ctrl && e.Key == Key.Enter)) ToggleReading();
    else if (e.Key == Key.Escape && _state != ReadState.Idle) StopReading();
    else if (ctrl && e.Key == Key.O && _state == ReadState.Idle) Open_Click(sender, e);
    else return;
    e.Handled = true;
  }

  private void Window_Closing(object? sender, System.ComponentModel.CancelEventArgs e) {
    StopReading();
    _settings.Voice = (cmbVoice.SelectedItem as VoiceOption)?.Id;
    _settings.Rate = (int)sldRate.Value;
    _settings.Mode = SelectedMode;
    _settings.ReadCode = chkReadCode.IsChecked == true;
    _settings.Save();
    foreach (var engine in _engines) engine.Dispose();
  }
}
