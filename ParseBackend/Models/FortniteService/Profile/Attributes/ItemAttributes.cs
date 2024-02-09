using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace ParseBackend.Models.FortniteService.Profile.Attributes
{
    public class ItemAttributes : BaseAttributes
    {
        [JsonProperty("variants")]
        public List<Variant> Variants { get; set; }

        [JsonProperty("rnd_sel_cnt")]
        public int RandomSelectionCount { get; set; }
    }

    public class Variant
    {
        [JsonProperty("channel"), BsonElement("channel")]
        public string Channel { get; set; }

        [JsonProperty("active"), BsonElement("active")]
        public string Active { get; set; }

        [JsonProperty("owned"), BsonElement("owned")]
        public List<string> Owned { get; set; } //skinkyyyyy bozzzzooo
    }
}
