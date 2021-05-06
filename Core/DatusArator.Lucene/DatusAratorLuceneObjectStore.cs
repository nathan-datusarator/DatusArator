using DatusArator.Core.ObjectStore;
using DatusArator.Core.Util;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using static Lucene.Net.Index.IndexWriter;

namespace DatusArator.DB.Lucene.LuceneSearch {
  public class DatusAratorLuceneObjectStore : DatusAratorBaseObjectStore {
    public static string nl = System.Environment.NewLine;

    private readonly object WRITER_LOCK = new object();

    public override IDatusAratorObjectStore Build(string config) {
      if (config.StartsWith("Lucene|", System.StringComparison.InvariantCultureIgnoreCase))
        return new DatusAratorLuceneObjectStore(config.Substring("Lucene|".Length));
      else
        return null;
    }

    public override string ToString() {
      return "Lucene Object Store : " + fRoot;
    }

    private readonly string fRoot;
    private Directory fIndexDirectory;

    private IndexWriter fWriter;
    private IndexSearcher fSearcher;

    private readonly Analyzer fWhitespaceAnalyzer = new WhitespaceAnalyzer();

    private IndexWriter Writer {
      get {
        if (fDisposed)
          return null;

        IndexWriter result;
        lock (WRITER_LOCK) {
          if (fWriter == null)
            fWriter = new IndexWriter(fIndexDirectory, fWhitespaceAnalyzer, false, MaxFieldLength.UNLIMITED);

          result = fWriter;
        }

        return result;
      }
    }

    private IndexSearcher Searcher {
      get {
        if (fDisposed)
          return null;

        IndexSearcher result;
        lock (WRITER_LOCK) {
          if (fSearcher == null)
            fSearcher = new IndexSearcher(fIndexDirectory);

          result = fSearcher;
        }

        return result;
      }
    }

    public DatusAratorLuceneObjectStore(string indexDirectory) : base() {
      if (string.IsNullOrEmpty(indexDirectory))
        return;

      fRoot = indexDirectory;
      fIndexDirectory = FSDirectory.Open(indexDirectory);
      bool create = !System.IO.Directory.Exists(indexDirectory) || (System.IO.Directory.GetFiles(indexDirectory)?.Length == 0);

      // Initialize the Writer to create the index (or test it) then clear it so that its only created when needed
      var createWriter = new IndexWriter(fIndexDirectory, fWhitespaceAnalyzer, create, MaxFieldLength.UNLIMITED);
      createWriter.Commit();
      createWriter.Dispose();

      fWriter = null;
      fSearcher = null;

      GeneralUtils.OnHeartbeat += HandleHeartbeat;

      AllowSearch = true;
    }

    private bool fDisposed = false;
    protected override void Dispose(bool disposing) {
      if (disposing) {
        fDisposed = true;

        GeneralUtils.OnHeartbeat -= HandleHeartbeat;

        lock (WRITER_LOCK) {
          var oldWriter = fWriter;
          fWriter = null;

          oldWriter?.Commit();
          oldWriter?.Dispose();

          if (fSearcher != null) 
            fSearcher.Dispose();
          fSearcher = null;
        }

        fIndexDirectory.Dispose();
        fIndexDirectory = null;

        fWhitespaceAnalyzer.Dispose();
      }
    }

