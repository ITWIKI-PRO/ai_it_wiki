using System.Text.Json.Serialization;

namespace ai_it_wiki.Models.LLM
{
    /// <summary>
    /// DTO: Стандартный формат ошибки API
    /// </summary>
    public class ErrorResponseDto
    {
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("details")]
        public string? Details { get; set; }
    }
}
