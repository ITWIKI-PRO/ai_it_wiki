using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ai_it_wiki.Models.Ozon
{
    /// <summary>
    /// Запрос характеристик товаров
    /// </summary>
    public class ProductAttributesRequest
    {
        /// <summary>
        /// Фильтр поиска
        /// </summary>
        [JsonPropertyName("filter")]
        public ProductAttributesFilter Filter { get; set; } = new();

        /// <summary>
        /// Ограничение количества результатов
        /// </summary>
        [JsonPropertyName("limit")]
        public int? Limit { get; set; }

        /// <summary>
        /// Направление сортировки
        /// </summary>
        [JsonPropertyName("sort_dir")]
        public string? SortDir { get; set; }
    }

    /// <summary>
    /// Фильтр для запроса характеристик
    /// </summary>
    public class ProductAttributesFilter
    {
        [JsonPropertyName("product_id")]
        public List<long>? ProductId { get; set; }

        [JsonPropertyName("offer_id")]
        public List<string>? OfferId { get; set; }

        [JsonPropertyName("sku")]
        public List<string>? Sku { get; set; }

        [JsonPropertyName("visibility")]
        public string? Visibility { get; set; }
    }
}

