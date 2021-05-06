using System;
using System.Collections.Generic;

namespace DatusArator.Core.ObjectStore {
  public class DatusAratorObjectStoreFactory {

    private static List<IDatusAratorObjectStore> fBuilders;
    private static List<IDatusAratorObjectStore> Builders {
      get {
        if (fBuilders == null) {
          fBuilders = new List<IDatusAratorObjectStore> {
            new DatusAratorFileObjectStore(null),
            new DatusAratorMemoryObjectStore(),
            new DatusAratorMultiObjectStore(null)
          };
        }

        return fBuilders;
      }
    }

    public static void Register(IDatusAratorObjectStore fBuilder) {
      Builders.Add(fBuilder);
    }

    public static IDatusAratorObjectStore BuildObjectStore(string config) {
      foreach (var builder in Builders) {
        var result = builder.Build(config);
        if (result != null)
          return result;
      }

      return null;
    }
  }
}
