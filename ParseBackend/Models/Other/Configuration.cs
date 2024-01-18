using ParseBackend.Enums;

namespace ParseBackend.Models.Other
{
    public class Configuration
    {
        public string GamePath { get; set; }
        public FortniteVersions FortniteVersions { get; set; }

        public int FortniteSeason => FortniteVersions switch
        {
            FortniteVersions.Version_8_51 => 8,
            FortniteVersions.Version_11_31 => 11
        };
    }
}
