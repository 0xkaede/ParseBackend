using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace ParseBackend.Models.CloudStorage
{
    public class CloudstorageFile
    {
        [JsonProperty("uniqueFilename")]
        public string UniqueFilename { get; set; }

        [JsonProperty("fileName")]
        public string FileName { get; set; }

        [JsonProperty("hash")]
        public string Hash { get; set; }

        [JsonProperty("hash256")]
        public string Hash256 { get; set; }

        [JsonProperty("length")]
        public long Length { get; set; }

        [JsonProperty("contentType")]
        public string ContentType { get; set; }

        [JsonProperty("uploaded")]
        public string Uploaded { get; set; }

        [JsonProperty("storageType")]
        public string StorageType { get; set; }

        [JsonProperty("doNotCache")]
        public bool DoNotCache { get; set; }

        public CloudstorageFile() { }

        public CloudstorageFile(string path, string accountId = null)
        {
            var fileInfo = new FileInfo(path);

            UniqueFilename = fileInfo.Name.Contains("Settings") ? "ClientSettings.Sav" : fileInfo.Name;
            FileName = fileInfo.Name.Contains("Settings") ? "ClientSettings.Sav" : fileInfo.Name;
            Hash = string.Concat(SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(File.ReadAllText(path))).Select(b => b.ToString("x2")));
            Hash256 = string.Concat(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(File.ReadAllText(path))).Select(b => b.ToString("x2")));
            Length = fileInfo.Length;
            ContentType = "application/octet-stream";
            Uploaded = fileInfo.LastWriteTime.ToString("yyyy-MM-ddTHH:mm:ss.sssZ");
            StorageType = "S3";
            DoNotCache = false;
        }
    }
}
