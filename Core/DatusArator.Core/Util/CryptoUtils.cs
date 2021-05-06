using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace DatusArator.Core.Util {
  public class CryptoUtils {
    #region Base64
    public static string Base64(byte[] bytes) {
      return (bytes != null) ? Convert.ToBase64String(bytes) : null;
    }

    public static byte[] FromBase64(string value) {
      return (value != null) ? Convert.FromBase64String(value) : null;
    }

    public static string GzipBase64String(string value) {
      if (string.IsNullOrEmpty(value))
        return null;

      byte[] bytes = StringUtils.GZipData(value);
      return Base64(bytes);
    }

    public static string FromGzipBase64String(string value) {
      if (string.IsNullOrEmpty(value))
        return null;

      byte[] data = FromBase64(value);
      return Encoding.UTF8.GetString(StringUtils.FromGzipData(data));
    }
    #endregion

    #region Sha256
    public static byte[] Sha256(string message) {
      using (var sha256Engine = SHA256Managed.Create()) {
        byte[] bytes = Encoding.ASCII.GetBytes(message);
        return sha256Engine.ComputeHash(bytes);
      }
    }

    public static byte[] HmacSha256(string key, string message) {
      byte[] keyBytes = Encoding.ASCII.GetBytes(key);
      using (HMACSHA256 hmac = new HMACSHA256(keyBytes)) {
        byte[] messageBytes = Encoding.ASCII.GetBytes(message);
        return hmac.ComputeHash(messageBytes);
      }
    }

    public static string Base64Sha256(string message) {
      return Base64(Sha256(message));
    }

    public static string Base64HmacSha256(string key, string message) {
      return Base64(HmacSha256(key, message));
    }
    #endregion

    #region AES
    public static void CreateAESKeyIV(string keystring, string saltstring, out byte[] aesKey, out byte[] aesIV) {
      using (var myAlg = new AesCryptoServiceProvider()) {
        byte[] salt = Encoding.ASCII.GetBytes(saltstring.PadRight(8, '!'));

        using (var key = new Rfc2898DeriveBytes(keystring, salt)) {
          aesKey = key.GetBytes(myAlg.KeySize / 8);
          aesIV = key.GetBytes(myAlg.BlockSize / 8);
        }
      }
    }

    private static byte[] fGlobalAesKey = null;
    private static byte[] fGlobalAesIV = null;
    public static void CreateGlobalAESKey(string keystring, string saltstring) {
      CreateAESKeyIV(keystring, saltstring, out fGlobalAesKey, out fGlobalAesIV);
    }

    public static string ToAESBase64(string plainText, byte[] key = null, byte[] iv = null) {
      return Base64(ToAESBytes(plainText, key, iv));
    }

    public static string FromAESBase64(string plainText, byte[] key = null, byte[] iv = null) {
      var value = FromBase64(plainText);

      return FromAESBytes(value, key, iv);
    }

    // AES FROM MSDN
    public static byte[] ToAESBytes(string plainText, byte[] key = null, byte[] iv = null) {
      key = key ?? fGlobalAesKey;
      iv = iv ?? fGlobalAesIV;

      // Check arguments.
      if (plainText == null || plainText.Length <= 0)
        throw new ArgumentNullException("plainText");
      if (key == null || key.Length <= 0)
        throw new ArgumentNullException("Key");
      if (iv == null || iv.Length <= 0)
        throw new ArgumentNullException("IV");

      byte[] encrypted;

      // Create an AesCryptoServiceProvider object
      // with the specified key and IV.
      using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider()) {
        aesAlg.Key = key;
        aesAlg.IV = iv;

        // Create a decrytor to perform the stream transform.
        ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

        // Create the streams used for encryption.
        var msEncrypt = new MemoryStream();
        var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt)) {

          //Write all data to the stream.
          swEncrypt.Write(plainText);
        }
        encrypted = msEncrypt.ToArray();
      }

      // Return the encrypted bytes from the memory stream.
      return encrypted;
    }

    static string FromAESBytes(byte[] cipherText, byte[] key = null, byte[] iv = null) {
      key = key ?? fGlobalAesKey;
      iv = iv ?? fGlobalAesIV;

      // Check arguments.
      if (cipherText == null || cipherText.Length <= 0)
        throw new ArgumentNullException("cipherText");
      if (key == null || key.Length <= 0)
        throw new ArgumentNullException("Key");
      if (iv == null || iv.Length <= 0)
        throw new ArgumentNullException("IV");

      // Declare the string used to hold
      // the decrypted text.
      string plaintext = null;

      // Create an AesCryptoServiceProvider object
      // with the specified key and IV.
      using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider()) {
        aesAlg.Key = key;
        aesAlg.IV = iv;

        // Create a decrytor to perform the stream transform.
        ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

        // Create the streams used for decryption.
        var msDecrypt = new MemoryStream(cipherText);
        var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
        using (StreamReader srDecrypt = new StreamReader(csDecrypt)) {
          // Read the decrypted bytes from the decrypting stream
          // and place them in a string.
          plaintext = srDecrypt.ReadToEnd();
        }
      }

      return plaintext;
    }
    #endregion
  }
}
