using ai_it_wiki.Models;

using NAudio.Wave;

using Newtonsoft.Json.Linq;

using System.Diagnostics;
using System.Text.RegularExpressions;

using Vosk;
namespace ai_it_wiki.Services.VoskTranscription
{
  public class VoskTranscriptionService
  {
    private readonly string _modelPath = "Models/vosk-model-small-ru-0.22";

    public TranscriptResult Transcribe(string filePath)
    {
      try
      {
        Vosk.Vosk.SetLogLevel(0);
        var model = new Model(_modelPath);

        using var waveReader = new WaveFileReader(filePath);
        using var resampler = new MediaFoundationResampler(waveReader, new WaveFormat(16000, 1));
        using var memoryStream = new MemoryStream();
        WaveFileWriter.WriteWavFileToStream(memoryStream, resampler);
        memoryStream.Position = 0;

        using var decoder = new WaveFileReader(memoryStream);
        var recognizer = new VoskRecognizer(model, 16000.0f);

        var buffer = new byte[4096];
        string resultText = "";

        int bytesRead;
        while ((bytesRead = decoder.Read(buffer, 0, buffer.Length)) > 0)
        {
          if (recognizer.AcceptWaveform(buffer, bytesRead))
          {
            var text = JObject.Parse(recognizer.Result())["text"]?.ToString();
            if (!string.IsNullOrWhiteSpace(text))
              resultText += text + " ";
          }
        }

        resultText += JObject.Parse(recognizer.FinalResult())["text"]?.ToString();

        return new TranscriptResult { Text = resultText.Trim(), Success = true };
      }
      catch (Exception ex)
      {
        return new TranscriptResult { Success = false, Error = ex.Message };
      }
    }

