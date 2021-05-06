using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using FirebirdSql.Data.FirebirdClient;
using System.IO;
using DatusArator.Core;
using System.Data.Common;
using DatusArator.Core.Json;
using DatusArator.Core.Util;
using DatusArator.Core.DataAccess;

namespace DatusArator.DB.Firebird {
  public class FirebirdConnection : IDisposable {
    public string DataSource { get; set; }
    public string Database { get; set; }
    public string UserId { get; set; }
    public string Password { get; set; }
    public bool Embedded { get; set; }

    public bool JsonWrapperPreserveCase { get; set; }

    private FbConnection fConnection;

    public FirebirdConnection() {
      DataSource = "localhost";
      Database = "test.fdb";
      UserId = "SYSDBA";
      Password = "masterkey";
      Embedded = true;

      JsonWrapperPreserveCase = false;
    }

    public FirebirdConnection(FirebirdConnection baseConnection) {
      DataSource = baseConnection.DataSource;
      Database = baseConnection.Database;

      UserId = baseConnection.UserId;
      Password = baseConnection.Password;

      Embedded = baseConnection.Embedded;
    }

    public override string ToString() {
      StringBuilder sb = new StringBuilder();
      sb.Append(DataSource).Append(" [").Append(Database).Append("] ").Append(UserId);
      return sb.ToString();
    }

    #region Dispose
    public void Dispose() {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing) {
      if (disposing)
        if (fConnection != null) {
          fConnection.Dispose();
          fConnection = null;
        }
    }

    ~FirebirdConnection() {
      Dispose(false);
    }
    #endregion

    #region Open/Close
    public string GetConnectionString() {
      var builder = new FbConnectionStringBuilder {
        DataSource = DataSource,
        Database = Database,
        UserID = UserId,
        Password = Password,
        Pooling = false,
        ServerType = Embedded ? FbServerType.Embedded : FbServerType.Default
      };

      return builder.ToString();
    }

    public bool CreateDatabase() {
      try {
        if (!Embedded || !File.Exists(Database)) {
          FbConnection.CreateDatabase(GetConnectionString());
          return true;
        }
      } catch (Exception ex) {
        GeneralUtils.RaiseGlobalError(ex.ToString());
      }

      return false;
    }

    public void Open() {
      if (fConnection != null)
        fConnection.Close();

      fConnection = new FbConnection(GetConnectionString());
      fConnection.Open();
    }

    public void Close() {
      if (fConnection != null)
        fConnection.Close();

      fConnection = null;
    }
    #endregion

