using DatusArator.Core.Json;
using DatusArator.Core.Util;
using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Text;

namespace DatusArator.Core.DataAccess {
  public class DatusAratorOdbcConnection : IDisposable {
    public string DataSource { get; set; }

    public string Driver { get; set; }
    public string Server { get; set; }
    public bool Trusted { get; set; }
    public string Database { get; set; }

    public string UserId { get; set; }
    public string Password { get; set; }

    public bool JsonWrapperPreserveCase { get; set; }

    private OdbcConnection fConnection;

    public DatusAratorOdbcConnection() {
      Driver = "Sql Server";
      Server = "localhost";
      Trusted = false;
      Database = "northwind";

      JsonWrapperPreserveCase = false;
    }

    public DatusAratorOdbcConnection(DatusAratorOdbcConnection baseConnection) {
      Driver = baseConnection.Driver;
      Server = baseConnection.Server;
      Trusted = baseConnection.Trusted;
      Database = baseConnection.Database;
    }

    public override string ToString() {
      StringBuilder sb = new StringBuilder();

      sb.Append(Server).Append(" [").Append(Database).Append("] {").Append(Driver).Append("}");

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

    ~DatusAratorOdbcConnection() {
      Dispose(false);
    }
    #endregion

    #region Open/Close
    public void Open() {
      string connectString;
      if (!string.IsNullOrEmpty(DataSource))
        connectString = "DSN=" + DataSource + ";";
      else
        connectString = string.Format("DRIVER={{{0}}};SERVER={1};DATABASE={2};", Driver, Server, Database);

      if (Trusted) {
        connectString += "TRUSTED_CONNECTION=Yes;";
      } else {
        if (!string.IsNullOrEmpty(UserId))
          connectString += "UID=" + UserId;

        if (!string.IsNullOrEmpty(Password))
          connectString += "PWD=" + Password;
      }

      if (fConnection != null)
        fConnection.Close();

      fConnection = new OdbcConnection(connectString);
      fConnection.Open();
    }

    public void Close() {
      if (fConnection != null)
        fConnection.Close();

      fConnection = null;
    }
    #endregion

    #region Execution
    public OdbcDataReader GetDataReader(string sql, bool namedParameters, params object[] paramList) {
      List<object> queryParams = new List<object>();
      for (int i = 0; i < paramList.Length; i++)
        queryParams.Add(paramList[i]);

      return GetDataReader(sql, namedParameters, queryParams);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
    public OdbcDataReader GetDataReader(string sql, bool namedParameters = false, List<object> paramList = null) {
      var command = fConnection.CreateCommand();
      command.CommandText = sql; 

      AddParamsToCommand(command, namedParameters, paramList);

      return command.ExecuteReader();
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

    private void AddParamsToCommand(OdbcCommand command, bool namedParameters, List<object> paramList) {
      if (paramList == null)
        return;

      string currentName = null;
      for (int i = 0; i < paramList.Count; i++) {
        if (namedParameters && (i % 2 == 0)) {
          currentName = paramList[i].ToString();
          continue;
        }

        currentName = namedParameters ? currentName : "@" + i;
        var param = new OdbcParameter() {
          ParameterName = currentName,
          DbType = GetParamType(paramList[i]),
          Direction = System.Data.ParameterDirection.Input,
          Value = GetParamValue(paramList[i])
        };
        command.Parameters.Add(param);
      }
    }

    public System.Data.DbType GetParamType(object p) {
      if (p.GetType() == typeof(TypedBasicObject)) {
        var tbo = (TypedBasicObject)p;
        switch (tbo.TBOType) {
          case TypedBasicObjectType.STRING:
            return System.Data.DbType.String;
          case TypedBasicObjectType.DOUBLE:
            return System.Data.DbType.Double;
          case TypedBasicObjectType.LONG:
            return System.Data.DbType.DateTime;
          case TypedBasicObjectType.INTEGER:
            return System.Data.DbType.Int32;
          case TypedBasicObjectType.DATETIME:
            return System.Data.DbType.DateTime;
          case TypedBasicObjectType.BOOLEAN:
            return System.Data.DbType.Boolean;
        }
      }

      return System.Data.DbType.String;
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

    #endregion

    #region Settings Helpers
    public void SettingsFromJson(JsonWrapper parser, string key) {
      Driver = parser.Get(key + ".driver");
      Server = parser.Get(key + ".server");
      Trusted = parser.GetAsBool(key + ".trusted");
      Database = parser.Get(key + ".database");

      UserId = parser.Get(key + ".user");

      string passwd = parser.Get(key + ".password");
      if (passwd != null)
        passwd = StringUtils.Encodify(passwd);

      Password = passwd;
    }

    public void SettingsToJson(JsonWrapper parser, string key) {
      parser.Put(key + ".driver", Driver);
      parser.Put(key + ".server", Server);
      parser.Put(key + ".trusted", Trusted);
      parser.Put(key + ".database", Database);

      parser.Put(key + ".user", UserId);

      string passwd = Password;
      if (passwd != null)
        passwd = StringUtils.Encodify(passwd);

      parser.Put(key + ".password", passwd);
    }
    #endregion
  }
}
