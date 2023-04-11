using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using System.Text.Json.Serialization;

namespace AzureSearchTraining
{
    public partial class Car
    {
        [SimpleField(IsKey = true, IsFilterable = true)]
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [SearchableField]
        [JsonPropertyName("description")]
        public string Description { get; set; }

        [SearchableField(IsFilterable = true, IsSortable = true)]
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [SearchableField(IsFilterable = true, IsFacetable = true, IsSortable = true)]
        [JsonPropertyName("color")]
        public string Color { get; set; }
        
        [SimpleField(IsFilterable = true, IsSortable = true)]
        [JsonPropertyName("price")]
        public int? Price { get; set; }
    }
}
