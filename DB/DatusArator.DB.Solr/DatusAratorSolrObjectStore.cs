using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

using DatusArator.Core;
using DatusArator.Core.Http;
using DatusArator.Core.Json;
using DatusArator.Core.ObjectStore;
using DatusArator.Core.Util;

namespace DatusArator.DB.Solr {
  public class DatusAratorSolrObjectStore : DatusAratorBaseObjectStore {
    public static string nl = System.Environment.NewLine;

    private readonly string fHost;
    private readonly string fCore;

    public override IDatusAratorObjectStore Build(string config) {
      if (config.StartsWith("solr|", System.StringComparison.InvariantCultureIgnoreCase))
        return new DatusAratorSolrObjectStore(config.Substring("solr|".Length));
      else
        return null;
    }

    public override string ToString() {
      return "Solr Object Store : " + fHost + " [" + fCore + "]";
    }

    public DatusAratorSolrObjectStore(string hostName) : base() {
      if (string.IsNullOrEmpty(hostName))
        return;

      var parts = hostName.Split('|');
      fHost = parts[0];
      fCore = parts[1];

      GeneralUtils.OnHeartbeat += HandleHeartbeat;

      AllowSearch = true;
    }

    protected override void Dispose(bool disposing) {
      if (disposing) {
        GeneralUtils.OnHeartbeat -= HandleHeartbeat;
      }

      base.Dispose(disposing);
    }

    #region Utils
    private static string BuildId(string bucket, string key) {
      return bucket + "$" + key;
    }

    private static string CleanBucket(string bucket) {
      return bucket?.Replace(":", "#");
    }
    #endregion

    #region Fetch
    // http://192.168.99.103:8983/solr/test/select?facet.field=bucket&facet.prefix=test%23&facet=on&q=bucket%3Atest%23*&rows=1000
    public override List<string> Buckets(string prefix) {
      prefix = HttpUtility.UrlEncode(CleanBucket(prefix) + "#");
      var result = new List<string>();

      var query = @"facet=on&facet.field=bucket&facet.prefix=" + prefix;
      query += @"&q=bucket%3A" + prefix + "*&rows=1";

      var webResult = QueryServer(query);
      var buckets = webResult.GetArrayAsList("facet_counts.facet_fields.bucket");
      for (int i = 0; i < buckets.Count; i += 2) {
        if (StringUtils.SafeStrToInt(buckets[i + 1], 0) > 0)
          result.Add(buckets[i].Replace("#", ":"));
      }

      return result;
    }

    public override string Get(string bucket, string key) {
      bucket = CleanBucket(bucket);
      var query = @"q=" + HttpUtility.UrlEncode("id:" + BuildId(bucket, key)) + "&fl=_value";
      var webResult = QueryServer(query);
      var value = webResult.Get("response.docs[0]._value");
      return CryptoUtils.FromGzipBase64String(value);
    }

    //http://192.168.99.103:8983/solr/test/select?fl=id&q=bucket%3Atest2%23test&rows=100
    public override List<string> Ids(string bucket) {
      bucket = HttpUtility.UrlEncode(CleanBucket(bucket));

      var result = new List<string>();
      if (string.IsNullOrEmpty(bucket))
        return result;

      var query = @"q=bucket%3A" + bucket + "&fl=key&rows=1000000";
      var webResult = QueryServer(query);
      foreach (var entry in webResult.GetArrayAsObjects("response.docs"))
        result.Add(entry.Get("key"));

      return result;
    }

    public override Dictionary<string, string> Records(string bucket) {
      bucket = HttpUtility.UrlEncode(CleanBucket(bucket));

      var result = new Dictionary<string, string>();
      if (string.IsNullOrEmpty(bucket))
        return result;

      var query = @"q=bucket%3A" + bucket + "&fl=key,_value&rows=1000000";
      var webResult = QueryServer(query);
      foreach (var entry in webResult.GetArrayAsObjects("response.docs")) {
        var value = entry.Get("_value");
        result[entry.Get("key")] = CryptoUtils.FromGzipBase64String(value);
      }

      return result;
    }

