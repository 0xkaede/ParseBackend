using ParseBackend.Enums;

namespace ParseBackend.Models.Other
{
    public class Configuration
    {
        public string GamePath { get; set; }
        public FortniteVersions FortniteVersions { get; set; }

        public int FortniteSeason => FortniteVersions switch
        {
            FortniteVersions.Version_7_40 => 7,
            FortniteVersions.Version_8_51 => 8,
            FortniteVersions.Version_11_31 => 11
        };

        public List<StorefrontConfiguration> DailyStoreFront = new List<StorefrontConfiguration>();
        public List<StorefrontConfiguration> WeeklyStoreFront = new List<StorefrontConfiguration>();
    }

    public class StorefrontConfiguration
    {
        public List<string> ItemIds { get; set; }

        /// <summary>
        /// It would be "Panel {num}" (Stacks items in the shops panels like 2 of 3)
        /// </summary>
        public string CategoryNumber { get; set; }

        /// <summary>
        /// You can view em in the asset "FortniteGame/Content/Athena/UI/Frontend/CatalogMessages.uasset"
        /// </summary>
        public string BannerOverride { get; set; }

        public int Price { get; set; }
    }
}
