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
    /// Содержимое части: может быть JSON-объектом (при неразбитом ответе или json-режиме) либо строкой (raw/base64).
        /// </summary>
    [JsonPropertyName("content")]
    public object? Content { get; set; }

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
