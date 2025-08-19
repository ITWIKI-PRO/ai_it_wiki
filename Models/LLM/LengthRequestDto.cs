using System.Text.Json.Serialization;

namespace ai_it_wiki.Models.LLM
{
    /// <summary>
    /// Request body for length-check endpoint
    /// </summary>
    public class LengthRequestDto
    {
        /// <summary>
        /// Length of the string to generate
        /// </summary>
        [JsonPropertyName("length")]
        public int Length { get; set; }
    }
}
