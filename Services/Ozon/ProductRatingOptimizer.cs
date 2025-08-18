using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace ai_it_wiki.Services.Ozon
{
  public class ProductRatingOptimizer
  {
    private readonly IOzonClient _client;
    private readonly HashSet<string> _optimizedSkus;
    private const string StateFile = "Data/optimized_skus.json";
    private const int DelayMilliseconds = 1000; // Added missing constant  

    private readonly string _stateFile;

    public ProductRatingOptimizer(IOzonClient client, string stateFile = "Data/optimized_skus.json")
    {
      _client = client;
      _stateFile = stateFile;
      _optimizedSkus = LoadState();
    }

    public async Task OptimizeSkuAsync(string sku)
    {
      if (_optimizedSkus.Contains(sku))
      {
        return;
      }

      var rating = await _client.GetRatingAsync(sku);
      if (rating >= 100)
      {
        // TODO[recommended]: логгировать пропуск
        _optimizedSkus.Add(sku);
        SaveState();
        return;
      }

      await _client.UpdateCardAsync(sku);

      const int MaxAttempts = 5; // TODO[moderate]: настроить лимит через конфигурацию  
      var attempts = 0;
      while (rating < 100 && attempts < MaxAttempts)
      {
        await Task.Delay(DelayMilliseconds);
        rating = await _client.GetRatingAsync(sku);
        attempts++;
      }

      if (rating >= 100)
      {
        _optimizedSkus.Add(sku);
        SaveState();
      }
    }

    private HashSet<string> LoadState()
    {
      if (!File.Exists(StateFile))
      {
        return new HashSet<string>();
      }

      try
      {
        var json = File.ReadAllText(StateFile);
        return JsonSerializer.Deserialize<HashSet<string>>(json) ?? new HashSet<string>();
      }
      catch (IOException)
      {
        // Optionally log the error
        return new HashSet<string>();
      }
      catch (UnauthorizedAccessException)
      {
        // Optionally log the error
        return new HashSet<string>();
      }
    }

    private void SaveState()
    {
      var json = JsonSerializer.Serialize(_optimizedSkus);
      try
      {
        File.WriteAllText(StateFile, json);
      }
      catch (IOException ex)
      {
        // TODO: log the exception or handle it as needed  
      }
      catch (UnauthorizedAccessException ex)
      {
        // TODO: log the exception or handle it as needed  
      }
    }
  }
}