    public override Dictionary<string, string> Search(string field, string value) {
      var result = new Dictionary<string, string>();

      string query = field + ":" + value;

      return result;
    }

    public List<string> SearchIds(string field, string value) {
      var result = new List<string>();

      return result;
    }

    public SolrQueryResults Query(string query, string fields, int rows = 10, int start = 0) {
      query = query.Replace(" ", "%20");
      query = query.Replace(":", "%3A");

      var subQuery = "fl=" + fields + "&rows=" + rows;
      if (start > 0)
        subQuery += "&start=" + start;

      var webResult = QueryServer("q=" + query + "&" + subQuery);
      
      return new SolrQueryResults(webResult);
    }
    #endregion

    #region Store
    public int BatchSize { get; set; } = 1000;
    private readonly List<JsonWrapper> fBatch = new List<JsonWrapper>();
    private void PostCurrentBatch() {
      var update = new StringBuilder();
      foreach (var item in fBatch) {
        if (update.Length > 0)
          update.Append(",");
        update.Append(item.ToJsonString());
      }

      PostUpdate("[" + update.ToString() + "]", @"application/json");
      fBatch.Clear();
    }

    protected override void BatchModeChanged() {
      if (!BatchMode)
        PostCurrentBatch();
    }

    public override IDatusAratorObjectStore Put(string bucket, string key, string value, List<ObjectStoreSearchField> searchFields = null) {
      bucket = CleanBucket(bucket);

      var wrapper = new JsonWrapper();
      wrapper.Put("id", BuildId(bucket, key));
      wrapper.Put("bucket", bucket);

      if (value != null) {
        wrapper.Put("key", key);
        wrapper.Put("_value", CryptoUtils.GzipBase64String(value));
      }

      foreach (var sf in searchFields)
        wrapper.Put(sf.Field, sf.Value);

      if (BatchMode) {
        lock (this) {
          fBatch.Add(wrapper);
          if (fBatch.Count >= BatchSize)
            PostCurrentBatch();
        }
      } else {
        PostUpdate("[" + wrapper.ToJsonString() + "]", @"application/json");
        Commit();
      }

      return this;
    }

    public override void Clear(string bucket) {
      bucket = CleanBucket(bucket);

      PostUpdate("<delete><query>bucket:" + bucket + "</query></delete>", "text/xml");
      Commit(true);
    }

    public override bool Delete(string bucket, string key) {
      bucket = CleanBucket(bucket);

      PostUpdate("<delete><id>" + BuildId(bucket, key) + "</id></delete>", "text/xml");
      Commit(true);

      return true;
    }

    private void PostCommit() {
      PostUpdate("<commit/>", "text/xml");
    }

    int fPendingCount = 0;
    public override void Commit(bool force = false) {
      if (force || AutoCommit) {
        PostCommit();
        fPendingCount = 0;
      } else {
        if (fPendingCount++ == 0)
          fFirstCommit = DateTime.Now;
      }
    }

    private System.DateTime fFirstCommit = DateTime.Now;
    private void HandleHeartbeat() {
      if (fPendingCount == 0)
        return;

      var secondsSince = (DateTime.Now - fFirstCommit).TotalSeconds;
      if ((fPendingCount > 500) || (secondsSince > 2)) {
        Commit(true);
      }
    }
    #endregion

    #region Solr Client
    public JsonWrapper QueryServer(string query) {
      var options = new GetUriOptions {
        Uri = new Uri(@"http://" + fHost + @"/solr/" + fCore + @"/select?" + query),
        Method = HttpMethod.Get,
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

    public JsonWrapper PostUpdate(string value, string type = @"application/json") {
      var queryParams = "_=" + TypedBasicObject.NetToJavaTime(DateTime.Now) + @"&commitWithin=1000&overwrite=true&wt=json";

      var options = new GetUriOptions {
        Uri = new Uri(@"http://" + fHost + @"/solr/" + fCore + @"/update?" + queryParams),
        Method = HttpMethod.Post,
        PostContentType = type,
        PostData = Encoding.ASCII.GetBytes(value),
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
    #endregion
  }
}