    #region Execution
    public DbDataReader GetDataReader(string sql, bool namedParameters, params object[] paramList) {
      List<object> queryParams = new List<object>();
      for (int i = 0; i < paramList.Length; i++)
        queryParams.Add(paramList[i]);

      return GetDataReader(sql, namedParameters, queryParams);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
    public DbDataReader GetDataReader(string sql, bool namedParameters = false, List<object> paramList = null) {
      var command = fConnection.CreateCommand();
      command.CommandText = sql;

      AddParamsToCommand(command, namedParameters, paramList);

      return command.ExecuteReader();
    }

    public DataTable GetDataTable(string sql, bool namedParameters, params object[] paramList) {
      List<object> queryParams = new List<object>();
      for (int i = 0; i < paramList.Length; i++)
        queryParams.Add(paramList[i]);

      return GetDataTable(sql, namedParameters, queryParams);
    }

    public DataTable GetDataTable(string sql, bool namedParameters = false, List<object> paramList = null) {
      var table = new DataTable();
      table.Load(GetDataReader(sql, namedParameters, paramList));

      return table;
    }

    public DataSet GetDataSet(string sql, bool namedParameters, params object[] paramList) {
      List<object> queryParams = new List<object>();
      for (int i = 0; i < paramList.Length; i++)
        queryParams.Add(paramList[i]);

      return GetDataSet(sql, namedParameters, queryParams);
    }

    public DataSet GetDataSet(string sql, bool namedParameters = false, List<object> paramList = null) {
      var result = new DataSet();
      result.Tables.Add(GetDataTable(sql, namedParameters, paramList));
      return result;
    }

    public JsonWrapper GetFirstRow(string sql, bool namedParameters, params object[] paramList) {
      List<object> queryParams = new List<object>();
      for (int i = 0; i < paramList.Length; i++)
        queryParams.Add(paramList[i]);

      return GetFirstRow(sql, namedParameters, queryParams);
    }

    public JsonWrapper GetFirstRow(string sql, bool namedParameters = false, List<object> paramList = null) {
      var rows = GetRows(sql, namedParameters, paramList);

      return ((rows != null) && (rows.Count > 0)) ? rows[0] : null;
    }

    public List<JsonWrapper> GetRows(string sql, bool namedParameters, params object[] paramList) {
      List<object> queryParams = new List<object>();
      for (int i = 0; i < paramList.Length; i++)
        queryParams.Add(paramList[i]);

      return GetRows(sql, namedParameters, queryParams);
    }

    public List<JsonWrapper> GetRows(string sql, bool namedParameters = false, List<object> paramList = null) {
      var reader = GetDataReader(sql, namedParameters, paramList);

      return DataAccessUtils.DbReaderToRows(reader, JsonWrapperPreserveCase);
    }

    public string GetScalar(string sql, bool namedParameters, params object[] paramList) {
      List<object> queryParams = new List<object>();
      for (int i = 0; i < paramList.Length; i++)
        queryParams.Add(paramList[i]);

      return GetScalar(sql, namedParameters, queryParams);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
    public string GetScalar(string sql, bool namedParameters = false, List<object> paramList = null) {
      var command = fConnection.CreateCommand();
      command.CommandText = sql;

      AddParamsToCommand(command, namedParameters, paramList);

      try {
        var result = command.ExecuteScalar();

        return result?.ToString();
      } catch (Exception) {
        return null;
      }
    }

    public object GetScalarRaw(string sql, bool namedParameters, params object[] paramList) {
      List<object> queryParams = new List<object>();
      for (int i = 0; i < paramList.Length; i++)
        queryParams.Add(paramList[i]);

      return GetScalarRaw(sql, namedParameters, queryParams);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
    public object GetScalarRaw(string sql, bool namedParameters = false, List<object> paramList = null) {
      var command = fConnection.CreateCommand();
      command.CommandText = sql;

      AddParamsToCommand(command, namedParameters, paramList);

      try {
        var result = command.ExecuteScalar();
        return result;
      } catch (Exception) {
        return null;
      }
    }

    public int ExecuteNonQuery(string sql, bool namedParameters, params object[] paramList) {
      List<object> queryParams = new List<object>();
      for (int i = 0; i < paramList.Length; i++)
        queryParams.Add(paramList[i]);

      return ExecuteNonQuery(sql, namedParameters, queryParams);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
    public int ExecuteNonQuery(string sql, bool namedParameters = false, List<object> paramList = null) {
      using (var command = fConnection.CreateCommand()) {
        command.CommandText = sql;

        AddParamsToCommand(command, namedParameters, paramList);

        return command.ExecuteNonQuery();
      }
    }

    private void AddParamsToCommand(FbCommand command, bool namedParameters, List<object> paramList) {
      if (paramList == null)
        return;

      string currentName = null;
      for (int i = 0; i < paramList.Count; i++) {
        if (namedParameters && (i % 2 == 0)) {
          currentName = paramList[i].ToString();
          continue;
        }

        currentName = namedParameters ? currentName : "@" + i;
        var param = new FbParameter() {
          ParameterName = currentName,
          FbDbType = GetParamType(paramList[i]),
          Direction = System.Data.ParameterDirection.Input,
          Value = GetParamValue(paramList[i])
        };
        command.Parameters.Add(param);
      }
    }

    public FbDbType GetParamType(object p) {
      if (p.GetType() == typeof(TypedBasicObject)) {
        var tbo = (TypedBasicObject)p;
        switch (tbo.TBOType) {
          case TypedBasicObjectType.STRING:
            return FbDbType.VarChar;
          case TypedBasicObjectType.DOUBLE:
            return FbDbType.Float;
          case TypedBasicObjectType.LONG:
            return FbDbType.TimeStamp;
          case TypedBasicObjectType.INTEGER:
            return FbDbType.Integer;
          case TypedBasicObjectType.DATETIME:
            return FbDbType.TimeStamp;
          case TypedBasicObjectType.BOOLEAN:
            return FbDbType.Binary;
        }
      }

      string typeName = p.GetType().Name.ToUpper();

      if ("STRING".Equals(typeName))
        return FbDbType.VarChar;
      if ("DECIMAL".Equals(typeName) || "MONEY".Equals(typeName))
        return FbDbType.Decimal;
      if ("DOUBLE".Equals(typeName))
        return FbDbType.Float;
      if ("DATETIME".Equals(typeName))
        return FbDbType.TimeStamp;
      if ("LONG".Equals(typeName) || "INT64".Equals(typeName))
        return FbDbType.BigInt;
      if ("INT".Equals(typeName) || "INTEGER".Equals(typeName) || "INT32".Equals(typeName) || "INT16".Equals(typeName))
        return FbDbType.Integer;
      if ("BYTE".Equals(typeName))
        return FbDbType.Integer;
      if ("BOOL".Equals(typeName) || "BOOLEAN".Equals(typeName))
        return FbDbType.Binary;
      if ("DBNULL".Equals(typeName))
        return FbDbType.VarChar;
      if ("GUID".Equals(typeName))
        return FbDbType.VarChar;

      return FbDbType.VarChar;
    }

    public object GetParamValue(object p) {
      if (p.GetType() == typeof(TypedBasicObject)) {
        var tbo = (TypedBasicObject)p;

        if (!tbo.IsNotNull())
          return DBNull.Value;

        switch (tbo.TBOType) {
          case TypedBasicObjectType.LONG:
          case TypedBasicObjectType.DATETIME:
            long value = (long)tbo.GetAsLong();
            if (value <= 0)
              return DateTime.Parse("1/1/1900");

            DateTime date = TypedBasicObject.JavaTimeToNet(value);
            return date.ToUniversalTime();
        }

        return tbo.Value;
      }

      return p.ToString();
    }
    #endregion

    #region JsonWrapper
    // Param List is name/value pairs
    public JsonWrapper GetJsonWrapper(string tableName, params object[] paramList) {
      StringBuilder sql = new StringBuilder();
      sql.Append("SELECT * FROM [").Append(tableName).Append("]");

      var sqlParamList = new List<object>();
      for (int i = 0; i < paramList.Length; i++) {
        if (i % 2 == 0) {
          sql.Append((i == 0) ? " WHERE [" : " AND [").Append(paramList[i].ToString()).Append("]");
        } else {
          sql.Append(" = @").Append(sqlParamList.Count);
          sqlParamList.Add(paramList[i]);
        }
      }

      var data = GetRows(sql.ToString(), false, sqlParamList);

      JsonWrapper result = (data.Count == 1) ? data[0] : new JsonWrapper();

      result.TableName = tableName;
      for (int i = 0; i < paramList.Length; i += 2) {
        result.KeyFields.Add(paramList[i].ToString());
        if (!result.LoadedFromDatabase)
          result.Put(paramList[i].ToString(), paramList[i + 1]);
      }

      return result;
    }

    public int SaveJsonWrapper(JsonWrapper wrapper, bool insertOnFail = false) {
      StringBuilder sql = new StringBuilder();
      List<object> paramList = new List<object>();

      sql.Append("UPDATE ");
      sql.Append("[").Append(wrapper.TableName).Append("]");
      sql.Append(" SET ");

      bool firstItem = true;

      if (wrapper.LoadedFromDatabase && wrapper.TrackChanges) {
        var changes = wrapper.GetChanges();
        if (changes.Count == 0)
          return 0;
        foreach (var item in changes) {
          if (firstItem)
            firstItem = false;
          else
            sql.Append(",");

          if ((item.Value.NewValue != null) && (item.Value.NewValue.IsNotNull())) {
            sql.Append(" [").Append(item.Key).Append("] = @").Append(paramList.Count);
            paramList.Add(item.Value.NewValue);
          } else {
            sql.Append(" [").Append(item.Key).Append("] = NULL");
          }
        }
      } else {
        foreach (var key in wrapper.GetFields()) {
          if (wrapper.KeyFields.Contains(key))
            continue;

          if (firstItem)
            firstItem = false;
          else
            sql.Append(",");

          var value = wrapper.GetRaw(key);

          if (value.IsNotNull()) {
            sql.Append(" [").Append(key).Append("] = @").Append(paramList.Count);
            paramList.Add(value);
          } else {
            sql.Append(" [").Append(key).Append("] = NULL");
          }
        }
      }

      bool firstWhereField = true;
      foreach (string keyField in wrapper.KeyFields) {
        string keyValue = wrapper.Get(keyField);

        if (string.IsNullOrEmpty(keyValue))
          if (insertOnFail)
            return SaveJsonWrapperAsNew(wrapper);
          else
            throw new Exception("No Value in Key Field: " + keyField);

        sql.Append(firstWhereField ? " WHERE [" : " AND [").Append(keyField).Append("] = @").Append(paramList.Count);
        paramList.Add(wrapper.GetOriginalValue(keyField));

        firstWhereField = false;
      }
      int rowsAffected = ExecuteNonQuery(sql.ToString(), false, paramList);

      if ((rowsAffected == 0) && (insertOnFail))
        return SaveJsonWrapperAsNew(wrapper);

      return rowsAffected;
    }

    public bool DeleteJsonWrapper(JsonWrapper wrapper) {
      StringBuilder sql = new StringBuilder();
      List<object> paramList = new List<object>();

      sql.Append("DELETE FROM ");
      sql.Append("[").Append(wrapper.TableName).Append("]");

      bool firstWhereField = true;
      foreach (string keyField in wrapper.KeyFields) {
        string keyValue = wrapper.Get(keyField);

        if (string.IsNullOrEmpty(keyValue))
          throw new Exception("No Value in Key Field: " + keyField);

        sql.Append(firstWhereField ? " WHERE [" : " AND [").Append(keyField).Append("] = @").Append(paramList.Count);
        paramList.Add(wrapper.GetOriginalValue(keyField));
        firstWhereField = false;
      }

      int rowsAffected = ExecuteNonQuery(sql.ToString(), false, paramList);

      return rowsAffected > 0;
    }

    public int SaveJsonWrapperAsNew(JsonWrapper wrapper) {
      StringBuilder sql = new StringBuilder();
      List<object> paramList = new List<object>();

      sql.Append("INSERT INTO ");
      sql.Append("[").Append(wrapper.TableName).Append("]");
      sql.Append(" (");

      bool firstItem = true;
      foreach (var item in wrapper) {
        if (item.Value.IsNotNull()) {
          if (firstItem)
            firstItem = false;
          else
            sql.Append(",");

          sql.Append(" [").Append(item.Key).Append("]");

          paramList.Add(item.Value);
        }
      }

      sql.Append(") VALUES (");

      for (int i = 0; i < paramList.Count; i++) {
        if (i > 0)
          sql.Append(',');
        sql.Append("@" + i);
      }

      sql.Append(")");

      return ExecuteNonQuery(sql.ToString(), false, paramList);
    }

    public List<string> ListTables(bool includeSystem = false) {
      List<string> tables = new List<string>();
      
      DataTable dt = fConnection.GetSchema("Tables");
      foreach (DataRow row in dt.Rows) {
        string tablename = (string)row[2];

        if (!tablename.Contains("$") || includeSystem)
          tables.Add(tablename);
      }
      
      return tables;
    }
    #endregion

    #region Settings Helpers
    public void SettingsFromJson(JsonWrapper parser, string key) {
      DataSource = parser.Get(key + ".datasource");
      Database = parser.Get(key + ".db");
      UserId = parser.Get(key + ".user");

      string passwd = parser.Get(key + ".password");
      if (passwd != null)
        passwd = StringUtils.Encodify(passwd);

      Password = passwd;

      Embedded = parser.GetAsBool(key + ".embedded", true);
    }

    public void SettingsToJson(JsonWrapper parser, string key) {
      parser.Put(key + ".datasource", DataSource);
      parser.Put(key + ".db", Database);
      parser.Put(key + ".user", UserId);

      string passwd = Password;
      if (passwd != null)
        passwd = StringUtils.Encodify(passwd);

      parser.Put(key + ".password", passwd);

      parser.Put(key + ".embedded", Embedded);
    }
    #endregion
  }
}
