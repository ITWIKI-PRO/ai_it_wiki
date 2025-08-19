using System.Text.Json.Serialization;

namespace ai_it_wiki.Models.LLM
{
    /// <summary>
    /// DTO: Запрос получения описания товара по SKU
    /// </summary>
    public class ProductDescriptionRequestDto
    {
        /// <summary>
        /// SKU товара (строка, обязательна)
        /// </summary>
        [JsonPropertyName("sku")]
        public string Sku { get; set; } = string.Empty;
    }
}
