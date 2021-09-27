﻿using Microsoft.AspNetCore.Mvc;
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

    public static Task<string> GenerateSecret() => Task.Run(static () =>
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
        var sb = new StringBuilder(64);
        for (int i = 0; i < 64; i++)
            sb.Append(chars[rand.Next(0, chars.Length)]);
        return sb.ToString();
    });

    public static Task<string> GetHash(string text, string key) => Task.Run(() =>
    {
        var encoding = Encoding.UTF8;

        byte[] textBytes = encoding.GetBytes(text);
        byte[] keyBytes = encoding.GetBytes(key);

        byte[] hashBytes;
        using (var hash = new HMACSHA512(keyBytes))
            hashBytes = hash.ComputeHash(textBytes);

        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    });

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
