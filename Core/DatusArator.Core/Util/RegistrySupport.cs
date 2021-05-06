using System;

using Microsoft.Win32;

namespace DatusArator.Core.Util {
  public static class RegistrySupport {
    public static string Read(RegistryKey baseKey, String subKeyName, string keyName) {
      var subKey = baseKey.OpenSubKey(subKeyName, false);
      return (subKey != null) ? (string)subKey.GetValue(keyName.ToUpper()) : null;
    }

    public static void CreateSubKey(RegistryKey baseKey, String subKeyName) {
      baseKey.CreateSubKey(subKeyName);
    }

    public static void Write(RegistryKey baseKey, String subKeyName, string keyName, string value) {
      var subKey = baseKey.CreateSubKey(subKeyName);
      if (subKey != null)
        subKey.SetValue(keyName.ToUpper(), value);
    }

    public static void Delete(RegistryKey baseKey, String subKeyName, string keyName) {
      var subKey = baseKey.CreateSubKey(subKeyName);

      if (subKey != null)
        subKey.DeleteValue(keyName.ToUpper());
    }

    public static void DeleteSubKeyTree(RegistryKey baseKey, String subKeyName) {
      if (baseKey.OpenSubKey(subKeyName) != null)
        baseKey.DeleteSubKeyTree(subKeyName);
    }
  }
}
