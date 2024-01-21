using Newtonsoft.Json;

namespace ParseBackend.Models.Storefront
{
    public class CatalogEntryItemGrant
    {
        [JsonProperty("templateId")]
        public string TemplateId { get; set; }

        [JsonProperty("quantity")]
        public int Quantity { get; set; }
    }
}