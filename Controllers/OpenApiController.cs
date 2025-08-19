using System.Net;
using System.Net.Http;
using System.Net.Mail;
using ai_it_wiki.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

/// <summary> Контроллер для реализации OpenAPI OpenAI GPT-4 </summary>
namespace ai_it_wiki.Controllers
{
    [Route("/api")]
    [EnableCors("AllowAllOrigins")] // Добавляем атрибут EnableCors
    //игнорируем сваггером
    [ApiExplorerSettings(IgnoreApi = true)]
    public class OpenApiController : OpenApiControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly SmtpClient _smtpClient;

        public OpenApiController()
        {
            _httpClient = new HttpClient();
            _smtpClient = new SmtpClient("mail.it-wiki.site", 25);
            _smtpClient.UseDefaultCredentials = false;
            _smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            _smtpClient.Credentials = new NetworkCredential("altron@it-wiki.site", "333Pycek9393");
            _smtpClient.EnableSsl = false;
        }

        //метод открытия url в браузере с использованием Selenium
        [HttpGet("OpenUrl")]
        public IActionResult OpenUrl(
            [FromQuery] string url,
            int timeOut = 10,
            int part = 1,
            ContentType contentType = ContentType.Text
        )
        {
            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
            {
                url = "http://" + url;
            }

            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArgument("--headless");
            chromeOptions.AddArgument("--disable-gpu");
            chromeOptions.AddArgument("--no-sandbox");
            chromeOptions.AddArgument("--disable-dev-shm-usage");
            chromeOptions.AddArgument("--disable-extensions");
            chromeOptions.AddArgument("--disable-popup-blocking");
            chromeOptions.AddArgument("--disable-logging");
            chromeOptions.AddArgument("--disable-notifications");
            chromeOptions.AddArgument("--disable-infobars");
            chromeOptions.AddArgument("--disable-extensions-file-access-check");
            chromeOptions.AddArgument("--disable-extensions-http-throttling");
            chromeOptions.AddArgument("--disable-default-apps");
            chromeOptions.AddArgument("--disable-sync");
            chromeOptions.AddArgument("--disable-background-networking");
            chromeOptions.AddArgument("--disable-component-extensions-with-background-pages");
            chromeOptions.AddArgument("--disable-breakpad");
            chromeOptions.AddArgument("--disable-client-side-phishing-detection");
            chromeOptions.AddArgument("--disable-cloud-import");
            chromeOptions.AddArgument("--disable-features=TranslateUI");
            chromeOptions.AddArgument("--disable-hang-monitor");
            chromeOptions.AddArgument("--disable-ipc-flooding-protection");
            chromeOptions.AddArgument("--disable-prompt-on-repost");
            chromeOptions.AddArgument("--disable-renderer-backgrounding");
            chromeOptions.AddArgument("--disable-translate");
            chromeOptions.AddArgument("--disable-web-resources");
            chromeOptions.AddArgument("--disable-component-update");
            chromeOptions.AddArgument("--disable-domain-reliability");
            chromeOptions.AddArgument("--disable-resource-fetching");
            chromeOptions.AddArgument("--disable-web-security");
            chromeOptions.AddArgument("--enable-automation");

            using (var driver = new ChromeDriver(chromeOptions))
            {
                driver.Navigate().GoToUrl(url);
                try
                {
                    WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeOut));
                    wait.Until(driver =>
                    {
                        try
                        {
                            // Проверяем, что document.readyState равен 'complete' и все асинхронные операции завершены
                            bool isPageLoaded = ((IJavaScriptExecutor)driver)
                                .ExecuteScript("return document.readyState")
                                .Equals("complete");
                            bool areScriptsCompleted = (bool)
                                ((IJavaScriptExecutor)driver).ExecuteScript(
                                    @"
                        return (typeof window.fetch === 'undefined' || window.fetch.length === 0) &&
                               (typeof window.XMLHttpRequest === 'undefined' || window.XMLHttpRequest.length === 0) &&
                               (typeof window.Promise === 'undefined' || window.Promise.length === 0) &&
                               (typeof window.requestAnimationFrame === 'undefined' || window.requestAnimationFrame.length === 0);
                    "
                                );
                            return isPageLoaded && areScriptsCompleted;
                        }
                        catch (Exception)
                        {
                            // Игнорируем ошибки JavaScript, продолжаем проверку
                            return false;
                        }
                    });

                    Console.WriteLine("Страница и все скрипты загружены.");
                }
                catch (WebDriverTimeoutException)
                {
                    Console.WriteLine($"Превышено время ожидания в {timeOut} секунд.");
                }

