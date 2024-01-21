﻿using Newtonsoft.Json;

namespace ParseBackend.Models.Storefront
{
    public class CatalogEntryBundleItem
    {
        [JsonProperty("bCanOwnMultiple")]
        public bool CanOwnMultiple { get; set; }

        [JsonProperty("regularPrice")]
        public int RegularPrice { get; set; }

        [JsonProperty("discountedPrice")]
        public int DiscountedPrice { get; set; }

        [JsonProperty("alreadyOwnedPriceReduction")]
        public int AlreadyOwnedPriceReduction { get; set; }

        [JsonProperty("item")]
        public CatalogEntryItemGrant Item { get; set; }
    }
}