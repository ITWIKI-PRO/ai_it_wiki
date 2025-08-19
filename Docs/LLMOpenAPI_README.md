# LLM OpenAPI — руководство по использованию

Это краткое руководство по контроллеру `LLMOpenAPIController` (группа Swagger: `LLMOpenAPI`). Здесь описаны эндпоинты, параметр фильтрации `fields`, а также типовые сценарии комбинирования методов с минимальным трафиком.

## Общие принципы

- Все эндпоинты поддерживают необязательный query-параметр `fields` (можно указывать несколько):
  - Пример: `?fields=sku&fields=description` или `?fields=sku,description`
  - Если `fields` не задан — возвращается полный объект.
  - Неизвестные поля игнорируются (whitelist на стороне сервера).
- В ответах массивы могут проектироваться в укороченный набор ключей согласно `fields`.
- Описание товара содержит HTML (например, `<br/>`). Это ожидаемо и нормально.

## Эндпоинты

1) GET `/llm/products`
   - Назначение: получить список товаров (упрощённые элементы `ProductListItem`).
   - Query:
     - `offerIds: string[]` (опц.)
     - `productIds: long[]` (опц.)
     - `skus: string[]` (опц.)
     - `lastId: string` (опц.)
     - `limit: int` (опц., по умолчанию 50)
     - `fields: string[]` (опц.) — whitelist: `product_id, offer_id, has_fbo_stocks, is_discounted`
   - Ответ: `ProductListItem[]`

2) POST `/llm/products/info`
   - Назначение: получить подробную информацию о товарах.
   - Body: `{ "product_id": long[] }`
   - Query: `fields: string[]` (опц.) — whitelist (поддерживаются без/с префиксом `items.`):
     - `product_id, offer_id, sku, name, description_category_id, attributes, items`
   - Ответ: `{ "items": ProductItem[] }`

3) POST `/llm/product/description`
   - Назначение: получить описание товара по SKU.
   - Body: `{ "sku": string }`
   - Query: `fields: string[]` (опц.) — whitelist: `sku, description, offer_id, name`
   - Ответ: `ProductDescriptionResponseDto` с полями `sku`, `description`, `offer_id?`, `name?`

4) POST `/llm/ratings`
   - Назначение: получить контент-рейтинг по нескольким SKU.
   - Body: `{ "skus": long[] }`
   - Query: `fields: string[]` (опц.) — whitelist: `result, sku, rating, groups, result.sku, result.rating, result.groups`
   - Ответ: `RatingResponse { products: ProductRating[] }`

## Типовой сценарий: получить описания для 10 товаров с минимальным трафиком

Задача: по возможности вернуть только нужные поля на каждом шаге.

### Шаг 1. Получить список товаров, вернуть только `product_id`

HTTP-запрос:
```
GET /llm/products?limit=10&fields=product_id
```
Пример PowerShell:
```powershell
Invoke-RestMethod -Uri 'http://localhost:5000/llm/products?limit=10&fields=product_id' -Method Get
```
Пример ответа:
```json
[
  { "product_id": 12345 },
  { "product_id": 67890 }
]
```

### Шаг 2. Получить подробную информацию по списку товаров, вернуть только `sku`

HTTP-запрос:
```
POST /llm/products/info?fields=items.sku
Content-Type: application/json

{ "product_id": [12345, 67890, ...] }
```
Пример PowerShell:
```powershell
$body = @{ product_id = @(12345, 67890) } | ConvertTo-Json
Invoke-RestMethod -Uri 'http://localhost:5000/llm/products/info?fields=items.sku' -Method Post -Body $body -ContentType 'application/json'
```
Пример ответа:
```json
{
  "items": [
    { "sku": 2000000015156 },
    { "sku": 2000000015157 }
  ]
}
```

### Шаг 3. По списку SKU получить описания, вернуть только `sku` и `description`

Данный шаг делается по каждому SKU (можно параллелить на стороне клиента).

HTTP-запрос:
```
POST /llm/product/description?fields=sku,description
Content-Type: application/json

{ "sku": "2000000015156" }
```
Пример PowerShell:
```powershell
$skuList = @("2000000015156", "2000000015157")
$results = foreach ($sku in $skuList) {
  $body = @{ sku = $sku } | ConvertTo-Json
  Invoke-RestMethod -Uri 'http://localhost:5000/llm/product/description?fields=sku,description' -Method Post -Body $body -ContentType 'application/json'
}
$results
```
Пример ответа для одного элемента:
```json
{ "sku": "2000000015156", "description": "Трёхколодочное сцепление..." }
```

## Ещё примеры комбинаций

- Получить названия и SKU по `product_id`:
  - `POST /llm/products/info?fields=items.sku,items.name`
- Получить только `offer_id` и `name` из полного списка товаров:
  - `GET /llm/products?fields=offer_id,name` (если поле доступно в `ProductListItem`; иначе используйте `/llm/products/info`)
- Получить рейтинги только с `sku` и `rating`:
  - `POST /llm/ratings?fields=result.sku,result.rating`

## Обработка ошибок

- 400 Bad Request: неправильный вход (например, отсутствует обязательный параметр).
- 500 Internal Server Error: внутренняя ошибка сервера / внешнего API.
- Формат ошибки: `{ "message": string, "details"?: string }`

## Замечания по `fields`

- Сервер использует белые списки полей на эндпоинт, чтобы исключить опечатки и неожиданные ключи.
- Допустимы префиксы (`items.sku`) и плоские ключи (`sku`).
- Если `fields` пуст — вернётся полный объект без проекции.

## Производительность и трафик

- Используйте `fields` на каждом шаге, чтобы сократить объём ответов.
- Объединяйте запросы по разумным батчам (например, 10–50 `product_id`/`sku` в одном запросе), чтобы не упираться в сетевые накладные расходы и лимиты внешних API.

---
Если потребуется, спецификацию можно выгружать и сохранять локально (файл `/openapi_llmopenapi.json` доступен из `wwwroot`) для передачи в инструменты генерации кода или агентов GPT.
