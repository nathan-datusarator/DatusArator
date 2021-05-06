using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;

using DatusArator.Core;
using DatusArator.Core.DataAccess;
using DatusArator.Core.Json;
using DatusArator.Core.Util;

namespace DatusArator.DB.SqlServer {
  public class SqlServerConnection : IDisposable {
    public string DataSource { get; set; }
    public bool IntegratedSecurity { get; set; }
    public string InitialCatalog { get; set; }
    public string UserId { get; set; }
    public string Password { get; set; }

    public bool Pooling { get; set; }
    public bool PersistSecurityInfo { get; set; }

    public bool JsonWrapperPreserveCase { get; set; }

    private SqlConnection fConnection;

    public SqlServerConnection() {
      DataSource = "localhost";
      InitialCatalog = "LN20";
      IntegratedSecurity = true;

      Pooling = true;
      PersistSecurityInfo = true;

      JsonWrapperPreserveCase = false;
    }

    public SqlServerConnection(SqlServerConnection baseConnection) {
      DataSource = baseConnection.DataSource;
      IntegratedSecurity = baseConnection.IntegratedSecurity;
      InitialCatalog = baseConnection.InitialCatalog;
      UserId = baseConnection.UserId;
      Password = baseConnection.Password;

      Pooling = baseConnection.Pooling;
      PersistSecurityInfo = baseConnection.PersistSecurityInfo;
    }

