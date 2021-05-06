using DatusArator.Core.Json;
using DatusArator.Core.ObjectStore;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace DatusArator.DB.TcpServer.Handlers {
  public static class ApiHelper {
    #region Sessions
    private static Dictionary<string, ApiSession> fSessions = new Dictionary<string, ApiSession>();

    public static ApiSession CreateSession(string config) {
      var result = new ApiSession(config);
      if (result.Storage == null)
        return null;

      fSessions[result.Id] = result;
      return result;
    }

    public static ApiSession GetSession(string id) {
      if (string.IsNullOrEmpty(id))
        return null;
      else if (fSessions.ContainsKey(id))
        return fSessions[id];
      else
        return null;
    }

    public static void CloseSession(string id) {
      var session = GetSession(id);
      if (session == null)
        return;

      var config = session.Config;
      fSessions.Remove(id);

      var found = false;
      foreach (var value in fSessions.Values) {
        if (config.Equals(value.Config, System.StringComparison.CurrentCultureIgnoreCase)) {
          found = true;
          break;
        }
      }

      if (!found)
        ApiSession.CloseObjectStore(config);

    }
    #endregion
  }

  public class ApiSession {
    public static int fNextId = 10;

    public string Id { get; set; }
    public string Config { get; set; }
    public IDatusAratorObjectStore Storage { get; set; }

    public ApiSession(string config) {
      Storage = GetStorage(config);
      if (Storage == null)
        return;

      Id = "" + fNextId++;
      Config = config;
    }

    public static Dictionary<string, IDatusAratorObjectStore> fObjectStores = new Dictionary<string, IDatusAratorObjectStore>();
    public static IDatusAratorObjectStore GetStorage(string config) {
      if (fObjectStores.ContainsKey(config))
        return fObjectStores[config];

      var result = DatusAratorObjectStoreFactory.BuildObjectStore(config);
      if (result == null)
        return null;

      fObjectStores[config] = result;
      return result;
    }

    public static void CloseObjectStore(string config) {
      if (!fObjectStores.ContainsKey(config))
        return;

      var storage = fObjectStores[config];
      fObjectStores.Remove(config);

      storage.Dispose();
    }
  }
}