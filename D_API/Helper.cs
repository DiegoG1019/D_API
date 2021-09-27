using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace D_API;

public static class Roles
{
    public const string Root = "root";
    public const string AppDataHost = $"{Root},adh";
}

public static class Helper
{
    public static bool GetUserKey(this ClaimsPrincipal user, out Guid key, [NotNullWhen(false)]out string? error)
    {
        Claim? c;
        if ((c = user.Claims.FirstOrDefault(x => x.Type is ClaimTypes.NameIdentifier)) is null)
        {
            error = "The claims of this client are invalid";
            key = Guid.Empty;
            return false;
        }
        if (!Guid.TryParse(c.Value, out key))
        {
            error = "The client's key is invalid";
            return false;
        }
        error = null;
        return true;
    }

    public static string GenerateRandomString(int length)
    {
        var chars = new ReadOnlySpan<char>(new char[] 
        {
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'ñ', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'Ñ', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', 
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
            '{', '}', '[', ']', '(', ')', '@', '#', '$', '%', '^', '&', '*', '_', '-', '=', '+', '*', 
            '~', '\'', ':', ';', '>', '<', '/', '?', '¡', '¿', '|', '`', ',', '.', '!'
        });

        var rand = new Random(DateTime.Now.Millisecond);
        var sb = new StringBuilder(length);
        for (int i = 0; i < length; i++)
            sb.Append(chars[rand.Next(0, chars.Length)]);
        return sb.ToString();
    }

    public static string GenerateUnhashedSecret() => GenerateRandomString(64);
    public static Task<string> GenerateUnhashedSecretAsync() => Task.Run(GenerateUnhashedSecret);

    public static string GenerateRandomSalt() => GenerateRandomString(16);
    public static Task<string> GenerateRandomSaltAsync() => Task.Run(GenerateRandomSalt);

    public static Task<string> GetHashAsync(string text, string key) => Task.Run(() => GetHash(text, key));

    public static string GetHash(string text, string key)
    {
        var encoding = Encoding.UTF8;

        byte[] textBytes = encoding.GetBytes(text);
        byte[] keyBytes = encoding.GetBytes(key);

        byte[] hashBytes;
        using (var hash = new HMACSHA512(keyBytes))
            hashBytes = hash.ComputeHash(textBytes);

        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }

    public static Task<byte[]> EncryptStringToBytesAESAsync(string plainText, byte[] key, byte[] iv) => Task.Run(() => EncryptStringToBytesAES(plainText, key, iv));
    public static byte[] EncryptStringToBytesAES(string plainText, byte[] key, byte[] iv)
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
    }

    public static Task<string> DecryptStringFromBytesAESAsync(byte[] ciphered, byte[] key, byte[] iv) => Task.Run(() => DecryptStringFromBytesAES(ciphered, key, iv));
    public static string DecryptStringFromBytesAES(byte[] ciphered, byte[] key, byte[] iv)
    {
        if (ciphered is null or { Length: <= 0 })
            throw new ArgumentNullException(nameof(ciphered));
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

            msDecrypt = new MemoryStream(ciphered);
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
    }

    public static Task<string> GenerateSecret() => throw new NotImplementedException();
}
