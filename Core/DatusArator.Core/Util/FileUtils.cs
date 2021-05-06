using System;
using System.Linq;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace DatusArator.Core.Util {
  public static class FileUtils {
    public readonly static DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    // Always add the final slash
    public static String FileRoot = @"c:\temp\";

    public static String AddSlash(String path, char slash = '\0') {
      if (String.IsNullOrEmpty(path))
        return path;

      if (slash == '\0')
        slash = path.Contains('/') ? '/' : '\\';

      if (path[path.Length - 1] != slash)
        return path + slash;

      return path;
    }

    public static string FileToString(string filePath) {
      if (!File.Exists(filePath))
        return null;

      StreamReader streamReader = new StreamReader(filePath);
      string text = streamReader.ReadToEnd();
      streamReader.Close();

      return text;
    }

    public static void StringToFile(string value, string filePath) {
      using (StreamWriter outfile = new StreamWriter(filePath)) {
        outfile.Write(value);
      }
    }

    public static void AppendToFile(string value, string filePath) {
      using (StreamWriter outfile = File.AppendText(filePath)) {
        outfile.Write(value);
      }
    }

    public static string CleanFileName(string fileName) {
      return CleanFileName(fileName, string.Empty);
    }

    public static string CleanFileName(string fileName, string replaceWith) {
      return Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c.ToString(), replaceWith));
    }

    #region GZIP
    public static bool GZipFile(string fileName, string outFileName = null) {
      if (!File.Exists(fileName))
        return false;

      if (outFileName == null)
        outFileName = fileName + ".gz";

      byte[] data = GZipFileToBytes(fileName);
      if (data != null) {
        File.WriteAllBytes(outFileName, data);
        return true;
      }

      return false;
    }

    public static byte[] GZipFileToBytes(string fileName) {
      if (!File.Exists(fileName))
        return null;

      FileInfo fileInfo = new FileInfo(fileName);
      using (FileStream fileStream = fileInfo.OpenRead()) {
        return StringUtils.GZipData(fileStream);
      }
    }

    public static bool GUnzipFile(string fileName, string outFileName = null) {
      if (!File.Exists(fileName))
        return false;

      if (outFileName == null) {
        if (fileName.EndsWith(".gz"))
          outFileName = fileName.Substring(0, fileName.Length - 3);
        else
          return false;
      }

      byte[] data = GUnzipFileToBytes(fileName);
      if (data != null) {
        File.WriteAllBytes(outFileName, data);
        return true;
      }

      return false;
    }

    public static byte[] GUnzipFileToBytes(String fileName) {
      if (!File.Exists(fileName))
        return null;

      MemoryStream result = new MemoryStream();
      FileInfo fileInfo = new FileInfo(fileName);
      FileStream fileStream = fileInfo.OpenRead();
      using (var zipStream = new GZipStream(fileStream, CompressionMode.Decompress, false)) {
        zipStream.CopyTo(result);
      }

      return result.ToArray();
    }

    public async static Task GUnzipFile(string fileName, Func<string, bool> lineRead, Action onComplete = null) {
      if (!File.Exists(fileName))
        return;

      FileInfo fileInfo = new FileInfo(fileName);
      using (var fileStream = fileInfo.OpenRead())
        await StringUtils.FromGzipData(fileStream, lineRead, onComplete);
    }
    #endregion

    public static bool IsEmpty(string filename) {
      var info = new FileInfo(filename);
      return !info.Exists || (info.Length == 0);
    }

    public static void DeleteFolder(string tempPath) {
      foreach (FileInfo file in new DirectoryInfo(tempPath).GetFiles())
        file.Delete();
      Directory.Delete(tempPath);
    }

    public static List<string> AllFiles(string path) {
      var result = new List<string>();

      foreach (var file in Directory.GetFiles(path))
        result.Add(file);

      string[] dirs;
      try {
        dirs = Directory.GetDirectories(path);
      } catch (Exception) {
        return result;
      }

      foreach (var dir in dirs)
        result.AddRange(AllFiles(StringUtils.Concat(path, dir.Substring(path.Length), @"\")));

      return result;
    }

    public static List<string> Directories(string path) {
      var result = new List<string>();

      path = AddSlash(path);

      string[] dirs;
      try {
        dirs = Directory.GetDirectories(path);
        foreach (var dir in dirs)
          result.Add(dir.Substring(path.Length));
      } catch (Exception) {
      }

      return result;
    }

    public static void ClearDirectory(string directoryName) {
      DirectoryInfo directory = new DirectoryInfo(directoryName);

      foreach (FileInfo file in directory.GetFiles())
        file.Delete();

      foreach (DirectoryInfo subDirectory in directory.GetDirectories())
        subDirectory.Delete(true);
    }

    public static string LoadResource(Assembly assembly, string resourcePath) {
      using (Stream stream = assembly.GetManifestResourceStream(resourcePath)) {
        if (stream == null)
          throw new Exception("Unable to find resource: " + resourcePath);

        using (StreamReader reader = new StreamReader(stream)) {
          return reader.ReadToEnd();
        }
      }
    }

    public static byte[] LoadResourceBytes(Assembly assembly, string resourcePath) {
      using (Stream stream = assembly.GetManifestResourceStream(resourcePath)) {
        if (stream == null)
          throw new Exception("Unable to find resource: " + resourcePath);

        using (var memoryStream = new MemoryStream()) {
          stream.CopyTo(memoryStream);
          return memoryStream.ToArray();
        }
      }
    }
  }
}
