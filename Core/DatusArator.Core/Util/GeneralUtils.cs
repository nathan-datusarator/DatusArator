using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

using DatusArator.Core.Interfaces;
using DatusArator.Core.Json;

namespace DatusArator.Core.Util {
  public static class GeneralUtils {
    public static double DPI_RATIO_X { get; set; } = 1.0;
    public static double DPI_RATIO_Y { get; set; } = 1.0;

    public static IProgressConsumer GlobalProgress = null;

    private static readonly Dictionary<string, object> fGlobalCache = new Dictionary<string, object>();

    public static T GetCachedObject<T>(string key) where T : class {
      if (fGlobalCache.ContainsKey(key))
        return fGlobalCache[key] as T;

      return default;
    }

    public static void SetCachedObject(string key, object value) {
      fGlobalCache[key] = value;
    }

    public static Action<string> OnGlobalError;
    public static void RaiseGlobalError(string message, bool addTimestamp = false) {
      if (addTimestamp)
        message = "[" + DateTime.Now.ToString("yyyyMMdd HH:mm:ss") + "] " + message;
      OnGlobalError?.Invoke(message);
    }

    public static string AddTimestamp(string message, bool addMilliseconds = false) {
      if (addMilliseconds)
        return "[" + DateTime.Now.ToString("yyyyMMdd HH:mm:ss.fff") + "] " + message;
      else
        return "[" + DateTime.Now.ToString("yyyyMMdd HH:mm:ss") + "] " + message;
    }

    public static Stream BytesToStream(byte[] bytes) {
      if (bytes == null)
        return null;

      MemoryStream ms = new MemoryStream(bytes);
      ms.Write(bytes, 0, bytes.Length);
      ms.Position = 0;

      return ms;
    }

    #region Change Type
    private static readonly Type STRING_TYPE = typeof(string);
    private static readonly Type BOOL_TYPE = typeof(bool);
    private static readonly Type DECIMAL_TYPE = typeof(decimal);
    private static readonly Type DATE_TIME_TYPE = typeof(DateTime);
    private static readonly Type INT_TYPE = typeof(int);
    private static readonly Type LONG_TYPE = typeof(long);

    public static object ChangeType(object value, Type conversion) {
      if (value == null)
        return null;

      if (value.GetType() == conversion)
        return value;

      var destType = conversion;

      if (destType.IsGenericType && destType.GetGenericTypeDefinition().Equals(typeof(Nullable<>))) {
        if (value == null)
          return null;

        destType = Nullable.GetUnderlyingType(destType);
      }

      if (destType == STRING_TYPE)
        return value.ToString();

      if (value.GetType() == STRING_TYPE) {
        if (destType == INT_TYPE)
          return StringUtils.SafeStrToInt(value as string, null, true) ?? null;
        else if (destType == DECIMAL_TYPE)
          return StringUtils.SafeStrToDecimal(value as string, null, true) ?? null;
        else if (destType == BOOL_TYPE)
          return StringUtils.IsTrue(value as string);
        else if (destType == DATE_TIME_TYPE)
          return StringUtils.SafeStrToDate(value as string, null) ?? null;
      }

      if ((value.GetType() == LONG_TYPE) && (destType == DATE_TIME_TYPE))
        return TypedBasicObject.JavaTimeToNet((value as long?).Value);

      var converter = TypeDescriptor.GetConverter(destType);
      if ((converter != null) && converter.CanConvertFrom(value.GetType()))
        return converter.ConvertFrom(value);

      return Convert.ChangeType(value, destType);
    }

    public static string[] PropertyNames(Type type, BindingFlags flags = BindingFlags.Instance | BindingFlags.Public) {
      var properties = type.GetProperties(flags);
      return properties.Select(x => x.Name).ToArray();
    }

    public static string[] FieldNames(Type type, BindingFlags flags = BindingFlags.Instance | BindingFlags.Public) {
      return type.GetFields(flags).Select(x => x.Name).ToArray();
    }
    #endregion

    #region Heartbeat
    // Heartbeat should be every 500 milliseconds
    // Pulse should be every 10 milliseconds

    public static bool HasPulse = false;
    public static bool HasHeartbeat = false;

    public static Action OnPulse;
    public static Action OnHeartbeat;

    private static int fPulses = 0;
    private static PulseThread fPulseThread;
    public static void StartPulse() {
      HasHeartbeat = true;
      HasPulse = true;

      fPulseThread = new PulseThread() {
        Rate = 10,
      };

      fPulseThread.Pulse += (s, e) => {
        OnPulse?.Invoke();
        if (++fPulses % 50 == 0)
          OnHeartbeat?.Invoke();
      };
      new Thread(new ThreadStart(fPulseThread.Start)).Start();
    }

    public static void StopPulse() {
      fPulseThread?.Abort();
      fPulseThread = null;

      HasHeartbeat = false;
      HasPulse = false;
    }

    public static double Evaluate(string expression) {
      var loDataTable = new DataTable();
      var loDataColumn = new DataColumn("Eval", typeof(double), expression);
      loDataTable.Columns.Add(loDataColumn);
      loDataTable.Rows.Add(0);
      return (double)(loDataTable.Rows[0]["Eval"]);
    }
    #endregion

    #region Settings
    private static JsonWrapper fSettings = new JsonWrapper();
    public static void SettingsInit(string json) {
      fSettings = new JsonWrapper(json);
    }

    public static string SettingsAsJson() {
      return fSettings.ToJsonString();
    }

    public static string SettingsGet(string key) {
      return fSettings.Get(key);
    }

    public static void SettingsPut(string key, string value) {
      fSettings[key] = value;
    }

    public static JsonWrapper SettingsSection(string key) {
      return new JsonWrapper(fSettings, key);
    }
    #endregion
  }
}
