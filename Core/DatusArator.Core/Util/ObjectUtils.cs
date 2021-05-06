using System;
using System.IO;
using System.Xml.Serialization;

namespace DatusArator.Core.Util {
  public static class ObjectUtils {
    public static object Clone(object source) {
      if (source == null)
        return null;

      XmlSerializer xs = new XmlSerializer(source.GetType());

      using (MemoryStream s = new MemoryStream()) {
        xs.Serialize(s, source);
        s.Seek(0, SeekOrigin.Begin);

        var result = xs.Deserialize(s);
        return result;
      }
    }
  }
}
