using System;
using System.IO;
using System.Security.Cryptography;

/// <summary>
/// encrypt and decrypt strings
/// </summary>
public static class Encryption
{
    /// <summary>
    /// Encrypts the string.
    /// </summary>
    /// <param Name="clearText">The clear text.</param>
    /// <param Name="Key">The key.</param>
    /// <param Name="IV">The IV.</param>
    /// <returns></returns>
    private static byte[] EncryptString(byte[] clearText, byte[] Key, byte[] IV)
    {
        MemoryStream ms = new();
        Rijndael alg = Rijndael.Create();
        alg.Key = Key;
        alg.IV = IV;
        CryptoStream cs = new(ms, alg.CreateEncryptor(), CryptoStreamMode.Write);
        cs.Write(clearText, 0, clearText.Length);
        cs.Close();
        byte[] encryptedData = ms.ToArray();
        return encryptedData;
    }

    /// <summary>
    /// Encrypts the string.
    /// </summary>
    /// <param Name="clearText">The clear text.</param>
    /// <param Name="Password">The password.</param>
    /// <returns></returns>
    public static string EncryptString(string clearText, string Password)
    {
        if (string.IsNullOrEmpty(Password) || string.IsNullOrEmpty(clearText))
            return clearText;
        byte[] clearBytes = System.Text.Encoding.Unicode.GetBytes(clearText);
        PasswordDeriveBytes pdb = new(Password, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
        byte[] encryptedData = EncryptString(clearBytes, pdb.GetBytes(32), pdb.GetBytes(16));
        return Convert.ToBase64String(encryptedData);
    }

    /// <summary>
    /// Decrypts the string.
    /// </summary>
    /// <param Name="cipherData">The cipher data.</param>
    /// <param Name="Key">The key.</param>
    /// <param Name="IV">The IV.</param>
    /// <returns></returns>
    private static byte[] DecryptString(byte[] cipherData, byte[] Key, byte[] IV)
    {
        MemoryStream ms = new();
        Rijndael alg = Rijndael.Create();
        alg.Key = Key;
        alg.IV = IV;
        CryptoStream cs = new(ms, alg.CreateDecryptor(), CryptoStreamMode.Write);
        cs.Write(cipherData, 0, cipherData.Length);
        cs.Close();
        byte[] decryptedData = ms.ToArray();
        return decryptedData;
    }

    /// <summary>
    /// Decrypts the string.
    /// </summary>
    /// <param Name="cipherText">The cipher text.</param>
    /// <param Name="Password">The password.</param>
    /// <returns></returns>
    public static string DecryptString(string cipherText, string Password)
    {
        if (string.IsNullOrEmpty(Password))
            return cipherText;

        try
        {
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            PasswordDeriveBytes pdb = new(Password, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
            byte[] decryptedData = DecryptString(cipherBytes, pdb.GetBytes(32), pdb.GetBytes(16));
            return System.Text.Encoding.Unicode.GetString(decryptedData);
        }
        catch 
        {
            return "";
        }
    }
}
