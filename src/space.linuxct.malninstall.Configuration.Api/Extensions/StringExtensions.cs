using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace space.linuxct.malninstall.Configuration.Extensions
{
    public static class StringExtensions
    {
        public static bool IsValidPackageName(this string packageName)
        {
            return Regex.IsMatch(packageName, @"^[a-zA-Z0-9.]+$");
        }
    }
}