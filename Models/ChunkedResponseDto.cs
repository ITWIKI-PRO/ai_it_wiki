using System.Text.Json.Serialization;

namespace ai_it_wiki.Models
{
    /// <summary>
    /// Обёртка для больших ответов: либо полный контент, либо часть контента.
    /// </summary>
    public class ChunkedResponseDto
    {
        /// <summary>
        /// Признак, что ответ разбит на последовательные части.
        /// </summary>
        [JsonPropertyName("is_consequential")]
        public bool IsConsequential { get; set; }

        /// <summary>
        /// Текстовая часть контента (JSON-строка или просто текст).
        /// </summary>
        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Номер части (начиная с 1).
        /// </summary>
        [JsonPropertyName("part")]
        public int Part { get; set; }

        /// <summary>
        /// Общее количество частей.
        /// </summary>
        [JsonPropertyName("total_parts")]
        public int TotalParts { get; set; }
    }
}
