// filepath: c:\Users\pycek\source\repos\ai_it_wiki\Controllers\ApiController.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace it_wiki_site.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ApiController : ControllerBase
    {
        private readonly ILogger<ApiController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;

        public ApiController(
            ILogger<ApiController> logger,
            IConfiguration configuration,
            IWebHostEnvironment env
        )
        {
            _logger = logger;
            _configuration = configuration;
            _env = env;
        }

        /// <summary>
        /// Проверка работоспособности API.
        /// </summary>
        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult Index()
        {
            return Ok(new { IsSuccess = true, Message = "API is running." });
        }

    /// <summary>
    /// Подтверждение VK Callback API и логирование входящих объектов.
    /// </summary>
    /// <param name="ct">Токен отмены.</param>
    /// <param name="root">JSON-объект события VK.</param>
    /// <returns>Результат подтверждения VK.</returns>
    [HttpPost("vk/callback")]
    public IActionResult Confirm(
            CancellationToken ct,
            [FromBody] System.Text.Json.JsonElement root = default
        )
        {
            // Логирование входящих данных
            try
            {
                Directory.CreateDirectory("logs");
                System.IO.File.WriteAllText(
                    $"logs/vk_confirm_{DateTime.Now:yyyyMMdd_HH_mm_ss_fff}.json",
                    root.GetRawText()
                );
                _logger.LogInformation("VK confirm input logged.");
            }
            catch (Exception logEx)
            {
                _logger.LogError(logEx, "Failed to log VK confirm input.");
            }

            // Проверка типа события
            if (!root.TryGetProperty("type", out var typeEl))
            {
                _logger.LogWarning("Missing VK event type.");
                return Content("ok", "text/plain");
            }
            var type = typeEl.GetString();

            // Подтверждение сервера
            if (type == "confirmation")
            {
                var confirmationCode = _configuration["Vk:Callback:ConfirmationCode"];
                return Content(confirmationCode ?? "no_code", "text/plain");
            }

            // Для других типов просто возвращаем "ok"
            return Content("ok", "text/plain");
        }
    }
}
