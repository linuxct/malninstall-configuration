using System.Security.Cryptography;
using System.Text;

namespace space.linuxct.malninstall.Configuration.Common.Extensions
{
    public static class HashingExtensions
    {
        public static string ToSha256(this string rawData)
        {
            var rawDataAsByteArray = Encoding.UTF8.GetBytes(rawData);
            return rawDataAsByteArray.ToSha256();
        }

        public static string ToSha256(this byte[] input)
        {
            using var sha256Hash = SHA256.Create();
            var bytes = sha256Hash.ComputeHash(input);
            var builder = new StringBuilder();  
            foreach (var b in bytes)
            {
                builder.Append(b.ToString("x2"));
            }  
            return builder.ToString();
        }
    }
}