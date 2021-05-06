using DatusArator.Core.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;


namespace DatusArator.Core.Logging {
  public enum DatusAratorLogLevel { DEBUG, INFO, ERROR }

  public struct DatusAratorLogEntry {
    public string Sender;
    public DatusAratorLogLevel Level;
    public DateTime LogDate;
    public string Message;
  }

  public class DatusAratorLogger : List<DatusAratorLogEntry> {
    #region Vanilla Shortcuts
    public static void Log(string message) {
      Log(DatusAratorLogLevel.INFO, message);
    }

    public static void Err(string message) {
      Log(DatusAratorLogLevel.ERROR, message);
    }

    public static void Err(Exception ex) {
      Log(DatusAratorLogLevel.ERROR, ex.Message + Environment.NewLine + ex.StackTrace);
    }

    public static void Debug(string message) {
      Log(DatusAratorLogLevel.DEBUG, message);
    }
    #endregion

    public static void Log(DatusAratorLogLevel level, string message) {
      Log(null, level, message);
    }

    public static void Log(object sender, DatusAratorLogLevel level, string message) {
      var className = sender?.GetType().ToString();

      if (level >= Instance.MinLevel)
        Instance.Add(new DatusAratorLogEntry() { Sender = className, Level = level, LogDate = DateTime.Now, Message = message });

      Instance.OnLog?.Invoke(sender, level, message);
    }

    private static DatusAratorLogger instance;
    public static DatusAratorLogger Instance {
      get {
        if (instance == null)
          instance = new DatusAratorLogger();
        return instance;
      }
    }

    public Action<object, DatusAratorLogLevel, string> OnLog { get; set; }

    private string logDir = FileUtils.FileRoot + @"logs\";
    public string LogDir {
      get { return logDir; }
      set { logDir = FileUtils.AddSlash(value); }
    }

    public DatusAratorLogLevel MinLevel { get; set; }

    public void Save() {
      if (Count <= 0)
        return;

      var filename = string.Format("log_{0:yyyymmdd_hhmmss}.txt", DateTime.Now);

      Directory.CreateDirectory(LogDir);

      StringBuilder data = new StringBuilder();
      foreach(var item in this) {
        data.Append(string.Format("{0}\t{1}\t{2}\n", item.Level.ToString(), item.LogDate, item.Message));
      }
     
      FileUtils.StringToFile(data.ToString(), LogDir + filename);
    }
  }
}