    public async Task<TranscriptResult> TranscribeAsync(Stream audioStream)
    {
      Vosk.Vosk.SetLogLevel(0);
      var model = new Model("models/vosk-model-small-ru-0.22");

      using var recognizer = new VoskRecognizer(model, 16000.0f);
      var buffer = new byte[8192]; // Увеличенный буфер для скорости
      string finalResult = "";

      int bytesRead;
      while ((bytesRead = await audioStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
      {
        bool isFinal = await Task.Run(() => recognizer.AcceptWaveform(buffer, bytesRead));

        if (isFinal)
        {
          var result = JObject.Parse(recognizer.Result())["text"]?.ToString();
          if (!string.IsNullOrWhiteSpace(result))
            finalResult += result + " ";
        }
      }

      finalResult += JObject.Parse(recognizer.FinalResult())["text"]?.ToString();
      return new TranscriptResult { Text = finalResult.Trim(), Success = true };
    }

    public unsafe TranscriptResult TranscribeUnsafe(Stream audioStream)
    {
      Vosk.Vosk.SetLogLevel(0);
      var model = new Model("models/vosk-model-small-ru-0.22");

      using var recognizer = new VoskRecognizer(model, 16000.0f);
      int bufferSize = 8192;

      // Буфер выделяется в стеке (без сборщика мусора)
      byte* buffer = stackalloc byte[bufferSize];

      string finalResult = "";

      int bytesRead;
      while ((bytesRead = audioStream.Read(new Span<byte>(buffer, bufferSize))) > 0)
      {
        bool isFinal = recognizer.AcceptWaveform(new Span<byte>(buffer, bytesRead).ToArray(), bytesRead);

        if (isFinal)
        {
          var result = JObject.Parse(recognizer.Result())["text"]?.ToString();
          if (!string.IsNullOrWhiteSpace(result))
            finalResult += result + " ";
        }
      }

      finalResult += JObject.Parse(recognizer.FinalResult())["text"]?.ToString();
      return new TranscriptResult { Text = finalResult.Trim(), Success = true };
    }

    public TranscriptResult Transcribe(Stream audioStream)
    {
      Vosk.Vosk.SetLogLevel(0);
      var model = new Model("models/vosk-model-small-ru-0.22");

      using var recognizer = new VoskRecognizer(model, 16000.0f);
      var buffer = new byte[4096];
      string finalResult = "";

      int bytesRead;
      while ((bytesRead = audioStream.Read(buffer, 0, buffer.Length)) > 0)
      {
        if (recognizer.AcceptWaveform(buffer, bytesRead))
        {
          var result = JObject.Parse(recognizer.Result())["text"]?.ToString();
          if (!string.IsNullOrWhiteSpace(result))
            finalResult += result + " ";
        }
      }

      finalResult += JObject.Parse(recognizer.FinalResult())["text"]?.ToString();
      return new TranscriptResult { Text = finalResult.Trim(), Success = true };
    }

    public Stream ConvertToWavStream(string inputFile)
    {
      string ffmpegPath = Path.Combine(Directory.GetCurrentDirectory(), "ffmpeg", "bin", "ffmpeg.exe");

      if (!File.Exists(ffmpegPath))
        throw new Exception($"FFmpeg не найден по пути: {ffmpegPath}");

      var psi = new ProcessStartInfo
      {
        FileName = ffmpegPath,
        Arguments = $"-i \"{inputFile}\" -ar 16000 -ac 1 -f wav pipe:1",
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true
      };

      var process = Process.Start(psi);
      return process.StandardOutput.BaseStream; // Отдаём поток прямо в Vosk
    }


    public string PostProcessText(string rawText)
    {
      if (string.IsNullOrWhiteSpace(rawText))
        return rawText;

      rawText = rawText.Trim();

      // 1️⃣ Заменяем множественные пробелы на один
      rawText = Regex.Replace(rawText, @"\s+", " ");

      // 2️⃣ Добавляем заглавные буквы в начало предложений
      rawText = Regex.Replace(rawText, @"(^|[.!?]\s+)([а-яё])", m => m.Groups[1].Value + char.ToUpper(m.Groups[2].Value[0]));

      // 3️⃣ Умное добавление точек (если в конце нет знака)
      if (!Regex.IsMatch(rawText, @"[.!?]$"))
        rawText += ".";

      // 4️⃣ Удаление дублированных слов (например: "привет привет как дела?")
      rawText = Regex.Replace(rawText, @"\b(\w+)\s+\1\b", "$1");

      // 5️⃣ Форматирование чисел (12 000 -> 12,000)
      rawText = Regex.Replace(rawText, @"\b(\d{1,3})\s(\d{3})\b", "$1,$2");

      return rawText;
    }

    public JArray GetSpeakers(string audioFilePath)
    {
      string basePath = Directory.GetCurrentDirectory(); // Получает корневую папку проекта
      string scriptPath = Path.Combine(basePath, "Services", "VoskTranscription", "diarization.py");
      
      var psi = new ProcessStartInfo
      {
        FileName = @"C:\Users\pycek\AppData\Local\Programs\Python\Python313\python.exe",
        Arguments = $"\"{scriptPath}\" \"{audioFilePath}\"",
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true
      };

      using var process = new Process { StartInfo = psi };
      process.Start();

      string output = process.StandardOutput.ReadToEnd();
      string error = process.StandardError.ReadToEnd();
      process.WaitForExit();

      if (!string.IsNullOrEmpty(error))
        throw new Exception($"Ошибка Python: {error}");

      return JArray.Parse(output);
    }

    public string ConvertToWav(string inputFile)
    {
      string ffmpegPath = Path.Combine(Directory.GetCurrentDirectory(), "ffmpeg", "bin", "ffmpeg.exe");
      string outputFile = Path.ChangeExtension(Path.GetTempFileName(), ".wav");

      if (!File.Exists(ffmpegPath))
        throw new Exception($"FFmpeg не найден по пути: {ffmpegPath}");

      var psi = new ProcessStartInfo
      {
        FileName = ffmpegPath, // Используем локальный ffmpeg.exe
        Arguments = $"-y -i \"{inputFile}\" -ar 16000 -ac 1 -f wav \"{outputFile}\"",
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true
      };

      using var process = Process.Start(psi);
      process.WaitForExit();

      if (!File.Exists(outputFile))
        throw new Exception("Ошибка конвертации файла через FFmpeg.");

      return outputFile;
    }


    public void TranscribeWithTimestamps(string filePath) { /* TODO */ }
    public void ExportToSrt(string transcript, string outputPath) { /* TODO */ }
    public void BatchTranscribe(string folderPath) { /* TODO */ }
  }
}
