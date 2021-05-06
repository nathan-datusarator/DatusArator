using DatusArator.Core.ObjectStore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

// Create Table DA_DATA (RECORD_KEY VARCHAR(128), BUCKET VARCHAR(128), ID VARCHAR(128), TS DateTime, DATA TEXT, PRIMARY KEY (RECORD_KEY));
// Create Index AK_DA_DATA_BUCKET ON DA_DATA(BUCKET, ID);

namespace DatusArator.DB.Firebird {
  public class FirebirdObjectStore : DatusAratorBaseObjectStore {
    public override IDatusAratorObjectStore Build(string config) {
      if (config.StartsWith("Firebird|", StringComparison.InvariantCultureIgnoreCase))
        return null; // new FirebirdObjectStore(config.Substring(4));
      else
        return null;
    }

    private readonly FirebirdConnection fConnection;

    public string TableName { get; set; } = "DA_DATA";

    public FirebirdObjectStore(FirebirdConnection connection) {
      this.fConnection = new FirebirdConnection(connection);

      this.fConnection.Open();

      CheckTables();
    }

    #region Init and Dispose
    private void CheckTables() {
      var tables = fConnection.ListTables();
      if (!tables.Contains(TableName, StringComparer.InvariantCultureIgnoreCase)) {
        fConnection.ExecuteNonQuery(@"Create Table " + TableName + " (RECORD_KEY VARCHAR(256), BUCKET VARCHAR(128), ID VARCHAR(128), TS TimeStamp, DATA BLOB SUB_TYPE TEXT, PRIMARY KEY (RECORD_KEY));");
        fConnection.ExecuteNonQuery(@"Create Index AK_" + TableName + "_BUCKET ON " + TableName + "(BUCKET, ID);");
      }
    }

    protected override void Dispose(bool disposing) {
      if (disposing) {
        fConnection.Close();
        fConnection.Dispose();
      }
    }
    #endregion

    public override List<string> Buckets(string prefix) {
      var sql = "SELECT DISTINCT BUCKET FROM " + TableName;
      if (!string.IsNullOrEmpty(prefix))
        sql += " WHERE BUCKET LIKE '" + prefix + "%'";

      List<string> results = new List<string>();

      using (var buckets = fConnection.GetDataTable(sql)) {
        foreach (DataRow row in buckets.Rows) {
          results.Add(row[0].ToString());
        }

        return results;
      }
    }

    public override List<string> Ids(string bucket) {
      var sql = "SELECT ID FROM " + TableName + " WHERE BUCKET = @Bucket";

      List<string> results = new List<string>();

      using (var ids = fConnection.GetDataTable(sql, true, new object[] { "Bucket", bucket })) {
        foreach (DataRow row in ids.Rows) {
          results.Add(row[0].ToString());
        }

        return results;
      }
    }

    public override Dictionary<string, string> Records(string bucket) {
      var sql = "SELECT ID, DATA FROM " + TableName + " WHERE BUCKET = @0";

      Dictionary<string, string> results = new Dictionary<string, string>();

      using (var ids = fConnection.GetDataTable(sql, false, new object[] { bucket })) {
        foreach (DataRow row in ids.Rows) {
          results[row[0].ToString()] = row[1].ToString();
        }

        return results;
      }
    }

    public override string Get(string bucket, string key) {
      var sql = "SELECT DATA FROM " + TableName + " WHERE BUCKET = @0 AND ID = @1";
      return fConnection.GetScalar(sql, false, new object[] { bucket, key });
    }

    public override IDatusAratorObjectStore Put(string bucket, string key, string value, List<ObjectStoreSearchField> searchFields) {
      var sql = "UPDATE OR INSERT INTO " + TableName + "(RECORD_KEY, BUCKET, ID, TS, DATA) VALUES (@0, @1, @2, @3, @4)";
      sql += " MATCHING (RECORD_KEY)";
      var data = new object[] { bucket + "~" + key, bucket, key, DateTime.Now, value };

      fConnection.ExecuteNonQuery(sql, false, data);

      return this;
    }

    public override void Clear(string bucket) {
      var sql = "DELETE FROM " + TableName;
      object[] data = null;
      if (bucket != null) {
        sql += " WHERE BUCKET = @0";
        data = new object[] { bucket };
      }

      fConnection.ExecuteNonQuery(sql, false, data);
    }

    public override bool Delete(string bucket, string key) {
      var sql = "DELETE FROM " + TableName + " WHERE BUCKET = @0 AND ID = @1";
      fConnection.ExecuteNonQuery(sql, false, new object[] { bucket, key });

      return true;
    }
  }
}
