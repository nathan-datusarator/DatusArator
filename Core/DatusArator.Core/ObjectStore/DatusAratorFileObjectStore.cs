using DatusArator.Core.Logging;
using DatusArator.Core.Util;
using System;
using System.Collections.Generic;
using System.IO;

namespace DatusArator.Core.ObjectStore {
  public class DatusAratorFileObjectStore : DatusAratorBaseObjectStore {
    public override IDatusAratorObjectStore Build(string config) {
      if (config.StartsWith("File|", StringComparison.InvariantCultureIgnoreCase))
        return new DatusAratorFileObjectStore(config.Substring(5));
      else
        return null;
    }

    public override string ToString() {
      return "File Object Store: " + fRoot;
    }

    private readonly string fRoot;
    public DatusAratorFileObjectStore(string rootDirectory) {
      this.fRoot = FileUtils.AddSlash(rootDirectory);

      if (!string.IsNullOrEmpty(this.fRoot))
        Directory.CreateDirectory(this.fRoot);
    }

    public override List<string> Buckets(string prefix) {
      var result = new List<string>();

      if (prefix.EndsWith(":"))
        prefix = prefix.Substring(0, prefix.Length - 1);

      string path = BuildPath(prefix);
      string[] dirs;
      try {
        dirs = Directory.GetDirectories(path);
      } catch (Exception) {
        DatusAratorLogger.Log(this, DatusAratorLogLevel.ERROR, "Invalid Root Bucket - " + prefix);
        return result;
      }

      foreach (var dir in dirs) {
        string bucket = StringUtils.Concat(prefix, dir.Substring(path.Length), ":");

        string[] files = Directory.GetFiles(dir);
        if (files.Length > 0)
          result.Add(bucket);

        result.AddRange(Buckets(bucket));
      }

      return result;
    }

    public override List<string> Ids(string bucket) {
      string path = BuildPath(bucket);
      if (!Directory.Exists(path))
        return new List<string>();

      List<string> result = new List<string>(Directory.GetFiles(path));

      for (int i = 0; i < result.Count; i++) {
        result[i] = result[i].Substring(path.Length);
      }
      return result;
    }

    public override Dictionary<string, string> Records(string bucket) {
      var result = new Dictionary<string, string>();
      foreach (var id in Ids(bucket)) {
        result[id] = Get(bucket, id);
      }

      return result;
    }

    public override string Get(string bucket, string key) {
      string fileName = GetFileName(bucket, key);

      lock (this) {
        return FileUtils.FileToString(fileName);
      }
    }

    public override byte[] GetBin(string bucket, string key) {
      string fileName = GetFileName(bucket, key);

      lock (this) {
        if (File.Exists(fileName))
          return File.ReadAllBytes(fileName);
        else
          return null;
      }
    }

    public override IDatusAratorObjectStore Put(string bucket, string key, string value, List<ObjectStoreSearchField> searchFields = null) {
      string fileName = GetFileName(bucket, key);

      lock (this) {
        FileUtils.StringToFile(value, fileName);
      }

      return this;
    }

    public override IDatusAratorObjectStore PutBin(string bucket, string key, byte[] bytes, List<ObjectStoreSearchField> searchFields = null) {
      string fileName = GetFileName(bucket, key);

      lock (this) {
        File.WriteAllBytes(fileName, bytes);
      }

      return this;
    }

    public override bool Delete(string bucket, string key) {
      string fileName = GetFileName(bucket, key);
      if (File.Exists(fileName)) {
        lock (this) {
          File.Delete(fileName);
        }

        return true;
      }

      return false;
    }

    public override void Clear(string bucket) {
      var path = BuildPath(bucket);
      var ids = Ids(bucket);

      lock (this) {
        foreach (var id in ids)
          File.Delete(path + id);
      }
    }

    private string BuildPath(string bucket) {
      if (bucket != null)
        bucket = bucket.Replace(":", @"\");

      return FileUtils.AddSlash(StringUtils.Concat(fRoot, bucket, ""));
    }

    private string GetFileName(string bucket, string key) {
      string result = BuildPath(bucket) + key;
      Directory.CreateDirectory(Path.GetDirectoryName(result));

      return result;
    }
  }
}
