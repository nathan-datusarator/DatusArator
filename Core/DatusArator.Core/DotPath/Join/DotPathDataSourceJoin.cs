using DatusArator.Core.DotPath.DataSource;
using System;

namespace DatusArator.Core.DotPath.Join {
  public class DotPathDataSourceJoin {
    private readonly string fDataSourceName;
    private IDotPathDataSource fSource;

    public Type LinkType { get; private set; }
    public string ClassName { get; private set; }

    public string LinkName { get; private set; }
    public string ParentKey { get; private set; }
    public string ChildKey { get; private set; }


    public DotPathDataSourceJoin(string dataSourceName, Type linkType, string linkName, string parentKey, string childKey) {
      this.fDataSourceName = dataSourceName;
      this.LinkType = linkType;
      this.LinkName = linkName;
      this.ParentKey = parentKey;
      this.ChildKey = childKey;

    }

    public DotPathDataSourceJoin(string dataSourceName, string className, string linkName, string parentKey, string childKey) {
      this.fDataSourceName = dataSourceName;

      this.ClassName = className;
      this.LinkName = linkName.ToUpper();
      this.ParentKey = parentKey;
      this.ChildKey = childKey;
    }

    public override string ToString() {
      return fDataSourceName + " [" + LinkName + "]";
    }

    public bool Matches(DotPathObject source, string linkName) {
      var keep = linkName.Equals(LinkName, StringComparison.InvariantCultureIgnoreCase);
      keep = keep && (string.IsNullOrEmpty(ClassName) || ClassName.Equals(source.ClassName, StringComparison.InvariantCultureIgnoreCase));
      keep = keep && ((LinkType == null) || LinkType.IsAssignableFrom(source.GetType()));

      return keep;
    }

    public bool CanJoin(Type parentType, string className) {
      var keep = (string.IsNullOrEmpty(ClassName) || ClassName.Equals(className, StringComparison.InvariantCultureIgnoreCase));
      keep = keep && ((LinkType == null) || LinkType.IsAssignableFrom(parentType));

      return keep;
    }

    public IDotPathDataSource DataSource {
      get {
        if (fSource == null)
          fSource = DotPathDataSources.Get(fDataSourceName);

        return fSource;
      }
    }

    public DotPathObject Get(DotPathObject source) {
      var key = source[ParentKey]?.ToString();
      return !string.IsNullOrEmpty(key) ? Get(key) : null;
    }

    public DotPathObject Get(string parentKeyValue) {
      return Get(ChildKey, parentKeyValue);
    }

    public DotPathObject Get(string keyName, string keyValue) {
      return DataSource?.Get(keyName, keyValue);
    }
  }
}
