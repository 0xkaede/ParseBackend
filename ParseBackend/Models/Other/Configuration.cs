using ParseBackend.Enums;

namespace ParseBackend.Models.Other
{
    public class Configuration
    {
        /// <summary>
        /// The path to your build paks folder
        /// </summary>
        public string GamePath { get; set; }

        /// <summary>
        /// The version you are hosting
        /// </summary>
        public FortniteVersions FortniteVersions { get; set; }

        /// <summary>
        /// Gets the seasons number
        /// </summary>
        /// 
        public int FortniteSeason => FortniteVersions switch
        {
            FortniteVersions.Version_7_40 => 7,
            FortniteVersions.Version_8_51 => 8,
            FortniteVersions.Version_11_31 => 11
        };

        /// <summary>
        /// The days when user recive a new refund token
        /// </summary>
        public int RefundsRefundsEveyDays { get; set; }

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

        /// <summary>
        /// Set the price off the item
        /// </summary>
        public int Price { get; set; }
    }
}
