using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using ai_it_wiki.Data;
using ai_it_wiki.Filters;
using ai_it_wiki.Internal;
using ai_it_wiki.Models;
using ai_it_wiki.Options;
using ai_it_wiki.Services.OpenAI;
using ai_it_wiki.Services.Ozon;
using ai_it_wiki.Services.Ozon;
using ai_it_wiki.Services.TelegramBot;
using ai_it_wiki.Services.Youtube;
using Kwork;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerGen;
using Telegram.Bot;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddConsole();

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

OpenApiInfo openApiInfo = new OpenApiInfo
{
    Title = "Altron OpenAPI",
    Version = "v1",
    Description = "Документация к REST-API сервиса автоматизации",
};

// Add services to the container.
builder
    .Services.AddRazorPages()
    .AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = null);

//builder.Services.AddOpenApi("v1", (e) =>
//{
//  e.OpenApiVersion = OpenApiSpecVersion.OpenApi3_0;
//});
var services = new ServiceCollection();

// TODO[recommended]: рассмотреть удаление неиспользуемой коллекции services

var mySqlConnectionString = builder.Configuration.GetConnectionString("context");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(mySqlConnectionString, new MySqlServerVersion(new Version(8, 0, 21)))
);

services.AddDbContext<ApplicationDbContext>();
services.AddTransient<AuthService>();
services.AddTransient<DialogService>();
services.AddTransient<OpenAIService>();
services.AddTransient<YoutubeService>();

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<GlobalExceptionFilter>();
});
builder.Services.AddScoped<GlobalExceptionFilter>();

// ↓ ДОБАВЬТЕ сразу после AddControllersWithViews();  :contentReference[oaicite:0]{index=0}
builder.Services.AddEndpointsApiExplorer();

//var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";

//var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Title = "Altron OpenAPI",
            Version = "v1",
            Description = "Документация коннектора для LLM",
        }
    );
    // Additional Swagger doc containing only endpoints with ApiExplorerSettings(GroupName = "LLMOpenAPI")
    options.SwaggerDoc(
        "LLMOpenAPI",
        new OpenApiInfo
        {
            Title = "LLM OpenAPI",
            Version = "LLM",
            Description = "Спецификация только для LLM/OpenAI интеграции",
        }
    );
    //  options.TagActionsBy(apiDesc => Array.Empty<string>());
    options.CustomOperationIds(apiDesc => apiDesc.ActionDescriptor.RouteValues["action"]);
    options.EnableAnnotations();

    // Подключаем наш DocumentFilter:
    options.DocumentFilter<AddServersDocumentFilter>();

    options.OperationFilter<RemoveTagsOperationFilter>();

    options.DocumentFilter<StripAltronPrefixFilter>();

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (!Directory.Exists(xmlPath))
    {
        Directory.CreateDirectory(Path.GetDirectoryName(xmlPath));
    }
    //if (!File.Exists(xmlFile))
    //{
    //  // Создаем пустой XML-файл, если он не существует
    //  File.Create(xmlPath).Close();
    //  //File.WriteAllText(xmlPath, "<?xml version=\"1.0\" encoding=\"utf-8\"?><root></root>");
    //}

    options.IncludeXmlComments(xmlPath, true);
    options.ExampleFilters();
    // Include endpoints into separate documents depending on their GroupName
    options.DocInclusionPredicate(
        (docName, apiDesc) =>
        {
            var groupName = apiDesc.GroupName;
            if (string.IsNullOrEmpty(groupName))
            {
                // default endpoints go to v1
                return docName == "v1";
            }
            return docName == groupName;
        }
    );
    //options.CustomSchemaIds(type => type.FullName);
});

builder.Services.AddSwaggerExamplesFromAssemblyOf<Program>();

// Export swagger docs to wwwroot on startup for external consumers (e.g., GPT tools)
builder.Services.AddHostedService<ai_it_wiki.Internal.SwaggerExporterHostedService>();

//builder.Services.AddOpen(options =>
//{
//  options.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_1;
//  // здесь же можно задавать Title/Version/Description и прочие свойства
//});

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowAllOrigins",
        builder =>
        {
            builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
        }
    );
});

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 100000000; // 100MB
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 100000000; // 100MB
});

builder.Services.Configure<OzonOptions>(builder.Configuration.GetSection("Ozon"));
builder.Services.Configure<OpenAiOptions>(builder.Configuration.GetSection("OpenAI"));

// Register a simple delegating handler for retries (handles 5xx and 429 Retry-After)
builder.Services.AddTransient<RetryHandler>();
builder
    .Services.AddHttpClient<IOzonApiService, OzonApiService>(client =>
    {
        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json")
        );
    })
    .AddHttpMessageHandler<RetryHandler>();
builder.Services.AddSingleton<OpenAIService>();
builder.Services.AddSingleton<IOpenAiService>(sp => sp.GetRequiredService<OpenAIService>());
builder.Services.AddSingleton<KworkManager>();

builder.Services.AddSingleton<YoutubeService>();
builder.Services.AddScoped<TelegramBotService>(e => new TelegramBotService(
    builder.Configuration["Api:TelegramBot"]
));

var app = builder.Build();

app.UseStaticFiles();

app.UseRouting();

app.UseDeveloperExceptionPage();

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
app.UseDeveloperExceptionPage();

// … после app.UseRouting();, но до MapControllerRoute …
app.UseSwagger();
app.UseSwaggerUI(ui =>
{
    ui.RoutePrefix = "swagger";
    ui.SwaggerEndpoint("/swagger/v1/swagger.json", "Altron OpenAPI");
    ui.SwaggerEndpoint("/swagger/LLMOpenAPI/swagger.json", "LLM OpenAPI");
});

// }
// else
// {
//     app.UseExceptionHandler("/Error");
// }

// Enable CORS
app.UseCors("AllowAllOrigins");

app.UseAuthorization();

app.MapControllers();
app.MapDefaultControllerRoute();

//app.Use(async (context, next) =>
//{
//  // Проверяем, если запрашивается не HTML-страница
//  if (!context.Request.Path.Value.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
//  {
//    context.Response.Headers["Content-Type"] = "text/plain; charset=utf-8";
//  }

//  await next();
//});

// Register routes at the top level
app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}",
    defaults: new { controller = "Home" }
);

app.MapControllerRoute(
    name: "api",
    pattern: "{controller}/{action}/{id?}",
    defaults: new { controller = "OpenApi" }
);

app.MapControllerRoute(
    name: "kwork",
    pattern: "{controller}/{action}/{id?}",
    defaults: new { controller = "KworkApi" }
);

app.MapControllerRoute(
    name: "app",
    pattern: "{controller}/{action}/{id?}",
    defaults: new { controller = "AppApi" }
);

app.MapControllerRoute(
    name: "llm",
    pattern: "{controller}/{action}/{id?}",
    defaults: new { controller = "LLMOpenAPI" }
);

app.MapRazorPages();

// Adds the /openapi/{documentName}.json endpoint to the application

app.Run();
