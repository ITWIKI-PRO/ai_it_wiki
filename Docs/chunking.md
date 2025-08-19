LLM API — сборка и обработка chunked-ответов

Кратко

- Всегда сначала вызывайте `/llm/schema` — там содержатся список эндпоинтов, схема моделей и чёткие инструкции по сборке частей.
- Ответы могут приходить в "chunked" виде. Обёртка возвращается в форме:
  {
    "is_consequential": bool,   // true — ответ разбит на части
    "content": ... ,            // если is_consequential==false — обычный JSON объект; если true — строковый фрагмент
    "part": int,                // текущая часть
    "total_parts": int          // всего частей (1 если не разбито)
  }

Когда ответ разбит

- Если `is_consequential` == true, клиент обязан:
  1) последовательно запрашивать ту же конечную точку с `?part=1`, `?part=2`, ... до `total_parts`;
  2) конкатенировать полученные `content` в порядке частей строго без каких-либо добавленных/удалённых символов;
  3) распарсить итоговую конкатенированную строку как JSON (например, JSON.parse(full)).

Замечание: части — это точные фрагменты сериализованной JSON-строки (token-based split). Они сами по себе обычно не являются валидным JSON, поэтому важно не модифицировать их при сборке.

Примеры

JavaScript (fetch)

```javascript
async function fetchAll(url) {
  const parts = [];
  for (let i = 1;; i++) {
    const res = await fetch(url + (url.includes('?') ? '&' : '?') + 'part=' + i);
    if (!res.ok) throw new Error('HTTP ' + res.status);
    const j = await res.json();
    parts.push(j.content);
    if (!j.is_consequential || i >= j.total_parts) break;
  }
  const full = parts.join('');
  return JSON.parse(full);
}

// usage
// const obj = await fetchAll('https://your-host/llm/products?fields=id');
```

C# (HttpClient + System.Text.Json)

```csharp
using System.Net.Http;
using System.Text.Json;

async Task<object?> FetchAllAsync(string url)
{
    using var client = new HttpClient();
    var parts = new List<string>();
    for (int i = 1; ; i++)
    {
        var res = await client.GetStringAsync(url + (url.Contains('?') ? '&' : '?') + "part=" + i);
        using var doc = JsonDocument.Parse(res);
        var root = doc.RootElement;
        var content = root.GetProperty("content").GetString() ?? string.Empty;
        parts.Add(content);
        if (!root.GetProperty("is_consequential").GetBoolean() || i >= root.GetProperty("total_parts").GetInt32())
            break;
    }
    var full = string.Concat(parts);
    return JsonSerializer.Deserialize<object>(full);
}

// usage: var obj = await FetchAllAsync("http://localhost:5000/llm/products?fields=id");
```

Python (requests)

```python
import requests, json

def fetch_all(url):
    parts = []
    for i in range(1, 10000):
        r = requests.get(url + ('&' if '?' in url else '?') + f'part={i}')
        r.raise_for_status()
        j = r.json()
        parts.append(j['content'])
        if not j.get('is_consequential') or i >= j.get('total_parts', i):
            break
    full = ''.join(parts)
    return json.loads(full)

# usage: obj = fetch_all('http://localhost:5000/llm/products?fields=id')
```

Советы для LLM-агентов (коротко)

- Сначала вызови `/llm/schema` и изучи `chunking.instructions` и примеры.
- Если `is_consequential==true`, вызывай endpoint повторно с part=1..N, собирай `content` точно в порядке и затем парсь JSON.
- Никогда не добавляй пробелы или разделители между частями. Используй точную конкатенацию.

Возможные улучшения (если потребуется в будущем)

- Версия "разбиения по JSON-элементам": чтобы каждая часть была валидным JSON-фрагментом (например, массив элементов) — потребует переработки сериализации и разбиения по границам JSON, а не по токенам.
- Альтернатива: передавать части в base64 (byte-safe). Это более надёжно, но менее удобно для просмотра в Swagger/UI.

Поддержка

Если хочешь, могу добавить в `GET /llm/schema` ещё примеры на другом языке или включить snippet в Swagger UI как пример ответа. Также могу изменить `ChunkedResponseDto` в схеме OpenAPI, чтобы явно указать, что `content` может быть либо string, либо object.
