using System.IO;
using System.Text.Json;
using SayMyNewsReally.Reading;

namespace SayMyNewsReally;

/// <summary>Remembers the reader's choices between runs (%AppData%\SayMyNewsReally\settings.json).</summary>
public sealed class AppSettings {
  public string? Voice { get; set; }
  public int Rate { get; set; }
  public TextMode Mode { get; set; } = TextMode.Auto;
  public bool ReadCode { get; set; }

  private static string FilePath => Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SayMyNewsReally", "settings.json");

  public static AppSettings Load() {
    try {
      return File.Exists(FilePath)
        ? JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(FilePath)) ?? new()
        : new();
    } catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException) {
      return new();
    }
  }

  public void Save() {
    try {
      Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
      File.WriteAllText(FilePath, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
    } catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) {
      // Losing preferences is not worth interrupting the user over.
    }
  }
}
