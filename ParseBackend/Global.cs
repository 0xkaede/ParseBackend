using System.Security.Cryptography;
using System.Text;

namespace ParseBackend
{
    public static class Global
    {
        public static string FromBytes(this byte[] bytes) => Encoding.UTF8.GetString(bytes);
        public static byte[] ToBytes(this string txt) => Encoding.UTF8.GetBytes(txt);

        public static string DecodeBase64(this string txt) => Convert.FromBase64String(txt).FromBytes();

        public static string CreateUuid() => Guid.NewGuid().ToString().Replace("-", string.Empty);

        public static string ComputeSHA256Hash(string input)
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
    }
}
