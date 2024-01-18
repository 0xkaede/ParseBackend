﻿using CUE4Parse.UE4.Versions;
using ParseBackend.Enums;
using ParseBackend.Models.Other;
using System.Security.Cryptography;
using System.Text;

namespace ParseBackend
{
    public static class Global
    {
        public static readonly Configuration Config = new Configuration
        {
            GamePath = "C:\\Users\\jmass\\Desktop\\Fortnite 11.31\\FortniteGame\\Content\\Paks",
            FortniteVersions = FortniteVersions.Version_11_31
        };

        public static string FromBytes(this byte[] bytes) => Encoding.UTF8.GetString(bytes);
        public static byte[] ToBytes(this string txt) => Encoding.UTF8.GetBytes(txt);

        public static string DecodeBase64(this string txt) => Convert.FromBase64String(txt).FromBytes();

        public static string CreateUuid() => Guid.NewGuid().ToString().Replace("-", string.Empty);
        public static string CurrentTime() => DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.sssZ");

        public static string ComputeSHA256Hash(this string input)
        {
            using (var sha256 = SHA256.Create())
            {
                var inputBytes = Encoding.UTF8.GetBytes(input);
                var hashBytes = sha256.ComputeHash(inputBytes);

                var sb = new StringBuilder();
                foreach (byte b in hashBytes)
                    sb.Append(b.ToString("x2"));

                return sb.ToString();
            }
        }

        public static readonly string JWT_SECRET = CreateUuid();
    }
}
