using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace UltraSonic.Static
{
    public static class StaticMethods
    {
        public static string CalculateSha256(string text, Encoding enc)
        {
            byte[] buffer = enc.GetBytes(text);
            var cryptoTransformSha1 = new SHA256CryptoServiceProvider();
            return BitConverter.ToString(cryptoTransformSha1.ComputeHash(buffer)).Replace("-", "");
        }

        public static DateTime DateTimeFromUnixTimestamp(long timestamp)
        {
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return dtDateTime.AddMilliseconds(timestamp);
        }
    }
}
