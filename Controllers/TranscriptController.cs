using Microsoft.AspNetCore.Mvc;
using ai_it_wiki.Services.VoskTranscription;

namespace ai_it_wiki.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  [ApiExplorerSettings(IgnoreApi = true)]
  public class TranscriptController : ControllerBase
  {
    private readonly VoskTranscriptionService _transcriber;

    public TranscriptController()
    {
      _transcriber = new VoskTranscriptionService();
    }

    [DisableRequestSizeLimit]
    [RequestFormLimits(MultipartBodyLengthLimit = 100000000)] // 100MB
    [HttpPost("upload")]
    public async Task<IActionResult> Upload([FromForm] IFormFile audioFile)
    {
      if (audioFile == null || audioFile.Length == 0)
        return BadRequest("Файл пустой.");

      var tempFilePath = Path.GetTempFileName();

      using (var stream = new FileStream(tempFilePath, FileMode.Create))
      {
        audioFile.CopyTo(stream);
      }

      string finalFilePath = tempFilePath;
      Stream? finalStream = null;

      // Если это не WAV — конвертируем
      if (!audioFile.ContentType.Contains("wav"))
      {
        try
        {
          finalStream = _transcriber.ConvertToWavStream(tempFilePath);

          //сохраняем конвертированный файл
          finalFilePath = Path.GetTempFileName();
          //меняем расширение .tmp на .wav
          finalFilePath = finalFilePath.Replace(".tmp", ".wav");
          using (var finalFileStream = new FileStream(finalFilePath, FileMode.Create))
          {
            finalStream.CopyTo(finalFileStream);
          }

          Console.WriteLine($"Файл {audioFile.FileName} успешно конвертирован в {finalFilePath}");
        }
        catch (Exception ex)
        {
          return StatusCode(500, new { error = "Ошибка конвертации: " + ex.Message });
        }
      }

      try
      {
        var speakers = _transcriber.GetSpeakers(finalFilePath);
        var result = await _transcriber.TranscribeAsync(finalStream);
        result.Text = _transcriber.PostProcessText(result.Text);
        result.Speakers = speakers;
        // Удаляем временные файлы
        System.IO.File.Delete(tempFilePath);
        if (finalFilePath != tempFilePath) System.IO.File.Delete(finalFilePath);

        if (result.Success)
          return Ok(result);
        else
          return StatusCode(500, result);
      }
      catch (Exception)
      {
        throw;
      }
    }

  }
}
