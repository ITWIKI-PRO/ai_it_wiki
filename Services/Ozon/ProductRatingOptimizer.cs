using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ai_it_wiki.Options;

namespace ai_it_wiki.Services.Ozon
{
  public class ProductRatingOptimizer
  {
    private readonly IOzonClient _client;
    private readonly ILogger<ProductRatingOptimizer> _logger;
    private readonly HashSet<string> _optimizedSkus;
    private readonly string _stateFile;
    private readonly int _maxAttempts;
    private readonly int _delayMilliseconds;

    public ProductRatingOptimizer(
      IOzonClient client,
      IOptions<OzonOptions> options,
      ILogger<ProductRatingOptimizer> logger,
      string stateFile = "Data/optimized_skus.json")
    {
      _client = client;
      _logger = logger;
      _stateFile = stateFile;
      _maxAttempts = options.Value.MaxAttempts;
      _delayMilliseconds = options.Value.DelayMilliseconds;
      _optimizedSkus = LoadState();
    }

    public async Task OptimizeSkuAsync(string sku)
    {
      if (_optimizedSkus.Contains(sku))
      {
        _logger.LogInformation("SKU {Sku} уже оптимизирован, пропуск.", sku);
        return;
      }

      var rating = await _client.GetRatingAsync(sku);
      if (rating >= 100)
      {
        _logger.LogInformation("Рейтинг SKU {Sku} уже {Rating}, оптимизация не требуется.", sku, rating);
        _optimizedSkus.Add(sku);
        SaveState();
        return;
      }

      await _client.UpdateCardAsync(sku);

      var attempts = 0;
      while (rating < 100 && attempts < _maxAttempts)
      {
        await Task.Delay(_delayMilliseconds);
        rating = await _client.GetRatingAsync(sku);
        attempts++;
      }

      if (rating >= 100)
      {
        _optimizedSkus.Add(sku);
        SaveState();
      }
      else
      {
        _logger.LogWarning("Не удалось оптимизировать SKU {Sku} после {Attempts} попыток.", sku, attempts);
      }
    }

    private HashSet<string> LoadState()
    {
      if (!File.Exists(_stateFile))
      {
        return new HashSet<string>();
      }

      try
      {
        var json = File.ReadAllText(_stateFile);
        return JsonSerializer.Deserialize<HashSet<string>>(json) ?? new HashSet<string>();
      }
      catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
      {
        _logger.LogError(ex, "Ошибка при загрузке состояния из файла {StateFile}.", _stateFile);
        return new HashSet<string>();
      }
    }

    private void SaveState()
    {
      try
      {
        var directory = Path.GetDirectoryName(_stateFile);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
          Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(_optimizedSkus);
        File.WriteAllText(_stateFile, json);
      }
      catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
      {
        _logger.LogError(ex, "Ошибка при сохранении состояния в файл {StateFile}.", _stateFile);
      }
    }
  }
}
