using CUE4Parse.UE4.Versions;

namespace ParseBackend.Models.Provider
{
    public class ProviderInfo
    {
        public ProviderInfo(EGame eGame, string mainAes)
        {
            EGame = eGame;
            MainAes = mainAes;
        }
    
        public EGame EGame { get; set; }
        public string MainAes { get; set; }
    }
}
