using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace lockit
{
    internal class Crypt
    {
        public static string GetSaltBytes()
        {
            byte[] Bytes = new byte[64];
            RNGCryptoServiceProvider Rng = new RNGCryptoServiceProvider();
            Rng.GetBytes(Bytes);
            return Convert.ToBase64String(Bytes);
        }

        public static string GetHash(string password, string salt)
        {
            List<byte> PasswordSaltBytes = new List<byte>();
            PasswordSaltBytes.AddRange(Encoding.UTF8.GetBytes(password));
            PasswordSaltBytes.AddRange(Convert.FromBase64CharArray(salt.ToCharArray(), 0, salt.Length));
            HashAlgorithm algo = SHA512.Create();
            return Convert.ToBase64String(algo.ComputeHash(PasswordSaltBytes.ToArray()));
        }
    }
}
