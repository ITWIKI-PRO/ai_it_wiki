using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ai_it_wiki.Models.LLM
{
    public class ApiParameterInfo
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("in")]
        public string In { get; set; } = "query"; // query, path, header, cookie, body

        [JsonPropertyName("required")]
        public bool Required { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; } = "string";

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("default")]
        public object? Default { get; set; }
    }

    public class ApiEndpointInfo
    {
        [JsonPropertyName("path")]
        public string Path { get; set; } = string.Empty;

        [JsonPropertyName("method")]
        public string Method { get; set; } = "GET";

        [JsonPropertyName("summary")]
        public string? Summary { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("parameters")]
        public List<ApiParameterInfo> Parameters { get; set; } = new();
    }

    public class ModelFieldInfo
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("nullable")]
        public bool Nullable { get; set; }
    }

    public class ModelInfo
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("fields")]
        public List<ModelFieldInfo> Fields { get; set; } = new();

        // Набор полей, поддерживаемых параметром fields (если применимо)
        [JsonPropertyName("selectableFields")]
        public List<string>? SelectableFields { get; set; }
    }

    public class SchemaMetadataDto
    {
        [JsonPropertyName("apiVersion")]
        public string ApiVersion { get; set; } = "LLM";

        [JsonPropertyName("usage")]
        public string Usage { get; set; } = "Сначала вызовите /llm/schema, затем используйте указанные эндпоинты и поля.";

        [JsonPropertyName("chunking")]
        public object Chunking { get; set; } = new { parameter = "part", wrapper = "ChunkedResponseDto", maxTokensPerPart = 7000 };

        [JsonPropertyName("endpoints")]
        public List<ApiEndpointInfo> Endpoints { get; set; } = new();

        [JsonPropertyName("models")]
        public List<ModelInfo> Models { get; set; } = new();
    }
}
