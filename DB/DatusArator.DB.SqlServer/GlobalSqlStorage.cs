using DatusArator.Core.Json;
using System.Collections.Generic;

namespace DatusArator.DB.SqlServer {
  public class GlobalSqlStorage {
    private static string DataSource { get; set; }
    private static string Database { get; set; }
    public static string UserName { get; private set; }
    public static string Password { get; private set; }

    private static SqlServerConnection fConnection = null;
    private static int fOperations = 0;

    public static SqlServerConnection GetConnection() {
      if (fOperations++ > 500) {
        CloseConnection();
        fOperations = 0;
      }

      if (fConnection == null) {
        fConnection = new SqlServerConnection();
        fConnection.DataSource = DataSource;
        fConnection.InitialCatalog = Database;
        fConnection.IntegratedSecurity = false;
        fConnection.UserId = UserName;
        fConnection.Password = Password;

        fConnection.Open();
      }

      return fConnection;
    }

    public static void CloseConnection() {
      if (fConnection != null) {
        fConnection.Close();
        fConnection.Dispose();

        fConnection = null;
      }
    }

    public static void Init(string dataSource, string dbName, string userName, string password) {
      DataSource = dataSource;
      Database = dbName;
      UserName = userName;
      Password = password;
    }

    public static T Get<T>(string tableName, params object[] paramList) {
      var connection = GetConnection();
      var wrapper = connection.GetJsonWrapper(tableName, paramList);
      if (wrapper.LoadedFromDatabase)
        return JsonUtils.JsonToObject<T>(wrapper.ToJsonString());
      else
        return default(T);
    }

    internal static List<T> GetAll<T>(string tableName) {
      var result = new List<T>();

      var connection = GetConnection();
      var wrappers = connection.GetRows("SELECT * FROM " + tableName);
      foreach (var wrapper in wrappers)
        result.Add(JsonUtils.JsonToObject<T>(wrapper.ToJsonString()));

      return result;
    }

    public static void SaveAsNew(object item, string tableName, params string[] keyFields) {
      var wrapper = new JsonWrapper(JsonUtils.ObjectToJson(item));
      wrapper.TableName = tableName;
      wrapper.KeyFields.AddRange(keyFields);

      var connection = GetConnection();
      connection.SaveJsonWrapperAsNew(wrapper);
    }

    public static void Save(object item, string tableName, params string[] keyFields) {
      var wrapper = new JsonWrapper(JsonUtils.ObjectToJson(item));
      wrapper.TableName = tableName;
      wrapper.KeyFields.AddRange(keyFields);

      var connection = GetConnection();
      if (connection.JsonWrapperExists(wrapper))
        connection.SaveJsonWrapper(wrapper);
      else
        connection.SaveJsonWrapperAsNew(wrapper);
    }
  }
}
