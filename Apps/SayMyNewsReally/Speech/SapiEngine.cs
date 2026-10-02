using System.Speech.Synthesis;
using SayMyNewsReally.Reading;

namespace SayMyNewsReally.Speech;

/// <summary>
/// Classic SAPI 5 voices (David, Zira, and anything registered by add-ons such as
/// NaturalVoiceSAPIAdapter). Must be created on the UI thread so its events arrive there.
/// </summary>
public sealed class SapiEngine : ISpeechEngine {
  private readonly SpeechSynthesizer _synth = new();
  private readonly Dictionary<Prompt, int> _queued = [];
  private Prompt? _lastPrompt;

  public event Action<int>? SegmentStarted;
  public event Action<int, int, int>? WordReached;
  public event Action? Finished;

  public SapiEngine() {
    _synth.SetOutputToDefaultAudioDevice();
    _synth.SpeakStarted += (_, e) => {
      if (_queued.TryGetValue(e.Prompt, out var index)) SegmentStarted?.Invoke(index);
    };
    _synth.SpeakProgress += (_, e) => {
      if (_queued.TryGetValue(e.Prompt, out var index)) WordReached?.Invoke(index, e.CharacterPosition, e.CharacterCount);
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
      var prompt = new Prompt(segments[i].Text);
      _queued[prompt] = i;
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
}
