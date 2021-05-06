using DatusArator.Core.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;

namespace DatusArator.Core.Json {
  public class DynamicWrapper : DynamicObject, INotifyPropertyChanged, IDisposable {
    private PropertyChangedEventHandler fConstructorNotifier = null;
    private readonly List<DynamicWrapper> fChildren = new List<DynamicWrapper>();

    public JsonWrapper Data { get; private set; }

    public DynamicWrapper() : this(null, null) { }
    public DynamicWrapper(JsonWrapper data) : this(data, null) { }
    public DynamicWrapper(PropertyChangedEventHandler propertyChanged) : this(null, propertyChanged) { }

    #region Conversion Shortcuts
    public static bool HasValue(dynamic value) { return (value != null) && !(value is DynamicWrapper);  }

    public static bool ToString(dynamic value) { return HasValue(value) ? value : null; }
    public static bool ToBool(dynamic value) { return HasValue(value) && StringUtils.IsTrue(value); }
    public static double? ToDouble(dynamic value, double? defaultValue) { return HasValue(value) && StringUtils.SafeStrToDouble(value, defaultValue); }
    public static int? ToInt(dynamic value, int? defaultValue) { return HasValue(value) && StringUtils.SafeStrToInt(value, defaultValue); }
    #endregion

    public bool TrackChanges { get { return Data.TrackChanges; } set { Data.TrackChanges = value; } }

    public DynamicWrapper(JsonWrapper data, PropertyChangedEventHandler propertyChanged) {
      if (data == null)
        data = new JsonWrapper();

      Data = data;
      if (propertyChanged != null) {
        PropertyChanged += propertyChanged;
        fConstructorNotifier = propertyChanged;
      }
    }

    public override string ToString() {
      return ToJsonString();
    }

    public string ToJsonString() {
      return Data.ToJsonString();
    }

    #region INotifyPropertyChanged
    public event PropertyChangedEventHandler PropertyChanged;
    protected void RaisePropertyChangedEvent(object sender, string propertyName) {
      PropertyChanged?.Invoke(sender ?? this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion

    #region DynamicObject Main
    public override bool TryGetMember(GetMemberBinder binder, out object result) {
      result = Data.GetRaw(binder.Name)?.ToString();

      if (result == null) {
        DynamicWrapper temp = new DynamicWrapper(new JsonWrapper(Data, binder.Name), PropertyChanged);
        fChildren.Add(temp);

        result = temp;
      }

      return true;
    }

    public override bool TrySetMember(SetMemberBinder binder, object value) {
      object current = Data.Get(binder.Name);
      Data.Put(binder.Name, value);

      if (((current != null) && !current.Equals(value)) || ((current == null) && (value != null))) {
        var sender = Data;
        var key = binder.Name;

        while (sender.ParentWrapper != null) {
          key = Data.JoinKeys(sender.KeyBase, key);
          sender = sender.ParentWrapper;
        }

        RaisePropertyChangedEvent(sender, key);
      }

      return true;
    }
    #endregion

    #region DynamicObject Indexes
    public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result) {
      if (Data.ParentWrapper != null) {
        int index = (int)indexes[0];
        String key = Data.KeyBase + "[" + index + "]";
        result = Data.ParentWrapper.Get(key);

        if (result == null) {
          if (Data.ParentWrapper.GetArrayLength(Data.KeyBase) <= index)
            Data.ParentWrapper.SetArrayLength(Data.KeyBase, index + 1);

          DynamicWrapper temp = new DynamicWrapper(new JsonWrapper(Data.ParentWrapper, key), PropertyChanged);
          fChildren.Add(temp);

          result = temp;
        }

        return true;
      }

      result = null;
      return false;
    }

    public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value) {
      if (Data.ParentWrapper != null) {
        int index = (int)indexes[0];
        Data.ParentWrapper.Put(Data.KeyBase + "[" + index + "]", value);

        if (Data.ParentWrapper.GetArrayLength(Data.KeyBase) <= index)
          Data.ParentWrapper.SetArrayLength(Data.KeyBase, index + 1);

        return true;
      }

      return false;
    }
    #endregion

    #region IDisposable
    public void Dispose() {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing) {
      if (disposing) {
        foreach (var child in fChildren)
          child.Dispose();

        fChildren.Clear();

        if (fConstructorNotifier != null) {
          PropertyChanged -= fConstructorNotifier;
          fConstructorNotifier = null;
        }
      }
    }
    #endregion
  }
}
