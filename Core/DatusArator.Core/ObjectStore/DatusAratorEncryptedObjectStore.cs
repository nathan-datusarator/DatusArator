using DatusArator.Core.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DatusArator.Core.ObjectStore {
  public class DatusAratorEncryptedObjectStore : DatusAratorBaseObjectStore {
    public override IDatusAratorObjectStore Build(string config) {
      if (config.StartsWith("Encrypted|", StringComparison.InvariantCultureIgnoreCase)) {
        var parts = config.Split('|');
        if (parts.Length < 5)
          return null;

        var store = DatusAratorObjectStoreFactory.BuildObjectStore(parts[3] + "|" + parts[4]);

        return new DatusAratorEncryptedObjectStore(store, parts[1], parts[2]);
      } else
        return null;
    }

    public IDatusAratorObjectStore BaseStore { get; private set; }

    public byte[] Key { get; private set; }
    public byte[] IV { get; private set; }

    public DatusAratorEncryptedObjectStore(IDatusAratorObjectStore baseStore, string key, string salt) {
      BaseStore = baseStore;

      CryptoUtils.CreateAESKeyIV(key, salt, out byte[] aesKey, out byte[] aesIV);

      Key = aesKey;
      IV = aesIV;
    }

    public override List<string> Buckets(string prefix) {
      return BaseStore.Buckets(prefix);
    }

    public override List<string> Ids(string bucket) {
      return BaseStore.Ids(bucket);
    }

    public override Dictionary<string, string> Records(string bucket) {
      var result = BaseStore.Records(bucket);

      var newResult = new Dictionary<string, string>();
      foreach (var entry in result)
        newResult[entry.Key] = CryptoUtils.FromAESBase64(entry.Value, Key, IV);

      return newResult;
    }

    public override string Get(string bucket, string key) {
      var encrypted = BaseStore.Get(bucket, key);

      return (encrypted != null) ? CryptoUtils.FromAESBase64(encrypted, Key, IV) : null;
    }

    public override IDatusAratorObjectStore Put(string bucket, string key, string value, List<ObjectStoreSearchField> searchFields = null) {
      var encrypted = CryptoUtils.ToAESBase64(value, Key, IV);

      BaseStore.Put(bucket, key, encrypted, searchFields);
      return this;
    }

    public override void Clear(string bucket) {
      BaseStore.Clear(bucket);
    }

    public override bool Delete(string bucket, string key) {
      return BaseStore.Delete(bucket, key);
    }

    protected override void Dispose(bool disposing) {
      if (disposing) {
        BaseStore.Dispose();
      }
    }
  }
}
