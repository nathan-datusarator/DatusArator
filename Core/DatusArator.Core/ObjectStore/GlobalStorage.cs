using DatusArator.Core.Json;

namespace DatusArator.Core.ObjectStore {
  public static class GlobalStorage {
    public static JsonWrapper Settings { get; set; }

    private static BaseStorage fStorage;
    public static BaseStorage Storage {
      get {
        if (fStorage == null)
          fStorage = new BaseStorage();

        return fStorage;
      }
    }

    public static void DumpInstance() {
      fStorage?.DumpInstance();
      fStorage = null;
    }
  }
}
