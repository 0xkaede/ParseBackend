using CUE4Parse.UE4.Objects.GameplayTags;
using CUE4Parse.UE4.Versions;
using ParseBackend.Enums;
using ParseBackend.Models.Other;
using ParseBackend.Models.Xmpp;
using ParseBackend.Services;
using ParseBackend;
using System.Security.Cryptography;
using System.Text;
using ParseBackend.Xmpp;

namespace ParseBackend
{
    public static class Global
    {
        public static readonly Configuration Config = new Configuration
        {
            GamePath = "C:\\Users\\jmass\\Desktop\\Fortnite 8.51\\FortniteGame\\Content\\Paks",
            FortniteVersions = FortniteVersions.Version_8_51,
            WeeklyStoreFront = new List<StorefrontConfiguration>
            {
                new StorefrontConfiguration
                {
                    ItemIds = new List<string> { "CID_395_Athena_Commando_F_ShatterFly", "BID_256_ShatterFly" },
                    BannerOverride = "Back",
                    CategoryNumber = "Panel 1",
                    Price = 1200
                },
                new StorefrontConfiguration
                {
                    ItemIds = new List<string> { "Glider_ID_140_ShatterFly" },
                    BannerOverride = "CollectTheSet",
                    CategoryNumber = "Panel 1",
                    Price = 1200
                },
                new StorefrontConfiguration
                {
                    ItemIds = new List<string> { "Wrap_051_ShatterFly" },
                    BannerOverride = "CollectTheSet",
                    CategoryNumber = "Panel 1",
                    Price = 300
                },
                new StorefrontConfiguration
                {
                    ItemIds = new List<string> { "CID_654_Athena_Commando_F_GiftWrap" },
                    BannerOverride = "Winter",
                    CategoryNumber = "Panel 2",
                    Price = 1200
                },
            },
            DailyStoreFront = new List<StorefrontConfiguration>
            {
                new StorefrontConfiguration
                {
                    ItemIds = new List<string> { "CID_149_Athena_Commando_F_SoccerGirlB" },
                    BannerOverride = "Back",
                    CategoryNumber = null,
                    Price = 500
                },
                new StorefrontConfiguration
                {
                    ItemIds = new List<string> { "CID_146_Athena_Commando_M_SoccerDudeC" },
                    BannerOverride = "",
                    CategoryNumber = null,
                    Price = 1200
                },
                new StorefrontConfiguration
                {
                    ItemIds = new List<string> { "CID_586_Athena_Commando_F_PunkDevil" },
                    BannerOverride = "MostPopular",
                    CategoryNumber = null,
                    Price = 300
                },
                new StorefrontConfiguration
                {
                    ItemIds = new List<string> { "Glider_ID_169_VoyagerRemix" },
                    BannerOverride = "Winter",
                    CategoryNumber = null,
                    Price = 1200
                },
            }
        };

        public static string FromBytes(this byte[] bytes) => Encoding.UTF8.GetString(bytes);
        public static byte[] ToBytes(this string txt) => Encoding.UTF8.GetBytes(txt);

        public static string DecodeBase64(this string txt) => Convert.FromBase64String(txt).FromBytes();

        public static string CreateUuid() => Guid.NewGuid().ToString().Replace("-", string.Empty);
        public static string CurrentTime() => DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.sssZ");
        public static string TimeToString(this DateTime dt) => dt.ToString("yyyy-MM-ddTHH:mm:ss.sssZ");

        public static Dictionary<string, XmppClient> GlobalClients { get; set; } = new();
        public static List<MUCRoom> GlobalMucRooms { get; set; } = new();

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

        public static IMongoService GlobalMongoService { get; set; }
        public static XmppServer GlobalXmppServer { get; set; }

        public static readonly string JWT_SECRET = CreateUuid();

        public static string GetLastTag(this FGameplayTag tag) => tag.TagName.PlainText.Split(".").LastOrDefault()!;
    }
}
