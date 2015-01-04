using System;
using System.Security.Cryptography;
using System.Text;

namespace UltraSonic.Static
{
    public static class StaticMethods
    {
        public static string CalculateSha256(string text, Encoding enc)
        {
            byte[] buffer = enc.GetBytes(text);

            using (var cryptoTransformSha1 = new SHA256CryptoServiceProvider())
                return BitConverter.ToString(cryptoTransformSha1.ComputeHash(buffer)).Replace("-", "");
        }
    }
}
