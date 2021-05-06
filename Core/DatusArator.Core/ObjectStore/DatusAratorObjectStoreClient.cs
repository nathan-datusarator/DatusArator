using DatusArator.Core.Http;
using DatusArator.Core.Json;
using DatusArator.Core.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace DatusArator.Core.ObjectStore {
  public class DatusAratorObjectStoreClient : DatusAratorBaseObjectStore {
    public string Server { get; set; }
    public string Config { get; set; }

    private readonly string SessionId;

    public override IDatusAratorObjectStore Build(string config) {
      return null;
    }

    public DatusAratorObjectStoreClient(string server, string config) {
      Server = server + (server.EndsWith("/") ? "" : "/");
      Config = config;

      var result = WebUtils.GetUri(new Uri(Server + "api/session?config=" + Config));
      var parser = new JsonWrapper(result);
      if (parser.Get("code") == "200")
        SessionId = parser.Get("id");
      else
        throw new Exception(parser.Get("error"));
    }

    protected override void Dispose(bool disposing) {
      if (disposing)
        WebUtils.GetUri(new Uri(Server + "api/session?session=" + SessionId + @"&action=close"));
    }

    private JsonWrapper SendCommand(JsonWrapper command) {
      command["session"] = SessionId;

      var options = new GetUriOptions {
        Uri = new Uri(Server + "api/data"),
        Method = HttpMethod.Post,
        PostContentType = "application/json",
        PostData = Encoding.ASCII.GetBytes(command.ToJsonString())
      };

      var data = WebUtils.GetUri(options);

      try {
        var result = new JsonWrapper(data);
        return result;
      } catch {
        var result = new JsonWrapper();
        result["code"] = 300;
        result["error"] = data;
        return result;
      }
    }

    public override List<string> Buckets(string prefix) {
      var command = new JsonWrapper();
      command["cmd"] = "buckets";
      command["prefix"] = prefix;

      var parser = SendCommand(command);
      if (parser.Get("code") == "200") {
        return parser.GetArrayAsList("buckets");
      }

      throw new Exception(parser.Get("error"));
    }

    public override List<string> Ids(string bucket) {
      var command = new JsonWrapper();
      command["cmd"] = "ids";
      command["bucket"] = bucket;

      var parser = SendCommand(command);
      if (parser.Get("code") == "200") {
        return parser.GetArrayAsList("ids");
      }

      throw new Exception(parser.Get("error"));
    }

    public override string Get(string bucket, string key) {
      var command = new JsonWrapper();
      command["cmd"] = "get";
      command["bucket"] = bucket;
      command["key"] = key;

      var parser = SendCommand(command);
      if (parser.Get("code") == "200")
        return CryptoUtils.FromGzipBase64String(parser.Get("value"));

      throw new Exception(parser.Get("error"));
    }

    public override List<string> Get(string bucket, string[] keys) {
      var command = new JsonWrapper();
      command["cmd"] = "get";
      command["bucket"] = bucket;
      command.SetArrayAsList("keys", new List<string>(keys));

      var parser = SendCommand(command);
      if (parser.Get("code") == "200") {
        var result = new List<string>();
        foreach (var item in parser.GetArrayAsList("data")) {
          result.Add(CryptoUtils.FromGzipBase64String(item));
        }

        return result;
      }

      throw new Exception(parser.Get("error"));
    }

    public override IDatusAratorObjectStore Put(string bucket, string key, string value, List<ObjectStoreSearchField> searchFields = null) {
      var command = new JsonWrapper();
      command["cmd"] = "put";
      command["bucket"] = bucket;
      command["key"] = key;
      command["value"] = CryptoUtils.GzipBase64String(value);

      if (searchFields != null) {
        foreach (var search in searchFields)
          search.ToJson(command.ExtendArray("search"));
      }

      var parser = SendCommand(command);
      if (parser.Get("code") == "200")
        return this;

      throw new Exception(parser.Get("error"));
    }

    public override void Clear(string bucket) {
      var command = new JsonWrapper();
      command["cmd"] = "clear";
      command["bucket"] = bucket;

      var parser = SendCommand(command);
      if (parser.Get("code") != "200")
        throw new Exception(parser.Get("error"));
    }

    public override bool Delete(string bucket, string key) {
      var command = new JsonWrapper();
      command["cmd"] = "delete";
      command["bucket"] = bucket;
      command["key"] = key;

      var parser = SendCommand(command);
      if (parser.Get("code") != "200")
        throw new Exception(parser.Get("error"));

      return parser.GetAsBool("success");
    }

    public override Dictionary<string, string> Records(string bucket) {
      var command = new JsonWrapper();
      command["cmd"] = "records";
      command["bucket"] = bucket;

      var parser = SendCommand(command);
      if (parser.Get("code") == "200") {
        var result = new Dictionary<string, string>();
        foreach (var item in parser.GetArrayAsList("data")) {
          var temp = CryptoUtils.FromGzipBase64String(item);
          var index = temp.IndexOf("|-|");
          var key = temp.Substring(0, index);
          var value = temp.Substring(index + 3);

          result[key] = value;
        }

        return result;
      }

      throw new Exception(parser.Get("error"));
    }

    public override Dictionary<string, string> Search(string field, string searchValue) {
      var command = new JsonWrapper();
      command["cmd"] = "search";
      command["field"] = field;
      command["value"] = searchValue;

      var parser = SendCommand(command);
      if (parser.Get("code") == "200") {
        var result = new Dictionary<string, string>();
        foreach (var item in parser.GetArrayAsList("data")) {
          var temp = CryptoUtils.FromGzipBase64String(item);
          var index = temp.IndexOf("|-|");
          var key = temp.Substring(0, index);
          var value = temp.Substring(index + 3);

          result[key] = value;
        }

        return result;
      }

      throw new Exception(parser.Get("error"));
    }

    public override void Commit(bool force = false) {
      var command = new JsonWrapper();
      command["cmd"] = "commit";
      command["force"] = force;

      var parser = SendCommand(command);
      if (parser.Get("code") != "200")
        throw new Exception(parser.Get("error"));
    }
  }
}
