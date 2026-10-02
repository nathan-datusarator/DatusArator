using SayMyNewsReally.Reading;

namespace SayMyNewsReally.Speech;

public sealed record VoiceOption(string Id, string Label, ISpeechEngine Engine);

/// <summary>
/// A Windows voice system that can read a list of segments with pause, resume and word progress.
/// Events are raised on the UI thread. Stop does not raise <see cref="Finished"/>.
/// </summary>
public interface ISpeechEngine : IDisposable {
  IReadOnlyList<VoiceOption> Voices { get; }

  /// <summary>Segment index started speaking.</summary>
  event Action<int>? SegmentStarted;

  /// <summary>Segment index, character position and length within that segment's text.</summary>
  event Action<int, int, int>? WordReached;

  event Action? Finished;

  /// <param name="rate">-10 (slowest) to +10 (fastest), 0 is normal.</param>
  void Start(IReadOnlyList<SpokenSegment> segments, int first, VoiceOption voice, int rate);
  void Pause();
  void Resume();
  void Stop();

  /// <summary>Applies from the next segment when changed mid-read.</summary>
  void SetRate(int rate);
}
