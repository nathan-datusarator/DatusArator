using DatusArator.Core.DAO;
using System;
using System.Collections;

namespace DatusArator.Core.DotPath.DataSource {
  public interface IDotPathDataSource {
    string Key { get; set; }
    string DisplayName { get; }
    bool IsSystem { get; }
    bool IsPrimary { get; }

    string SuperGrid { get; }

    Type LinkType { get; }
    string ClassName { get; }
    Schema Schema { get; }

    DotPathObject Get(string keyName, string keyValue);
    IEnumerable GetChildren(string keyName, string keyValue);

    IList GetAll();
  }
}
