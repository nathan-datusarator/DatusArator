using DatusArator.Core.Json;
using DatusArator.Core.ObjectStore;
using DatusArator.Core.Util;
using System;
using System.Collections.Generic;

namespace DatusArator.DB.TcpServer.Handlers {
  public static class RequestHandler {
    #region Session
    public static JsonWrapper ProcessSession(JsonWrapper request) {
      var config = request.Get("config");
      var sessionId = request.Get("session");
      var action = request.Get("action");

      var result = new JsonWrapper();

      if (!string.IsNullOrEmpty(config)) {
        var session = ApiHelper.CreateSession(config);

        if (session == null) {
          result["code"] = 400;
          result["error"] = "Unable to create Object Store.  Invalid Config";
        } else {
          result["code"] = 200;
          result["id"] = session?.Id;
        }
      } else if (!string.IsNullOrEmpty(sessionId)) {
        if ("close".Equals(action, System.StringComparison.CurrentCultureIgnoreCase)) {
          ApiHelper.CloseSession(sessionId);
        } else if ("check".Equals(action, System.StringComparison.CurrentCultureIgnoreCase)) {
          if (ApiHelper.GetSession(sessionId) == null) {
            result["code"] = 400;
            result["error"] = "Invalid Session Id";
          } else {
            result["code"] = 200;
            result["success"] = "Valid Session Id";
          }
        } else {
          result["code"] = 400;
          result["error"] = "Unknown action.  Expecting 'close' or 'check'";
        }
      } else {
        result["code"] = 400;
        result["error"] = "Must provide a config or a session";
      }

      return result;
    }
    #endregion

    #region Data Commands
    public static JsonWrapper ProcessData(JsonWrapper request) {
      var result = new JsonWrapper();

      var session = ApiHelper.GetSession(request.Get("session"));
      if (session != null) {
        try {
          ProcessDataCommand(session, request, result);
        } catch (Exception ex) {
          result["code"] = 404;
          result["error"] = ex.Message;
        }
      } else {
        ResultError(result, "Unknown or missing session");
      }

      return result;
    }

    private static void ProcessDataCommand(ApiSession session, JsonWrapper parser, JsonWrapper result) {
      var cmd = parser.Get("cmd");
      if (string.IsNullOrEmpty(cmd)) {
        ResultError(result, "Missing cmd");
        return;
      }

      switch (cmd.ToLower()) {
        case "buckets":
          ProcessBuckets(session, parser, result); break;
        case "ids":
          ProcessIds(session, parser, result); break;
        case "get":
          ProcessGet(session, parser, result); break;
        case "put":
          ProcessPut(session, parser, result); break;
        case "clear":
          ProcessClear(session, parser, result); break;
        case "delete":
          ProcessDelete(session, parser, result); break;
        case "records":
          ProcessRecords(session, parser, result); break;
        case "search":
          ProcessSearch(session, parser, result); break;
        case "commit":
          ProcessCommit(session, parser, result); break;
        default:
          ResultError(result, "Unknown cmd: " + cmd);
          break;
      }
    }

    private static void ProcessBuckets(ApiSession session, JsonWrapper parser, JsonWrapper result) {
      var prefix = parser.Get("prefix");
      prefix = (prefix == null) ? "" : prefix;

      var buckets = session.Storage.Buckets(prefix);

      result["code"] = 200;
      result.SetArrayAsList("buckets", buckets);
    }

    private static void ProcessIds(ApiSession session, JsonWrapper parser, JsonWrapper result) {
      var bucket = parser.Get("bucket");
      if (string.IsNullOrEmpty(bucket)) {
        result["code"] = 400;
        result["error"] = "Missing required parameter: bucket";
      } else {
        var ids = session.Storage.Ids(bucket);

        result["code"] = 200;
        result.SetArrayAsList("ids", ids);
      }
    }

