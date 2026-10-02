namespace SayMyNewsReally.Speech;

internal static class VoiceLabel {
  /// <summary>"Microsoft Zira Desktop - English (United States)" becomes "Zira (en-US)".</summary>
  public static string Of(string name, string language) {
    var dash = name.IndexOf(" - ", StringComparison.Ordinal);
    var shortName = (dash > 0 ? name[..dash] : name)
      .Replace("Microsoft ", "")
      .Replace(" Desktop", "")
      .Trim();
    return $"{shortName} ({language})";
  }
}
