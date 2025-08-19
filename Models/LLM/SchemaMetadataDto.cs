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

    public class ChunkingInfo
    {
        [JsonPropertyName("parameter")]
        public string Parameter { get; set; } = "part";

        [JsonPropertyName("wrapper")]
        public string Wrapper { get; set; } = "ChunkedResponseDto";

        [JsonPropertyName("maxTokensPerPart")]
        public int MaxTokensPerPart { get; set; } = 7000;

        [JsonPropertyName("instructions")]
        public string Instructions { get; set; } = string.Empty;

        [JsonPropertyName("exampleJs")]
        public string? ExampleJs { get; set; }

        [JsonPropertyName("exampleCsharp")]
        public string? ExampleCsharp { get; set; }

        [JsonPropertyName("examplePython")]
        public string? ExamplePython { get; set; }
    }

    public class SchemaMetadataDto
    {
        [JsonPropertyName("apiVersion")]
        public string ApiVersion { get; set; } = "LLM";

        [JsonPropertyName("usage")]
        public string Usage { get; set; } = "Сначала вызовите /llm/schema, затем используйте указанные эндпоинты и поля.";

        [JsonPropertyName("chunking")]
        public ChunkingInfo Chunking { get; set; } = new ChunkingInfo
        {
            Instructions = "If a response is split, call the same endpoint repeatedly with ?part=1..N and concatenate the 'content' fields in order without adding or removing characters; then parse the concatenated string as JSON. If 'is_consequential' is false, 'content' is a JSON object; if true, 'content' is a string fragment of the serialized JSON.",
            ExampleJs = "// fetch parts and reassemble\nasync function fetchAll(url){\n  const parts = [];\n  for(let i=1;;i++){\n    const res = await fetch(url + '&part=' + i);\n    const j = await res.json();\n    parts.push(j.content);\n    if(!j.is_consequential || i >= j.total_parts) break;\n  }\n  const full = parts.join('');\n  return JSON.parse(full);\n}",
            ExampleCsharp = "// HttpClient + System.Text.Json\nusing var client = new HttpClient();\nvar parts = new List<string>();\nfor(int i=1;;i++){\n  var res = await client.GetStringAsync(url + \"&part=\" + i);\n  using var doc = JsonDocument.Parse(res);\n  var root = doc.RootElement;\n  parts.Add(root.GetProperty(\"content\").GetString());\n  if(!root.GetProperty(\"is_consequential\").GetBoolean() || i >= root.GetProperty(\"total_parts\").GetInt32()) break;\n}\nvar full = string.Concat(parts);\nvar obj = JsonSerializer.Deserialize<object>(full);",
            ExamplePython = "# requests\nimport requests, json\nparts = []\nfor i in range(1, 9999):\n    r = requests.get(url + f'&part={i}')\n    j = r.json()\n    parts.append(j['content'])\n    if not j.get('is_consequential') or i >= j.get('total_parts', i):\n        break\nfull = ''.join(parts)\nobj = json.loads(full)"
        };

        [JsonPropertyName("endpoints")]
        public List<ApiEndpointInfo> Endpoints { get; set; } = new();

        [JsonPropertyName("models")]
        public List<ModelInfo> Models { get; set; } = new();
    }
}
