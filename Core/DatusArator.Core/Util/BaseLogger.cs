using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DatusArator.Core.Util {
  public class BaseLogger {
    public static int LOG_LEVEL_ERROR = 0;
    public static int LOG_LEVEL_INFO = 1;
    public static int LOG_LEVEL_DEBUG = 2;

    private static Action<String> fCallback;

    public static void setCallback(Action<String> callback) { fCallback = callback; }

    private static string fLogDirectory = null;
    public static String LogDirectory {
      get { return fLogDirectory; }
      set { fLogDirectory = FileUtils.AddSlash(value); }
    }

    #region Main File Logging
    public static int LogLevel { get; set; }
    public static string LogLevelAsString {
      get { return LogLevel == 0 ? "ERROR" : LogLevel == 1 ? "INFO" : "DEBUG"; }
      set {
        LogLevel = LOG_LEVEL_ERROR;
        if ("DEBUG".Equals(value.ToUpper()))
          LogLevel = LOG_LEVEL_DEBUG;
        if ("INFO".Equals(value.ToUpper()))
          LogLevel = LOG_LEVEL_INFO;
      }
    }

    public static void LogDebug(String info) {
      if (LogLevel >= LOG_LEVEL_DEBUG)
        Log("Debug", info);
    }

    public static void LogInfo(String info) {
      if (LogLevel >= LOG_LEVEL_INFO)
        Log("Info", info);
    }

    public static void LogError(String error) {
      Log("Error", error);
    }

    private static void Log(String messageType, String message) {
      string logText = buildMessage(messageType, message);

      if (fCallback != null)
        fCallback.Invoke(logText);
      else if (fLogDirectory != null)
        FileUtils.AppendToFile(logText, LogDirectory + DateTime.Now.ToString("yyyyMMdd") + ".log");
      else
        Console.Write(logText);
    }

    public static void LogSpace() {
      if (fCallback != null)
        fCallback.Invoke(Environment.NewLine);
      else if (fLogDirectory != null)
        FileUtils.AppendToFile(Environment.NewLine, LogDirectory + DateTime.Now.ToString("yyyyMMdd") + ".log");
      else
        Console.Write(Environment.NewLine);
    }

    private static string buildMessage(string messageType, string message) {
      StringBuilder result = new StringBuilder();
      result.Append("[").Append(messageType).Append("]");
      result.Append("\t").Append(String.Format("{0:yyyyMMdd HHmmss}", DateTime.Now));
      result.Append("\t").Append(message).Append(Environment.NewLine);

      return result.ToString();
    }
    #endregion
  }
}
