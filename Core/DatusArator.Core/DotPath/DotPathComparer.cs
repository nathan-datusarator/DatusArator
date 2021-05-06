using System;
using System.Collections.Generic;

namespace DatusArator.Core.DotPath {
  public sealed class DotPathComparer : IComparer<DotPathObject> {
    public string Field { get; set; }
    public string SubType { get; set; }

    public DotPathComparer(string field) {
      Field = field;
    }

    int IComparer<DotPathObject>.Compare(DotPathObject x, DotPathObject y) {
      var val1 = x[Field]?.ToString();
      var val2 = y[Field]?.ToString();

      if (val1 == null)
        return (val2 == null) ? 0 : 1;

      if (SubType == "INT")
        return Int32.Parse(val1).CompareTo(Int32.Parse(val2));
      if (SubType == "DECIMAL")
        return Decimal.Parse(val1).CompareTo(Decimal.Parse(val2));

      return val1.CompareTo(val2);
    }

    public static void Sort(List<DotPathObject> list, string field, TypedBasicObjectType fieldType, bool sortDesc) {
      switch (fieldType) {
        case TypedBasicObjectType.DECIMAL:
          list.Sort(new DotPathDecimalComparer(field));
          if (!sortDesc)
            list.Reverse();
          break;
        case TypedBasicObjectType.DATETIME:
          list.Sort(new DotPathDateTimeComparer(field));
          if (!sortDesc)
            list.Reverse();
          break;
        case TypedBasicObjectType.LONG:
          list.Sort(new DotPathLongComparer(field));
          if (!sortDesc)
            list.Reverse();
          break;
        case TypedBasicObjectType.INTEGER:
          list.Sort(new DotPathIntComparer(field));
          if (!sortDesc)
            list.Reverse();
          break;
        case TypedBasicObjectType.DOUBLE:
          list.Sort(new DotPathDoubleComparer(field));
          if (!sortDesc)
            list.Reverse();
          break;
        default:
          list.Sort(new DotPathComparer(field));
          if (sortDesc)
            list.Reverse();
          break;
      }
    }
  }

  public sealed class DotPathDateTimeComparer : IComparer<DotPathObject> {
    public string Field { get; set; }

    public DotPathDateTimeComparer(string field) {
      Field = field;
    }

    int IComparer<DotPathObject>.Compare(DotPathObject x, DotPathObject y) {
      var val1 = x[Field] as DateTime?;
      var val2 = y[Field] as DateTime?;

      if (val1 == null)
        return (val2 == null) ? 0 : 1;

      return val1.Value.CompareTo(val2.Value);
    }
  }

  public sealed class DotPathIntComparer : IComparer<DotPathObject> {
    public string Field { get; set; }

    public DotPathIntComparer(string field) {
      Field = field;
    }

    int IComparer<DotPathObject>.Compare(DotPathObject x, DotPathObject y) {
      var val1 = x[Field] as int?;
      var val2 = y[Field] as int?;

      if (val1 == null)
        return (val2 == null) ? 0 : 1;

      return val1.Value.CompareTo(val2.Value);
    }
  }

  public sealed class DotPathLongComparer : IComparer<DotPathObject> {
    public string Field { get; set; }

    public DotPathLongComparer(string field) {
      Field = field;
    }

    int IComparer<DotPathObject>.Compare(DotPathObject x, DotPathObject y) {
      var val1 = x[Field] as long?;
      var val2 = y[Field] as long?;

      if (val1 == null)
        return (val2 == null) ? 0 : 1;

      return val1.Value.CompareTo(val2.Value);
    }
  }

  public sealed class DotPathDoubleComparer : IComparer<DotPathObject> {
    public string Field { get; set; }

    public DotPathDoubleComparer(string field) {
      Field = field;
    }

    int IComparer<DotPathObject>.Compare(DotPathObject x, DotPathObject y) {
      var val1 = x[Field] as double?;
      var val2 = y[Field] as double?;

      if (val1 == null)
        return (val2 == null) ? 0 : 1;

      return val1.Value.CompareTo(val2.Value);
    }
  }

  public sealed class DotPathDecimalComparer : IComparer<DotPathObject> {
    public string Field { get; set; }

    public DotPathDecimalComparer(string field) {
      Field = field;
    }

    int IComparer<DotPathObject>.Compare(DotPathObject x, DotPathObject y) {
      var val1 = x[Field] as decimal?;
      var val2 = y[Field] as decimal?;

      if (val1 == null)
        return (val2 == null) ? 0 : 1;

      return val1.Value.CompareTo(val2.Value);
    }
  }
}
