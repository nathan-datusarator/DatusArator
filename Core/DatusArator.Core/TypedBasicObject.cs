using System;

using static DatusArator.Core.Util.StringUtils;

namespace DatusArator.Core {
  public class TypedBasicObject {
    public TypedBasicObjectType TBOType { get; private set; }
    public Object Value { get; private set; }

    #region Constructors
    public TypedBasicObject(string value) {
      TBOType = TypedBasicObjectType.STRING;
      this.Value = value;
    }

    public TypedBasicObject(Decimal value) {
      TBOType = TypedBasicObjectType.DECIMAL;
      this.Value = value;
    }

    public TypedBasicObject(Double value) {
      TBOType = TypedBasicObjectType.DOUBLE;
      this.Value = value;
    }

    public TypedBasicObject(int value) {
      TBOType = TypedBasicObjectType.INTEGER;
      this.Value = value;
    }

    public TypedBasicObject(long value) {
      if ((value > Int32.MinValue) && (value < Int32.MaxValue)) {
        TBOType = TypedBasicObjectType.INTEGER;
        this.Value = Convert.ToInt32(value);
      } else {
        TBOType = TypedBasicObjectType.LONG;
        this.Value = value;
      }
    }

    public TypedBasicObject(DateTime value) {
      TBOType = TypedBasicObjectType.DATETIME;
      this.Value = value;
    }

    public TypedBasicObject(bool value) {
      TBOType = TypedBasicObjectType.BOOLEAN;
      this.Value = value;
    }

    public TypedBasicObject(object value) {
      TBOType = ComputeObjectType(value);
      if (TBOType == TypedBasicObjectType.NULL)
        this.Value = null;
      else if (TBOType == TypedBasicObjectType.STRING)
        this.Value = value.ToString();
      else
        this.Value = value;
    }
    #endregion

    #region Object Overrides
    public bool SameValue(Object obj) {
      if (obj == null)
        return false;

      if (obj == this)
        return true;

      if (obj.GetType() != GetType())
        return ToString().Equals(obj.ToString());

      TypedBasicObject rhs = (TypedBasicObject)obj;
      if (Value == null)
        return !rhs.HasValue();

      if (TBOType == TypedBasicObjectType.DOUBLE) {
        double? testValue = rhs.GetAsDouble(null);
        return (testValue != null) && testValue.Equals(Value);
      }

      if (TBOType == TypedBasicObjectType.DECIMAL) {
        decimal? testValue = rhs.GetAsDecimal(null);
        return (testValue != null) && testValue.Equals(Value);
      }

      if ((TBOType == TypedBasicObjectType.DATETIME) || (rhs.TBOType == TypedBasicObjectType.DATETIME)) {
        long? testValue = rhs.GetAsLong(null);
        return (testValue != null) && testValue.Equals(GetAsLong(null));
      }

      if ((TBOType == TypedBasicObjectType.BOOLEAN) || (rhs.TBOType == TypedBasicObjectType.BOOLEAN)) {
        bool testValue = rhs.GetAsBool();
        return testValue == GetAsBool();
      }

      return (TBOType == rhs.TBOType) && Value.Equals(rhs.Value);
    }

    public override string ToString() {
      return GetAsString();
    }

    public override bool Equals(object obj) {
      if (this == obj)
        return true;
      if (obj == null)
        return false;

      return ToString().Equals(obj.ToString());
    }

    public override int GetHashCode() {
      return base.GetHashCode();
    }
    #endregion

    #region Get as Types
    public string GetAsString(string defValue = null) {
      return Value != null ? Value.ToString() : defValue;
    }

    public bool GetAsBool(bool defValue = false) {
      if (Value == null)
        return defValue;

      switch (TBOType) {
        case TypedBasicObjectType.STRING:
          return StringToBool((string)Value);
        case TypedBasicObjectType.DECIMAL:
          return (Decimal)Value != 0;
        case TypedBasicObjectType.DOUBLE:
          return (Double)Value != 0;
        case TypedBasicObjectType.LONG:
          return (long)Value != 0;
        case TypedBasicObjectType.INTEGER:
          return (int)Value != 0;
        case TypedBasicObjectType.DATETIME:
          return true;
        case TypedBasicObjectType.BOOLEAN:
          return (bool)Value;
        default:
          return defValue;
      }
    }

    public double? GetAsDouble(double? defValue = null) {
      try {
        switch (TBOType) {
          case TypedBasicObjectType.STRING:
            return SafeStrToDouble((string)Value, defValue);
          case TypedBasicObjectType.DECIMAL:
            return Convert.ToDouble(Value);
          case TypedBasicObjectType.DOUBLE:
            return (Double)Value;
          case TypedBasicObjectType.LONG:
            return (long)Value;
          case TypedBasicObjectType.INTEGER:
            return (int)Value;
          case TypedBasicObjectType.DATETIME:
            return ((DateTime)Value).Ticks;
          case TypedBasicObjectType.BOOLEAN:
            return (bool)Value ? 1 : 0;
          default:
            return defValue;
        }
      } catch (FormatException) {
        return defValue;
      }
    }

    public decimal? GetAsDecimal(decimal? defValue = null) {
      try {
        switch (TBOType) {
          case TypedBasicObjectType.STRING:
            return SafeStrToDecimal((string)Value, defValue);
          case TypedBasicObjectType.DECIMAL:
            return (Decimal)Value;
          case TypedBasicObjectType.DOUBLE:
            return Convert.ToDecimal(Value);
          case TypedBasicObjectType.LONG:
            return (long)Value;
          case TypedBasicObjectType.INTEGER:
            return (int)Value;
          case TypedBasicObjectType.DATETIME:
            return ((DateTime)Value).Ticks;
          case TypedBasicObjectType.BOOLEAN:
            return (bool)Value ? 1 : 0;
          default:
            return defValue;
        }
      } catch (FormatException) {
        return defValue;
      }

    }

