using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;

public class OpenApiControllerBase : ControllerBase
{
    public List<string> responseParts { get; private set; }

    public enum ChunkMode
    {
        // Split serialized JSON by tokens (raw string chunks)
        Raw = 0,

        // Split by JSON array elements when possible (each part is a valid JSON array of items)
        JsonArray = 1,

        // Return base64-encoded chunks of the serialized JSON
        Base64 = 2,
    }

    private List<string> SplitResponse(string response, int maxSize)
    {
        List<string> parts = new List<string>();

        var gPT4OTokenizer = new Microsoft.KernelMemory.AI.OpenAI.GPT4oTokenizer();
        var tokens = gPT4OTokenizer.GetTokens(response);
        int startIndex = 0;

        while (startIndex < tokens.Count)
        {
            int endIndex = Math.Min(startIndex + maxSize, tokens.Count);
            var partTokens = tokens.Skip(startIndex).Take(endIndex - startIndex).ToList();
            var partText = string.Concat(partTokens);
            parts.Add(partText);
            startIndex = endIndex;
        }

        return parts;
    }

    [Produces("application/json")]
    public IActionResult SplitedResponse(object payload, int part, ChunkMode mode = ChunkMode.Raw)
    {
        // Serialize once using consistent options (camelCase, ignore nulls)
        var options = new System.Text.Json.JsonSerializerOptions
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System
                .Text
                .Json
                .Serialization
                .JsonIgnoreCondition
                .WhenWritingNull,
            WriteIndented = false,
        };

        var text = System.Text.Json.JsonSerializer.Serialize(payload, options);

        if (mode == ChunkMode.JsonArray)
        {
            return JsonArrayModeResponse(text, part);
        }
        else if (mode == ChunkMode.Base64)
        {
            return Base64ModeResponse(text, part);
        }
        else
        {
            return RawModeResponse(text, part);
        }
    }

    private IActionResult RawModeResponse(string text, int part)
    {
        var gPT4OTokenizer = new Microsoft.KernelMemory.AI.OpenAI.GPT4oTokenizer();
        var tokensCount = gPT4OTokenizer.CountTokens(text);
        if (tokensCount > 7000)
        {
            responseParts = SplitResponse(text, 7000);
            if (part > responseParts.Count || part < 1)
            {
                return BadRequest("Запрашиваемая часть не существует.");
            }

            return new JsonResult(
                new
                {
                    is_consequential = true,
                    content = responseParts[part - 1], // raw string fragment
                    part,
                    total_parts = responseParts.Count,
                }
            );
        }
        var options = new System.Text.Json.JsonSerializerOptions
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System
                .Text
                .Json
                .Serialization
                .JsonIgnoreCondition
                .WhenWritingNull,
            WriteIndented = false,
        };
        var obj = System.Text.Json.JsonSerializer.Deserialize<object>(text, options);
        return new JsonResult(
            new
            {
                is_consequential = false,
                content = obj,
                part = 1,
                total_parts = 1,
            }
        );
    }

    private IActionResult Base64ModeResponse(string text, int part)
    {
        // Always chunk by bytes and base64-encode each chunk
        var bytes = System.Text.Encoding.UTF8.GetBytes(text);
        const int maxBytes = 32_000; // approx safe chunk size
        if (bytes.Length > maxBytes)
        {
            var parts = new List<string>();
            for (int i = 0; i < bytes.Length; i += maxBytes)
            {
                var sliceLen = Math.Min(maxBytes, bytes.Length - i);
                var slice = new byte[sliceLen];
                System.Buffer.BlockCopy(bytes, i, slice, 0, sliceLen);
                parts.Add(System.Convert.ToBase64String(slice));
            }
            if (part < 1 || part > parts.Count)
                return BadRequest("Запрашиваемая часть не существует.");

            return new JsonResult(
                new
                {
                    is_consequential = true,
                    content = parts[part - 1], // base64
                    part,
                    total_parts = parts.Count,
                    encoding = "base64",
                }
            );
        }
        var options = new System.Text.Json.JsonSerializerOptions
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System
                .Text
                .Json
                .Serialization
                .JsonIgnoreCondition
                .WhenWritingNull,
            WriteIndented = false,
        };
        var obj = System.Text.Json.JsonSerializer.Deserialize<object>(text, options);
        return new JsonResult(
            new
            {
                is_consequential = false,
                content = obj,
                part = 1,
                total_parts = 1,
                encoding = "none",
            }
        );
    }

    private IActionResult JsonArrayModeResponse(string text, int part)
    {
        // Try to detect a top-level array and split by items into subarrays of limited size
        using var doc = System.Text.Json.JsonDocument.Parse(text);
        if (doc.RootElement.ValueKind == System.Text.Json.JsonValueKind.Array)
        {
            var arr = doc.RootElement;
            const int maxItemsPerPart = 100; // pragmatic default
            int totalParts = (arr.GetArrayLength() + maxItemsPerPart - 1) / maxItemsPerPart;
            if (totalParts <= 1)
            {
                // return full deserialized object
                var optionsWhole = new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = System
                        .Text
                        .Json
                        .Serialization
                        .JsonIgnoreCondition
                        .WhenWritingNull,
                    WriteIndented = false,
                };
                var whole = System.Text.Json.JsonSerializer.Deserialize<object>(text, optionsWhole);
                return new JsonResult(
                    new
                    {
                        is_consequential = false,
                        content = whole,
                        part = 1,
                        total_parts = 1,
                    }
                );
            }
            if (part < 1 || part > totalParts)
                return BadRequest("Запрашиваемая часть не существует.");

            // Create a subarray for the requested part
            int start = (part - 1) * maxItemsPerPart;
            int end = System.Math.Min(start + maxItemsPerPart, arr.GetArrayLength());
            var options = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System
                    .Text
                    .Json
                    .Serialization
                    .JsonIgnoreCondition
                    .WhenWritingNull,
                WriteIndented = false,
            };
            var itemsObj = new System.Collections.Generic.List<object>(end - start);
            int idx = 0;
            foreach (var el in arr.EnumerateArray())
            {
                if (idx >= start && idx < end)
                {
                    var raw = el.GetRawText();
                    var o = System.Text.Json.JsonSerializer.Deserialize<object>(raw, options);
                    itemsObj.Add(o!);
                }
                if (idx >= end)
                    break;
                idx++;
            }
            return new JsonResult(
                new
                {
                    is_consequential = true,
                    content = itemsObj, // valid JSON array part as actual objects
                    part,
                    total_parts = totalParts,
                    mode = "json-array",
                }
            );
        }

        // Fallback to raw mode if not an array
        return RawModeResponse(text, part);
    }
}