    private static void ProcessGet(ApiSession session, JsonWrapper parser, JsonWrapper result) {
      var bucket = parser.Get("bucket");
      var key = parser.Get("key");

      if (string.IsNullOrEmpty(bucket))
        ResultError(result, "Missing Required Value: bucket");
      else if (!string.IsNullOrEmpty(key)) {
        var data = session.Storage.Get(bucket, key);
        var raw = parser.GetAsBool("raw", false);

        if (!string.IsNullOrEmpty(data)) {
          result["code"] = 200;
          result["value"] = raw ? data : CryptoUtils.GzipBase64String(data);
        } else {
          result["code"] = 200;
        }
      } else if (parser.GetArrayLength("keys") > 0) {
        var ids = parser.GetArrayAsList("keys");
        var data = session.Storage.Get(bucket, ids);
        var raw = parser.GetAsBool("raw", false);

        result["code"] = 200;
        result["count"] = data.Count;
        foreach (var item in data) {
          result.AddToArray("data", raw ? item : CryptoUtils.GzipBase64String(item));
        }
      } else {
        ResultError(result, "Missing either 'key' or 'keys'");
      }
    }

    private static void ProcessPut(ApiSession session, JsonWrapper parser, JsonWrapper result) {
      var bucket = parser.Get("bucket");
      var key = parser.Get("key");

      if (string.IsNullOrEmpty(bucket))
        ResultError(result, "Missing Required Value: bucket");
      else if (string.IsNullOrEmpty(key))
        ResultError(result, "Missing Required Value: key");
      else {
        var value = parser.Get("value");

        var raw = parser.GetAsBool("raw", false);
        if (!raw)
          value = CryptoUtils.FromGzipBase64String(value);

        session.Storage.Put(bucket, key, value, GetSearchFields(parser));

        result["code"] = 200;
      }
    }

    private static List<ObjectStoreSearchField> GetSearchFields(JsonWrapper parser) {
      var result = new List<ObjectStoreSearchField>();
      foreach (var item in parser.GetArrayAsObjects("search"))
        result.Add(ObjectStoreSearchField.FromJson(item));

      return (result.Count > 0) ? result : null;
    }

    private static void ProcessClear(ApiSession session, JsonWrapper parser, JsonWrapper result) {
      var bucket = parser.Get("bucket");
      if (string.IsNullOrEmpty(bucket)) {
        ResultError(result, "Missing required parameter: bucket");
      } else {
        session.Storage.Clear(bucket);
        result["code"] = 200;
      }
    }

    private static void ProcessDelete(ApiSession session, JsonWrapper parser, JsonWrapper result) {
      var bucket = parser.Get("bucket");
      var key = parser.Get("key");

      if (string.IsNullOrEmpty(bucket)) {
        ResultError(result, "Missing required parameter: bucket");
      } else if (string.IsNullOrEmpty(key)) {
        ResultError(result, "Missing required parameter: key");
      } else {
        result["code"] = 200;
        result["success"] = session.Storage.Delete(bucket, key);
      }
    }

    private static void ProcessRecords(ApiSession session, JsonWrapper parser, JsonWrapper result) {
      var bucket = parser.Get("bucket");
      var raw = parser.GetAsBool("raw", false);

      if (string.IsNullOrEmpty(bucket)) {
        ResultError(result, "Missing required parameter: bucket");
      } else {
        foreach (var item in session.Storage.Records(bucket)) {
          var data = item.Key + "|-|" + item.Value;
          result.AddToArray("data", raw ? data : CryptoUtils.GzipBase64String(data));
        }
        result["code"] = 200;
      }
    }

    private static void ProcessSearch(ApiSession session, JsonWrapper parser, JsonWrapper result) {
      var field = parser.Get("field");
      var value = parser.Get("value");
      var raw = parser.GetAsBool("raw", false);

      if (string.IsNullOrEmpty(field) || string.IsNullOrEmpty(value)) {
        ResultError(result, "Missing required parameter: bucket and/or field");
      } else {
        var results = session.Storage.Search(field, value);
        if (results == null) {
          result["code"] = 400;
          result["error"] = "Object Store does not support search";
          return;
        }

        foreach (var item in results) {
          var data = item.Key + "|-|" + item.Value;
          result.AddToArray("data", raw ? data : CryptoUtils.GzipBase64String(data));
        }
        result["code"] = 200;
      }
    }

    private static void ProcessCommit(ApiSession session, JsonWrapper parser, JsonWrapper result) {
      try {
        var force = parser.GetAsBool("force");
        session.Storage.Commit(force);

        result["code"] = 200;
      } catch (Exception ex) {
        ResultError(result, ex.Message);
      }
    }
    #endregion

    #region Helpers
    private static void ResultError(JsonWrapper result, string error) {
      result["code"] = 404;
      result["error"] = error;
    }
    #endregion
  }
}