    public int? GetAsInt(int? defValue = null) {
      try {
        switch (TBOType) {
          case TypedBasicObjectType.STRING:
            return SafeStrToInt((string)Value, defValue);
          case TypedBasicObjectType.DECIMAL:
            return Convert.ToInt32(Value);
          case TypedBasicObjectType.DOUBLE:
            return Convert.ToInt32(Value);
          case TypedBasicObjectType.LONG:
            return Convert.ToInt32(Value);
          case TypedBasicObjectType.INTEGER:
            return Convert.ToInt32(Value);
          case TypedBasicObjectType.DATETIME:
            return null;
          case TypedBasicObjectType.BOOLEAN:
            return (bool)Value ? 1 : 0;
          default:
            return defValue;
        }
      } catch (FormatException) {
        return defValue;
      }
    }

    public long? GetAsLong(long? defValue = null) {
      try {
        switch (TBOType) {
          case TypedBasicObjectType.STRING:
            return SafeStrToLong((string)Value, defValue);
          case TypedBasicObjectType.DECIMAL:
            return Convert.ToInt64(Value);
          case TypedBasicObjectType.DOUBLE:
            return Convert.ToInt64(Value);
          case TypedBasicObjectType.LONG:
            return ((long)Value);
          case TypedBasicObjectType.INTEGER:
            return ((int)Value);
          case TypedBasicObjectType.DATETIME:
            return Value != null ? NetToJavaTime((DateTime)Value) : defValue;
          case TypedBasicObjectType.BOOLEAN:
            return (bool)Value ? 1 : 0;
          default:
            return defValue;
        }
      } catch (FormatException) {
        return defValue;
      }
    }

    public DateTime? GetAsDate(DateTime? defaultValue = null) {
      switch (TBOType) {
        case TypedBasicObjectType.STRING:
          return defaultValue;
        case TypedBasicObjectType.DECIMAL:
          return defaultValue;
        case TypedBasicObjectType.DOUBLE:
          return defaultValue;
        case TypedBasicObjectType.LONG:
          return JavaTimeToNet((long)Value);
        case TypedBasicObjectType.INTEGER:
          return JavaTimeToNet((int)Value);
        case TypedBasicObjectType.DATETIME:
          return (DateTime)Value;
        case TypedBasicObjectType.BOOLEAN:
          return defaultValue;
        default:
          return defaultValue;
      }
    }

    public bool IsNotNull() {
      return Value != null;
    }

    public bool HasValue() {
      if (Value == null)
        return false;

      switch (TBOType) {
        case TypedBasicObjectType.STRING:
          return ((string)Value).Length > 0;
        case TypedBasicObjectType.DECIMAL:
          return true;
        case TypedBasicObjectType.DOUBLE:
          return true;
        case TypedBasicObjectType.LONG:
          return true;
        case TypedBasicObjectType.INTEGER:
          return true;
        case TypedBasicObjectType.DATETIME:
          return true;
        case TypedBasicObjectType.BOOLEAN:
          return true;
        case TypedBasicObjectType.NULL:
          return false;
        default:
          return false;
      }
    }
    #endregion

    #region Static Helpers
    public static TypedBasicObjectType ComputeObjectType(object value) {
      if (value == null)
        return TypedBasicObjectType.NULL;

      string typeName = value.GetType().Name.ToUpper();

      if ("STRING".Equals(typeName))
        return TypedBasicObjectType.STRING;
      if ("DECIMAL".Equals(typeName) || "MONEY".Equals(typeName))
        return TypedBasicObjectType.DECIMAL;
      if ("DOUBLE".Equals(typeName))
        return TypedBasicObjectType.DOUBLE;
      if ("DATETIME".Equals(typeName))
        return TypedBasicObjectType.DATETIME;
      if ("LONG".Equals(typeName) || "INT64".Equals(typeName))
        return TypedBasicObjectType.LONG;
      if ("INT".Equals(typeName) || "INTEGER".Equals(typeName) || "INT32".Equals(typeName) || "INT16".Equals(typeName))
        return TypedBasicObjectType.INTEGER;
      if ("BYTE".Equals(typeName))
        return TypedBasicObjectType.INTEGER;
      if ("BOOL".Equals(typeName) || "BOOLEAN".Equals(typeName))
        return TypedBasicObjectType.BOOLEAN;
      if ("DBNULL".Equals(typeName))
        return TypedBasicObjectType.NULL;
      if ("GUID".Equals(typeName))
        return TypedBasicObjectType.STRING;

      return TypedBasicObjectType.STRING;
    }

    public static DateTime JavaTimeToNet(long millis) {
      var UTCBaseTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
      return UTCBaseTime.AddMilliseconds(millis).ToLocalTime();
    }

    public static long NetToJavaTime(DateTime time) {
      DateTime UTCBaseTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

      DateTime utcTime = (time.Date.Kind == DateTimeKind.Unspecified) ? time : time.ToUniversalTime();
      return (utcTime.Ticks - UTCBaseTime.Ticks) / TimeSpan.TicksPerMillisecond;
    }
    #endregion
  }

  public enum TypedBasicObjectType { STRING, INTEGER, DECIMAL, DOUBLE, LONG, DATETIME, BOOLEAN, NULL }
}