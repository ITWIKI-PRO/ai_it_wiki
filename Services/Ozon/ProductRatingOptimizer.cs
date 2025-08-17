using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace ai_it_wiki.Services.Ozon
{
    public class ProductRatingOptimizer
    {
        private readonly IOzonClient _client;
        private readonly HashSet<long> _optimizedSkus;
        private const string StateFile = "Data/optimized_skus.json";

        public ProductRatingOptimizer(IOzonClient client)
        {
            _client = client;
            _optimizedSkus = LoadState();
        }

        public async Task OptimizeSkuAsync(long sku)
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
                await Task.Delay(1000);
                rating = await _client.GetRatingAsync(sku);
                attempts++;
            }

            if (rating >= 100)
            {
                _optimizedSkus.Add(sku);
                SaveState();
            }
        }

        private HashSet<long> LoadState()
        {
            if (!File.Exists(StateFile))
            {
                return new HashSet<long>();
            }

            var json = File.ReadAllText(StateFile);
            return JsonSerializer.Deserialize<HashSet<long>>(json) ?? new HashSet<long>();
        }

        private void SaveState()
        {
            var json = JsonSerializer.Serialize(_optimizedSkus);
            File.WriteAllText(StateFile, json);
        }
    }
}
