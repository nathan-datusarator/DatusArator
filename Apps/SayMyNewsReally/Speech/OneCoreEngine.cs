using System.Speech.Synthesis;
using Windows.Media.Core;
using Windows.Media.Playback;
using SayMyNewsReally.Reading;
using WinRtSynthesizer = Windows.Media.SpeechSynthesis.SpeechSynthesizer;
using WinRtSynthesisStream = Windows.Media.SpeechSynthesis.SpeechSynthesisStream;

namespace SayMyNewsReally.Speech;

/// <summary>
/// The newer Windows voices (Settings › Time &amp; language › Speech › Add voices), which the
/// classic SAPI API cannot see. Each segment is synthesized to a stream with word-boundary
/// metadata and played through a MediaPlayer; the next segment is synthesized while the
/// current one plays. Must be created on the UI thread.
/// </summary>
public sealed class OneCoreEngine : ISpeechEngine {
  private readonly WinRtSynthesizer _synth = new();
  private readonly MediaPlayer _player = new();
  private readonly SynchronizationContext _ui = SynchronizationContext.Current
    ?? throw new InvalidOperationException("Create the engine on the UI thread.");
  private CancellationTokenSource? _run;
  private volatile bool _paused;
  private volatile bool _playing;

  public event Action<int>? SegmentStarted;
  public event Action<int, int, int>? WordReached;
  public event Action? Finished;

  public OneCoreEngine() {
    _synth.Options.IncludeWordBoundaryMetadata = true;
    Voices = WinRtSynthesizer.AllVoices
      .Select(v => new VoiceOption("onecore:" + v.Id, VoiceLabel.Of(v.DisplayName, v.Language), this))
      .ToList();
  }

  public IReadOnlyList<VoiceOption> Voices { get; }

  public void Start(IReadOnlyList<SpokenSegment> segments, int first, VoiceOption voice, int rate) {
    Stop();
    var id = voice.Id["onecore:".Length..];
    _synth.Voice = WinRtSynthesizer.AllVoices.First(v => v.Id == id);
    SetRate(rate);
    _paused = false;
    _run = new CancellationTokenSource();
    _ = RunAsync(segments, first, _run.Token);
  }

  public void Pause() {
    _paused = true;
    if (_playing) _player.Pause();
  }

  public void Resume() {
    _paused = false;
    if (_playing) _player.Play();
  }

  public void Stop() {
    _run?.Cancel();
    _run = null;
    _playing = false;
    _player.Pause();
    _player.Source = null;
  }

  /// <summary>SAPI's -10..+10 scale is roughly a third to three times normal speed.</summary>
  public void SetRate(int rate) => _synth.Options.SpeakingRate = Math.Clamp(Math.Pow(3, rate / 10.0), 0.5, 6.0);

  public void Dispose() {
    Stop();
    _player.Dispose();
    _synth.Dispose();
  }

  private async Task RunAsync(IReadOnlyList<SpokenSegment> segments, int first, CancellationToken token) {
    try {
      Task<WinRtSynthesisStream>? next = Synthesize(segments[first].Text);
      for (var i = first; i < segments.Count; i++) {
        // Always set here: the previous pass prefetched segment i.
        var stream = await next!;
        token.ThrowIfCancellationRequested();
        next = i + 1 < segments.Count ? Synthesize(segments[i + 1].Text) : null;

        await WhileOnPause(token);
        SegmentStarted?.Invoke(i);
        await PlayAsync(stream, i, token);
        await Task.Delay(GapMilliseconds(segments[i].PauseAfter), token);
      }
      Finished?.Invoke();
    } catch (OperationCanceledException) {
      // Stopped.
    }
  }

  private Task<WinRtSynthesisStream> Synthesize(string text) => _synth.SynthesizeTextToStreamAsync(text).AsTask();

  private async Task PlayAsync(WinRtSynthesisStream stream, int index, CancellationToken token) {
    var item = new MediaPlaybackItem(MediaSource.CreateFromStream(stream, stream.ContentType));
    for (var t = 0; t < item.TimedMetadataTracks.Count; t++) {
      var track = item.TimedMetadataTracks[t];
      if (track.Id != "SpeechWord") continue;
      track.CueEntered += (_, args) => {
        if (args.Cue is not SpeechCue { StartPositionInInput: int start, EndPositionInInput: int end }) return;
        _ui.Post(_ => {
          if (!token.IsCancellationRequested) WordReached?.Invoke(index, start, end - start + 1);
        }, null);
      };
      item.TimedMetadataTracks.SetPresentationMode((uint)t, TimedMetadataTrackPresentationMode.ApplicationPresented);
    }

    var done = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    void Ended(MediaPlayer p, object o) => done.TrySetResult();
    void Failed(MediaPlayer p, MediaPlayerFailedEventArgs e) => done.TrySetException(new InvalidOperationException(e.ErrorMessage));
    _player.MediaEnded += Ended;
    _player.MediaFailed += Failed;
    using var cancel = token.Register(() => done.TrySetCanceled(token));
    try {
      _player.Source = item;
      _playing = true;
      _player.Play();
      await done.Task;
    } finally {
      _playing = false;
      _player.MediaEnded -= Ended;
      _player.MediaFailed -= Failed;
    }
  }

  private async Task WhileOnPause(CancellationToken token) {
    while (_paused) await Task.Delay(100, token);
  }

  // The synthesized audio already ends with a short silence, so these only add to it.
  private static int GapMilliseconds(PromptBreak pause) => pause switch {
    PromptBreak.Small => 150,
    PromptBreak.Medium => 350,
    PromptBreak.Large => 700,
    PromptBreak.ExtraLarge => 1000,
    _ => 0,
  };
}
