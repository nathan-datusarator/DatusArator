using System;
using System.Collections.Generic;

using DatusArator.Core.Json;

namespace DatusArator.Excel {
  public static class JsonToExcel {
    public static void ToExcel(MappingDef _, List<JsonWrapper> _1, string _2) {
    }

    public class MappingDef {
      public string Name { get; set; }
      public List<MappingFieldDef> Fields = new List<MappingFieldDef>();

    }

    public class MappingFieldDef {
      public string Header { get; set; }
      public string Field { get; set; }

      public MappingFieldDef(String header, String field) {
        this.Header = header;
        this.Field = field;
      }
    }
  }
}
