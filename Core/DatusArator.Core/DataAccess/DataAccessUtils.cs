using DatusArator.Core.Json;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatusArator.Core.DataAccess {
  public static class DataAccessUtils {
    public static List<JsonWrapper> DbReaderToRows(DbDataReader reader, bool preserverCase = true, Action<List<JsonWrapper>> statusCallback = null) {
      List<JsonWrapper> result = new List<JsonWrapper>();

      if (reader.HasRows) {
        List<string> headers = new List<string>();

        result = new List<JsonWrapper>();
        while (reader.Read()) {
          bool getHeaders = (headers.Count == 0);

          JsonWrapper wrapper = new JsonWrapper() { LoadedFromDatabase = true };

          for (int i = 0; i < reader.FieldCount; i++) {
            if (getHeaders)
              headers.Add(reader.GetName(i));

            string header = headers[i];
            object value = reader[i];

            if ("DECIMAL".Equals(value.GetType().Name.ToUpper()))
              value = Convert.ToDouble(value);
            else if ("BYTE[]".Equals(value.GetType().Name.ToUpper())) {
              var byteValue = (byte[])value;
              Array.Reverse(byteValue);

              if (byteValue.Length == 8)
                value = BitConverter.ToUInt64(byteValue, 0);
              else if (byteValue.Length == 4)
                value = BitConverter.ToUInt32(byteValue, 0);
              else
                value = "0x" + BitConverter.ToString(byteValue).Replace("-", "");
            } else if ("STRING".Equals(value.GetType().Name.ToUpper()))
              value = ((string)value).Trim();

            string headerValue = preserverCase ? header : header.ToLower();

            wrapper.Put(headerValue, value);
          }
          wrapper.ClearChanges();

          result.Add(wrapper);

          if (result.Count % 100 == 0)
            statusCallback?.Invoke(result);
        }
      }

      reader.Close();

      return result;
    }

  }
}