    public override string ToString() {
      StringBuilder sb = new StringBuilder();
      sb.Append(DataSource).Append(" [").Append(InitialCatalog).Append("] ").Append(UserId);
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

    ~SqlServerConnection() {
      Dispose(false);
    }
    #endregion

    #region Open/Close
    public void Open() {
      string connectString;
      if (IntegratedSecurity)
        connectString = string.Format("Data Source={0};Initial Catalog={1};Integrated Security=True", DataSource, InitialCatalog);
      else {
        connectString = string.Format("Data Source={0};User ID={1};Password={2};Initial Catalog={3}", DataSource, UserId, Password, InitialCatalog);
      }

      if (!PersistSecurityInfo)
        connectString = string.Format("Persist Security Info=False;{0}", connectString);

      if (!Pooling)
        connectString = string.Format("Pooling=false;{0}", connectString);


      if (fConnection != null)
        fConnection.Close();

      fConnection = new SqlConnection(connectString);
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

    private void AddParamsToCommand(SqlCommand command, bool namedParameters, List<object> paramList) {
      if (paramList == null)
        return;

      string currentName = null;
      for (int i = 0; i < paramList.Count; i++) {
        if (namedParameters && (i % 2 == 0)) {
          currentName = paramList[i].ToString();
          continue;
        }

        currentName = namedParameters ? currentName : "@" + i;
        var param = new SqlParameter() {
          ParameterName = currentName,
          SqlDbType = GetParamType(paramList[i]),
          Direction = System.Data.ParameterDirection.Input,
          Value = GetParamValue(paramList[i])
        };
        command.Parameters.Add(param);
      }
    }

    public System.Data.SqlDbType GetParamType(object p) {
      if (p.GetType() == typeof(TypedBasicObject)) {
        var tbo = (TypedBasicObject)p;
        switch (tbo.TBOType) {
          case TypedBasicObjectType.STRING:
            return System.Data.SqlDbType.NVarChar;
          case TypedBasicObjectType.DOUBLE:
            return System.Data.SqlDbType.Float;
          case TypedBasicObjectType.LONG:
            return System.Data.SqlDbType.DateTime;
          case TypedBasicObjectType.INTEGER:
            return System.Data.SqlDbType.Int;
          case TypedBasicObjectType.DATETIME:
            return System.Data.SqlDbType.DateTime;
          case TypedBasicObjectType.BOOLEAN:
            return System.Data.SqlDbType.Bit;
        }
      }

      string typeName = p.GetType().Name.ToUpper();

      if ("STRING".Equals(typeName))
        return System.Data.SqlDbType.NVarChar;
      if ("DECIMAL".Equals(typeName) || "MONEY".Equals(typeName))
        return System.Data.SqlDbType.Decimal;
      if ("DOUBLE".Equals(typeName))
        return System.Data.SqlDbType.Float;
      if ("DATETIME".Equals(typeName))
        return System.Data.SqlDbType.DateTime;
      if ("LONG".Equals(typeName) || "INT64".Equals(typeName))
        return System.Data.SqlDbType.BigInt;
      if ("INT".Equals(typeName) || "INTEGER".Equals(typeName) || "INT32".Equals(typeName) || "INT16".Equals(typeName))
        return System.Data.SqlDbType.Int;
      if ("BYTE".Equals(typeName))
        return System.Data.SqlDbType.Int;
      if ("BOOL".Equals(typeName) || "BOOLEAN".Equals(typeName))
        return System.Data.SqlDbType.Int;
      if ("DBNULL".Equals(typeName))
        return System.Data.SqlDbType.NVarChar;
      if ("GUID".Equals(typeName))
        return System.Data.SqlDbType.NVarChar;

      return System.Data.SqlDbType.NVarChar;
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

    public bool JsonWrapperExists(JsonWrapper wrapper) {
      StringBuilder sql = new StringBuilder();
      sql.Append("SELECT COUNT(1) FROM [").Append(wrapper.TableName).Append("]");

      List<object> paramList = new List<object>();
      bool firstWhereField = true;

      foreach (string keyField in wrapper.KeyFields) {
        string keyValue = wrapper.Get(keyField);

        if (string.IsNullOrEmpty(keyValue))
          return false;

        sql.Append(firstWhereField ? " WHERE [" : " AND [").Append(keyField).Append("] = @").Append(paramList.Count);
        paramList.Add(wrapper.GetOriginalValue(keyField));

        firstWhereField = false;
      }

      return Convert.ToInt32(GetScalarRaw(sql.ToString(), false, paramList)) > 0;
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
          if (item.Key.Contains("["))
            continue;

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
        foreach (var item in wrapper) {
          if (wrapper.KeyFields.Contains(item.Key))
            continue;

          if (item.Key.Contains("["))
            continue;

          if (firstItem)
            firstItem = false;
          else
            sql.Append(",");

          if (item.Value.IsNotNull()) {
            sql.Append(" [").Append(item.Key).Append("] = @").Append(paramList.Count);
            paramList.Add(item.Value);
          } else {
            sql.Append(" [").Append(item.Key).Append("] = NULL");
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
        if (item.Key.Contains("["))
          continue;

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

    public List<string> ListTables() {
      List<string> tables = new List<string>();

      DataTable dt = fConnection.GetSchema("Tables");
      foreach (DataRow row in dt.Rows) {
        string tablename = (string)row[2];
        tables.Add(tablename);
      }

      return tables;
    }
    #endregion

    #region Settings Helpers
    public void SettingsFromJson(JsonWrapper parser, string key) {
      DataSource = parser.Get(key + ".datasource");
      InitialCatalog = parser.Get(key + ".initial");
      IntegratedSecurity = parser.GetAsBool(key + ".security");
      UserId = parser.Get(key + ".user");
      Pooling = parser.GetAsBool(key + ".pooling");
      PersistSecurityInfo = parser.GetAsBool(key + ".persist");

      string passwd = parser.Get(key + ".password");
      if (passwd != null)
        passwd = StringUtils.Encodify(passwd);

      Password = passwd;
    }

    public void SettingsToJson(JsonWrapper parser, string key) {
      parser.Put(key + ".datasource", DataSource);
      parser.Put(key + ".initial", InitialCatalog);
      parser.Put(key + ".security", IntegratedSecurity);
      parser.Put(key + ".user", UserId);
      parser.Put(key + ".pooling", Pooling);
      parser.Put(key + ".persist", PersistSecurityInfo);

      string passwd = Password;
      if (passwd != null)
        passwd = StringUtils.Encodify(passwd);

      parser.Put(key + ".password", passwd);
    }
    #endregion
  }
}
