using System.Text.Json.Serialization;

namespace ai_it_wiki.Models.LLM
{
    /// <summary>
    /// DTO: Ответ с описанием товара
    /// </summary>
    public class ProductDescriptionResponseDto
    {
        /// <summary>
        /// SKU товара
        /// </summary>
        [JsonPropertyName("sku")]
        public string Sku { get; set; } = string.Empty;

        /// <summary>
        /// Текст описания (может содержать HTML)
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Идентификатор товара в Ozon (если доступен)
        /// </summary>
        [JsonPropertyName("offer_id")]
        public string? OfferId { get; set; }

        /// <summary>
        /// Название товара (если доступно)
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }
}
