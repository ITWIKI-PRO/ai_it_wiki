# Обработка SKU

1. Получите учётные данные Ozon API (ClientId и ApiKey) и сохраните их в конфигурации приложения.
2. В `Program.cs` зарегистрируйте `OzonApiService`:
   ```csharp
   builder.Services.AddHttpClient<OzonApiService>(c =>
   {
     c.BaseAddress = new Uri("https://api.ozon.ru");
   });
   ```
3. Скомпилируйте и запустите приложение.
4. Отправьте запрос на нужный эндпойнт сервиса Ozon для обработки SKU:
   ```
   GET /sku/{id}
   ```
   где `id` — идентификатор SKU.
5. Полученный ответ содержит данные по SKU и может быть передан далее в бизнес‑логику.
