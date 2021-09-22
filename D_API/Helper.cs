using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace D_API;

public static class Roles
{
    public const string Root = "root";
    public const string Moderator = "mod";
    public const string Administrator = "admin";
}

public static class Helper
{
    public static Task CheckAuthValidity(this ClaimsPrincipal user)
    {
        if (user.Identity?.Name is null)
        {
            string d = $"An authorized user cannot have a null Identity or Name. Claims: \n*> {string.Join("\n*>", user.Claims.Select(x => $"Type: {x.Type}, Value: {x.Value}, Issuer: {x.Issuer}"))}";
            Log.Error(d);
            throw new InvalidOperationException(d);
        }
        return Task.CompletedTask;
    }
    {
        public static async Task CheckAuthValidity(this ClaimsPrincipal user)

    public static Task<byte[]> EncryptStringToBytesAES(string plainText, byte[] key, byte[] iv) => Task.Run(() =>
    {
        if (plainText is null or { Length: <= 0 })
            throw new ArgumentNullException(nameof(plainText));
        if (key is null or { Length: <= 0 })
            throw new ArgumentNullException(nameof(key));
        if (iv is null or { Length: <= 0 })
            throw new ArgumentNullException(nameof(iv));

        Aes? aesAlg = null;
        MemoryStream? msEncrypt = null;
        CryptoStream? csEncrypt = null;
        StreamWriter? swEncrypt = null;
        try
        {
            aesAlg = Aes.Create();

            aesAlg.Key = key;
            aesAlg.IV = iv;
            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            msEncrypt = new MemoryStream();
            csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
            swEncrypt = new StreamWriter(csEncrypt);
            swEncrypt.Write(plainText);
            return msEncrypt.ToArray();
        }
        finally
        {
            aesAlg?.Dispose();
            msEncrypt?.Dispose();
            csEncrypt?.Dispose();
            swEncrypt?.Dispose();
        }
    });

    public static Task<string> DecryptStringFromBytesAES(byte[] cipherText, byte[] key, byte[] iv) => Task.Run(() =>
    {
        if (cipherText is null or { Length: <= 0 })
            throw new ArgumentNullException(nameof(cipherText));
        if (key is null or { Length: <= 0 })
            throw new ArgumentNullException(nameof(key));
        if (iv is null or { Length: <= 0 })
            throw new ArgumentNullException(nameof(iv));

        Aes? aesAlg = null;
        MemoryStream? msDecrypt = null;
        CryptoStream? csDecrypt = null;
        StreamReader? srDecrypt = null;
        try
        {
            aesAlg = Aes.Create();

            aesAlg.Key = key;
            aesAlg.IV = iv;
            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            msDecrypt = new MemoryStream(cipherText);
            csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            srDecrypt = new StreamReader(csDecrypt);
            return srDecrypt.ReadToEnd();
        }
        finally
        {
            aesAlg?.Dispose();
            msDecrypt?.Dispose();
            csDecrypt?.Dispose();
            srDecrypt?.Dispose();
        }
    });
}
