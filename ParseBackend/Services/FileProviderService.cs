using ParseBackend.Utils;
using System.Xml.Linq;
using System;
using static ParseBackend.Global;
using CUE4Parse.FileProvider;
using CUE4Parse.UE4.Objects.Core.Misc;
using CUE4Parse.Encryption.Aes;
using CUE4Parse.UE4.Versions;
using ParseBackend.Enums;
using ParseBackend.Models.Provider;
using LogLevel = ParseBackend.Utils.LogLevel;

namespace ParseBackend.Services
{
    public interface IFileProviderService
    {
        public List<string> GetAllCosmetics();
    }

    public class FileProviderService : IFileProviderService
    {
        public static DefaultFileProvider Provider { get; set; }

        public FileProviderService()
        {
            try
            {
                var providerInfo = GetProviderInfo();
                Provider = new DefaultFileProvider(Config.GamePath, SearchOption.TopDirectoryOnly, false, new VersionContainer(providerInfo.EGame));
                Provider.Initialize();

                var keys = new List<KeyValuePair<FGuid, FAesKey>>
                {
                    new KeyValuePair<FGuid, FAesKey>(new FGuid(), new FAesKey(providerInfo.MainAes))
                };

                Provider.SubmitKeys(keys);

                Logger.Log($"File provider initalized with {Provider.Keys.Count} keys", LogLevel.CUE4Parse);

                foreach (var file in Provider.MountedVfs)
                    Logger.Log($"Mounted File: {file.Name}", LogLevel.CUE4Parse);

            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString(), LogLevel.Error);
            }
        }

        private ProviderInfo GetProviderInfo() => Config.FortniteVersions switch
        {
            FortniteVersions.Version_11_31 => new ProviderInfo(EGame.GAME_UE4_24, "0x6C51ABA88CA1240A0D14EB94701F6C41FD7799B102E9060D1E6C316993196FDF"),
        };

        private List<string> GetAssetsFromPath(string path)
            => Provider.Files.Where(x => x.Key.ToLower().StartsWith(path.ToLower())
            && x.Key.EndsWith(".uasset")).Select(x => x.Key).ToList();

        public List<string> GetAllCosmetics() => GetAssetsFromPath($"FortniteGame/Content/Athena/Items/Cosmetics");
    }
}