    public static void FixIndex(string dir, bool writeResults = true) {
      var results = GeneralUtils.AddTimestamp("*** Start Index Check ***") + nl + nl;

      var indexDir = FSDirectory.Open(dir);
      var checker = new CheckIndex(indexDir);

      var errors = checker.CheckIndex_Renamed_Method();
      results += GeneralUtils.AddTimestamp("Check Finished") + nl;
      if (errors.clean)
        results += GeneralUtils.AddTimestamp("  No Errors Found");
      else {
        results += GeneralUtils.AddTimestamp("  " + errors.numSegments + " Total Segements") + nl;
        results += GeneralUtils.AddTimestamp("  " + errors.numBadSegments + " Bad Segments found") + nl;
        results += GeneralUtils.AddTimestamp("  " + errors.totLoseDocCount + " Documents will be lost") + nl;

        results += nl;

        results += GeneralUtils.AddTimestamp("Repair Started") + nl;
        checker.FixIndex(errors);
        results += GeneralUtils.AddTimestamp("Repair Finished") + nl;
      }

      if (writeResults)
        FileUtils.StringToFile(results, FileUtils.AddSlash(dir) + "__CheckIndex_" + System.DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt");
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
    public override List<string> Buckets(string prefix) {
      prefix = CleanBucket(prefix);

      TermEnum terms;
      using (var reader = IndexReader.Open(fIndexDirectory, true)) {
        terms = reader.Terms(new Term("bucket"));

        if (prefix == "")
          prefix = null;
        else if (!prefix.EndsWith("#"))
          prefix += "#";

        var result = new List<string>();
        do {
          var currentTerm = terms.Term;
          if (currentTerm?.Field != "bucket")
            break;

          var value = currentTerm.Text;
          if ((prefix == null) || value.StartsWith(prefix))
            result.Add(value.Replace("#", ":"));
        } while (terms.Next());

        return result;
      }
    }

    public override string Get(string bucket, string key) {
      bucket = CleanBucket(bucket);

      var parser = new QueryParser(Version.LUCENE_30, "id", fWhitespaceAnalyzer);

      var query = parser.Parse(BuildId(bucket, key));
      var found = Searcher.Search(query, 1);
      if ((found?.TotalHits ?? 0) > 0) {
        var doc = Searcher.Doc(found.ScoreDocs[0].Doc);
        return doc.Get("_value");
      }

      return null;
    }

    public override List<string> Ids(string bucket) {
      var result = new List<string>();

      bucket = CleanBucket(bucket);
      if (string.IsNullOrEmpty(bucket))
        return result;

      var parser = new QueryParser(Version.LUCENE_30, "bucket", fWhitespaceAnalyzer);

      var query = parser.Parse(bucket);
      try {
        var found = Searcher.Search(query, 1000000);
        var fieldSelector = new IdFieldSelector();

        for (int i = 0; i < found.TotalHits; i++) {
          var id = Searcher.Doc(found.ScoreDocs[i].Doc, fieldSelector).Get("id");

          result.Add(id.Substring(bucket.Length + 1));

          if (i % 1000 == 0)
            Application.DoEvents();
        }
      } catch (System.Exception ex) {
        GeneralUtils.OnGlobalError("Error in Lucene Index [" + bucket + "] " + ex.Message);
      }

      return result;
    }

    public override Dictionary<string, string> Records(string bucket) {
      bucket = CleanBucket(bucket);

      var parser = new QueryParser(Version.LUCENE_30, "bucket", fWhitespaceAnalyzer);

      var query = parser.Parse(bucket);
      var found = Searcher.Search(query, 1000000);
      var result = new Dictionary<string, string>();

      var fieldSelector = new IdValueFieldSelector();
      for (int i = 0; i < found.TotalHits; i++) {
        try {
          var doc = Searcher.Doc(found.ScoreDocs[i].Doc, fieldSelector);
          result[doc.Get("id")] = doc.Get("_value");
        } catch (System.Exception ex) {
          if (Debugger.IsAttached) {
            System.Console.WriteLine("Error in Records: " + ex.Message);
            System.Console.WriteLine("  [I] " + i + "  [Total] " + found.TotalHits);
          }
        }

        if (i % 1000 == 0)
          Application.DoEvents();
      }

      return result;
    }

    public override Dictionary<string, string> Search(string field, string value) {
      var parser = new QueryParser(Version.LUCENE_30, field, fWhitespaceAnalyzer);

      var query = parser.Parse(value);
      var found = Searcher.Search(query, 1000000);

      var result = new Dictionary<string, string>();

      var fieldSelector = new IdValueFieldSelector();
      for (int i = 0; i < found.TotalHits; i++) {
        try {
          var doc = Searcher.Doc(found.ScoreDocs[i].Doc, fieldSelector);
          result[doc.Get("id")] = doc.Get("_value");
        } catch (System.Exception ex) {
          if (Debugger.IsAttached) {
            System.Console.WriteLine("Error in Records: " + ex.Message);
            System.Console.WriteLine("  [I] " + i + "  [Total] " + found.TotalHits);
          }
        }

        if (i % 1000 == 0) {
          Application.DoEvents();
          GeneralUtils.GlobalProgress?.UpdateProgress(null, i, found.TotalHits);
        }
      }

      return result;
    }

    public List<string> SearchIds(string field, string value) {
      var parser = new QueryParser(Version.LUCENE_30, field, fWhitespaceAnalyzer);

      var query = parser.Parse(value);
      var found = Searcher.Search(query, 1000000);

      var result = new List<string>();

      var fieldSelector = new IdFieldSelector();
      for (int i = 0; i < found.TotalHits; i++) {
        try {
          var doc = Searcher.Doc(found.ScoreDocs[i].Doc, fieldSelector);
          result.Add(doc.Get("id"));
        } catch (System.Exception ex) {
          if (Debugger.IsAttached) {
            System.Console.WriteLine("Error in Records: " + ex.Message);
            System.Console.WriteLine("  [I] " + i + "  [Total] " + found.TotalHits);
          }
        }

        if (i % 1000 == 0)
          Application.DoEvents();
      }

      return result;
    }
    #endregion

    #region Store
    public override IDatusAratorObjectStore Put(string bucket, string key, string value, List<ObjectStoreSearchField> searchFields = null) {
      bucket = CleanBucket(bucket);

      var doc = new Document();
      doc.Add(new Field("bucket", bucket, Field.Store.YES, Field.Index.NOT_ANALYZED));
      doc.Add(new Field("key", key, Field.Store.YES, Field.Index.NOT_ANALYZED));
      doc.Add(new Field("id", BuildId(bucket, key), Field.Store.YES, Field.Index.NOT_ANALYZED));
      doc.Add(new Field("_value", value, Field.Store.YES, Field.Index.NO));

      if (searchFields != null) {
        foreach (var searchField in searchFields) {
          var field = new Field(searchField.Field, searchField.Value,
            searchField.Stored ? Field.Store.YES : Field.Store.NO,
            searchField.Analyzed ? Field.Index.ANALYZED : Field.Index.NOT_ANALYZED);
          doc.Add(field);
        }
      }

      lock (WRITER_LOCK) {
        Writer.UpdateDocument(new Term("id", BuildId(bucket, key)), doc);
        Commit();
      }

      return this;
    }

    public override void Clear(string bucket) {
      bucket = CleanBucket(bucket);

      var parser = new QueryParser(Version.LUCENE_30, "bucket", fWhitespaceAnalyzer);
      var query = parser.Parse(bucket);

      lock (WRITER_LOCK) {
        Writer.DeleteDocuments(query);
        Commit();
      }
    }

    public override bool Delete(string bucket, string key) {
      bucket = CleanBucket(bucket);

      var parser = new QueryParser(Version.LUCENE_30, "id", fWhitespaceAnalyzer);
      var query = parser.Parse(BuildId(bucket, key));

      lock (WRITER_LOCK) {
        Writer.DeleteDocuments(query);
        Commit();
      }

      return true;
    }

    private int fPendingCount = 0;
    public override void Commit(bool force = false) {
      if (force || AutoCommit) {
        lock (WRITER_LOCK) {
          fWriter?.Commit();
        }

        fSearcher = null;

        fPendingCount = 0;
        fLastCommit = System.DateTime.Now;
      } else {
        if (fPendingCount++ == 0)
          fLastCommit = System.DateTime.Now;
      }
    }

    private System.DateTime fLastCommit = System.DateTime.Now;
    private void HandleHeartbeat() {
      if (fPendingCount == 0)
        return;

      var secondsSince = (System.DateTime.Now - fLastCommit).TotalSeconds;
      if ((fPendingCount > 500) || (secondsSince > 2)) {
        lock (WRITER_LOCK) {
          Commit(true);

          var oldWriter = fWriter;
          fWriter = null;

          oldWriter?.Dispose();
        }
      }
    }
    #endregion
  }

  class IdFieldSelector : FieldSelector {
    public FieldSelectorResult Accept(string fieldName) {
      if (fieldName == "id")
        return FieldSelectorResult.LOAD_AND_BREAK;
      return FieldSelectorResult.NO_LOAD;
    }
  }

  class IdValueFieldSelector : FieldSelector {
    public FieldSelectorResult Accept(string fieldName) {
      if ((fieldName == "id") || (fieldName == "_value"))
        return FieldSelectorResult.LOAD;
      return FieldSelectorResult.NO_LOAD;
    }
  }
}
