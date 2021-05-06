using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatusArator.Core.Json {
  public class GenericViewModel : INotifyPropertyChanged, IDisposable {
    public dynamic Data = new DynamicWrapper();

    public GenericViewModel() {
      ((DynamicWrapper)Data).PropertyChanged += InternalPropertyChanged;
    }

    private readonly Dictionary<String, String> fMapping = new Dictionary<String, String>();
    protected void AddMapping(string intKey, string extKey) {
      fMapping[intKey] = extKey;
    }

    public event PropertyChangedEventHandler PropertyChanged;
    private void InternalPropertyChanged(object sender, PropertyChangedEventArgs e) {
      if (fMapping.ContainsKey(e.PropertyName))
        e = new PropertyChangedEventArgs(fMapping[e.PropertyName]);

      PropertyChanged?.Invoke(this, e);
    }

    public void Dispose() {
      Dispose(true);
    }

    protected virtual void Dispose(bool disposing) {
      if (disposing) {
        ((DynamicWrapper)Data).PropertyChanged -= InternalPropertyChanged;
      }
    }
  }
}
