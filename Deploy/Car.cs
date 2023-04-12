using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Newtonsoft.Json;

namespace AzureSearchTraining.Deploy
{
    public partial class Car
    {
        [SimpleField(IsKey = true, IsFilterable = true)]
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        [SearchableField]
        [JsonProperty("description")]
        public string Description { get; set; } = string.Empty;

        [SearchableField(IsFilterable = true, IsSortable = true)]
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [SearchableField(IsFilterable = true, IsFacetable = true, IsSortable = true)]
        [JsonProperty("color")]
        public string Color { get; set; } = string.Empty;

        [SimpleField(IsFilterable = true, IsSortable = true)]
        [JsonProperty("price")]
        public int? Price { get; set; }

        [SimpleField]
        [JsonProperty("type")]
        public string Type { get; set; } = nameof(Car);
    }
}
