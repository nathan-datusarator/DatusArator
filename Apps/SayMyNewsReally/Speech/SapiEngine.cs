using System.Speech.Synthesis;
using System.Text;
using SayMyNewsReally.Reading;

namespace SayMyNewsReally.Speech;

/// <summary>
/// Classic SAPI 5 voices (David, Zira, and anything registered by add-ons such as
/// NaturalVoiceSAPIAdapter). Must be created on the UI thread so its events arrive there.
/// </summary>
public sealed class SapiEngine : ISpeechEngine {
  // SAPI sends text to the voice as SSML, so & < > reach it as &amp; &lt; &gt;. Some voices
  // (NaturalVoiceSAPIAdapter's "Online" voices) then report word positions counted in the SSML,
  // and System.Speech throws on its own worker thread taking the word out of the prompt, which
  // kills the process. Speaking these as words keeps them out of the SSML entirely.
  private static readonly Dictionary<char, string> Spoken = new() {
    ['&'] = " and ", ['<'] = " less than ", ['>'] = " greater than ",
  };

  private readonly SpeechSynthesizer _synth = new();
  private readonly Dictionary<Prompt, (int Index, int[] ToSegment)> _queued = [];
  private Prompt? _lastPrompt;

  public event Action<int>? SegmentStarted;
  public event Action<int, int, int>? WordReached;
  public event Action? Finished;

  public SapiEngine() {
    _synth.SetOutputToDefaultAudioDevice();
    _synth.SpeakStarted += (_, e) => {
      if (_queued.TryGetValue(e.Prompt, out var queued)) SegmentStarted?.Invoke(queued.Index);
    };
    _synth.SpeakProgress += (_, e) => {
      if (!_queued.TryGetValue(e.Prompt, out var queued) || e.CharacterCount <= 0) return;
      var map = queued.ToSegment;
      var start = map[Math.Min(e.CharacterPosition, map.Length - 1)];
      var end = map[Math.Min(e.CharacterPosition + e.CharacterCount - 1, map.Length - 1)] + 1;
      WordReached?.Invoke(queued.Index, start, Math.Max(end - start, 1));
    };
    _synth.SpeakCompleted += (_, e) => {
      _queued.Remove(e.Prompt);
      if (e.Prompt == _lastPrompt && !e.Cancelled) {
        _lastPrompt = null;
        Finished?.Invoke();
      }
    };

    Voices = _synth.GetInstalledVoices()
      .Where(v => v.Enabled)
      .Select(v => new VoiceOption("sapi:" + v.VoiceInfo.Name, VoiceLabel.Of(v.VoiceInfo.Name, v.VoiceInfo.Culture.Name), this))
      .ToList();
  }

  public IReadOnlyList<VoiceOption> Voices { get; }

  public void Start(IReadOnlyList<SpokenSegment> segments, int first, VoiceOption voice, int rate) {
    _synth.SelectVoice(voice.Id["sapi:".Length..]);
    _synth.Rate = rate;
    _queued.Clear();

    for (var i = first; i < segments.Count; i++) {
      var (text, toSegment) = WithoutMarkup(segments[i].Text);
      var prompt = new Prompt(text);
      _queued[prompt] = (i, toSegment);
      _synth.SpeakAsync(prompt);
      _lastPrompt = prompt;

      if (segments[i].PauseAfter != PromptBreak.None) {
        var pause = new PromptBuilder();
        pause.AppendBreak(segments[i].PauseAfter);
        _lastPrompt = new Prompt(pause);
        _synth.SpeakAsync(_lastPrompt);
      }
    }
  }

  public void Pause() => _synth.Pause();

  public void Resume() => _synth.Resume();

  public void Stop() {
    var wasPaused = _synth.State == SynthesizerState.Paused;
    _queued.Clear();
    _lastPrompt = null;
    _synth.SpeakAsyncCancelAll();
    // A paused synthesizer holds cancelled prompts until it is resumed.
    if (wasPaused) _synth.Resume();
  }

  public void SetRate(int rate) => _synth.Rate = rate;

  public void Dispose() => _synth.Dispose();

  /// <summary>
  /// Replaces the characters in <see cref="Spoken"/> and returns, for each char of the result,
  /// the position in <paramref name="text"/> it came from (plus one past the end).
  /// </summary>
  private static (string Text, int[] ToSegment) WithoutMarkup(string text) {
    var result = new StringBuilder(text.Length);
    var map = new List<int>(text.Length + 1);
    for (var i = 0; i < text.Length; i++) {
      var piece = Spoken.TryGetValue(text[i], out var words) ? words : text[i].ToString();
      result.Append(piece);
      for (var k = 0; k < piece.Length; k++) map.Add(i);
    }
    map.Add(text.Length);
    return (result.ToString(), [.. map]);
  }
}
