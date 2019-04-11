using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Infra.Auth
{
    public class EncryptedObj
    {
        public byte[] EncryptedData { get; set; }
        public byte[] VectorData { get; set; }

        public EncryptedObj()
        {

        }
        public EncryptedObj(byte[] data, byte[] salt)
        {
            EncryptedData = data;
            VectorData = salt;
        }
    }

    /// <summary>
    /// https://msdn.microsoft.com/en-us/library/system.security.cryptography.aescryptoserviceprovider.aspx
    /// </summary>
    public static class AESEncryption
    {
        public static string Password { get; set; }

        public static byte[] EncryptStringToBytes_Aes(string plainText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");
            byte[] encrypted;
            // Create an AesCryptoServiceProvider object
            // with the specified key and IV.
            using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {

                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }
            // Return the encrypted bytes from the memory stream.
            return encrypted;
        }

        public static string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an AesCryptoServiceProvider object
            // with the specified key and IV.
            using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }

            }

            return plaintext;
        }
        public static RijndaelManaged GetKeys(byte[] salt)
        {
            RijndaelManaged myAlg = new RijndaelManaged();
            Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(Password, salt);
            myAlg.Key = key.GetBytes(myAlg.KeySize / 8);
            myAlg.IV = key.GetBytes(myAlg.BlockSize / 8);
            return myAlg;
        }

        public static RijndaelManaged GetKeys(byte[] salt, string secret)
        {
            RijndaelManaged myAlg = new RijndaelManaged();
            Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(secret, salt);
            myAlg.Key = key.GetBytes(myAlg.KeySize / 8);
            myAlg.IV = key.GetBytes(myAlg.BlockSize / 8);
            return myAlg;
        }
    }
}