                if (contentType == ContentType.Html)
                {
                    var html = driver.PageSource;
                    driver.Quit();
                    return SplitedResponse(html, part);
                }
                else
                {
                    var text = driver.FindElement(By.TagName("body")).Text;
                    driver.Quit();
                    return SplitedResponse(text, part);
                }
            }
        }

        [HttpGet("Debug")]
        public IActionResult Debug()
        {
            var data = SampleData.Orders;
            return Ok(data);
        }

        [HttpGet("ResponseLengthTest")]
        public IActionResult ResponseLengthTest([FromQuery] int part = 1)
        {
            var text = System.IO.File.ReadAllText(
                Path.Combine("wwwroot", "samples", "5716.txt"),
                System.Text.Encoding.UTF8
            );
            return SplitedResponse(text, part);
        }

        [HttpPost("execute")]
        public async Task<IActionResult> ExecuteRequest(
            [FromBody] HttpRequestModel requestModel,
            [FromQuery] int part = 1
        )
        {
            // Создание HTTP запроса в зависимости от типа метода
            var requestMessage = new HttpRequestMessage(
                new HttpMethod(requestModel.Method),
                requestModel.Url
            );

            // Добавление тела запроса, если есть
            if (!string.IsNullOrEmpty(requestModel.Body))
            {
                requestMessage.Content = new StringContent(
                    requestModel.Body,
                    System.Text.Encoding.UTF8,
                    "application/json"
                );
            }

            // Добавление заголовков, если есть
            if (requestModel.Headers != null)
            {
                foreach (var header in requestModel.Headers)
                {
                    requestMessage.Headers.Add(header.Key, header.Value);
                }
            }

            try
            {
                // Отправка запроса
                var response = await _httpClient.SendAsync(requestMessage);

                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    return Ok(new { response.StatusCode, Response = "" });

                var responseBody = await response.Content.ReadAsStringAsync();

                var Response = SplitedResponse(responseBody, part);

                // Возвращаем результат
                return Ok(Response);
            }
            catch (HttpRequestException ex)
            {
                // Обработка ошибок
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        /// <summary>
        ///   {
        //  "openapi": "3.0.0",
        //  "info": {
        //    "title": "API Documentation",
        //    "version": "1.0.0",
        //    "description": "This is the documentation for the API."
        //  },
        //  "paths": {
        //    "/api/MailTo": {
        //      "post": {
        //        "summary": "Send an email",
        //        "requestBody": {
        //          "required": true,
        //          "content": {
        //            "application/json": {
        //              "schema": {
        //                "$ref": "#/components/schemas/MailToObject"
        //              }
        //            }
        //          }
        //        },
        //        "responses": {
        //          "200": {
        //            "description": "Success",
        //            "content": {
        //              "application/json": {
        //                "schema": {
        //                  "$ref": "#/components/schemas/ITWContentResult"
        //                }
        //              }
        //            }
        //          }
        //        }
        //      }
        //    }
        //  },
        //  "components": {
        //    "schemas": {
        //      "MailToObject": {
        //        "type": "object",
        //        "properties": {
        //          "To": {
        //            "type": "string"
        //          },
        //          "Title": {
        //            "type": "string"
        //          },
        //          "Body": {
        //            "type": "string"
        //          },
        //          "Attachments": {
        //            "type": "array",
        //            "items": {
        //              "type": "string"
        //            }
        //          }
        //        },
        //        "required": ["To", "Body"]
        //  },
        //      "ITWContentResult": {
        //        "type": "object",
        //        "properties": {
        //          "ContentType": {
        //            "type": "string"
        //          },
        //          "StatusCode": {
        //  "type": "integer"
        //          },
        //          "IsSuccess": {
        //  "type": "boolean"
        //          },
        //          "Content": {
        //  "type": "string"
        //          }
        //        },
        //        "required": ["ContentType", "StatusCode", "IsSuccess", "Content"]
        //      }
        //    }
        //  }
        //}
        /// </summary>
        /// <param name="mailToObject"></param>
        /// <returns></returns>
        [HttpPost("MailTo")]
        public async Task<IActionResult> MailTo([FromBody] MailToObject mailToObject)
        {
            ITWContentResult result = new() { ContentType = "application/json" };

            if (string.IsNullOrEmpty(mailToObject.To) || string.IsNullOrEmpty(mailToObject.Body))
            {
                result.StatusCode = 200;
                result.Content =
                    "Один или несколько обязательных параметров были пустыми, проверьте входные значения.";
                return result;
            }
            result.IsSuccess = false;
            MailAddress from = new MailAddress("altron@it-wiki.pro", "Альтрон");
            MailAddress to = new MailAddress(mailToObject.To);
            MailMessage message = new MailMessage(from, to);
            message.Subject = mailToObject.Title;
            message.Body = mailToObject.Body;
            try
            {
                await _smtpClient.SendMailAsync(message);
                result.IsSuccess = true;
                result.StatusCode = 200;
                result.Content = "Mail sent successfully";
            }
            catch (Exception exc)
            {
                result.IsSuccess = false;
                result.StatusCode = 500;
                result.Content = exc.ToString();
            }
            return result;
        }
    }

    internal class ITWContentResult : ContentResult
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class MailToObject
    {
        public string To { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public List<string> Attachments { get; set; } = new List<string>();
    }

    public class HttpRequestModel
    {
        public string Method { get; set; } // GET, POST, PUT, DELETE и т.д.
        public string Url { get; set; }
        public string Body { get; set; } // Для POST или PUT запросов
        public Dictionary<string, string> Headers { get; set; } // Дополнительные заголовки
    }

    public enum ContentType
    {
        Text,
        Html,
    }
}
