using System.Text.Json.Serialization;

namespace ai_it_wiki.Models.Ozon
{
    /// <summary>
    /// Запрос описания товара по SKU
    /// </summary>
    public class ProductDescriptionRequest
    {
        /// <summary>
        /// SKU товара
        /// </summary>
        [JsonPropertyName("sku")]
        public string Sku { get; set; } = string.Empty;
    }
}

