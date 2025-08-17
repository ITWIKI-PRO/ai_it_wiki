using System;
using System.Threading.Tasks;

namespace ai_it_wiki.Services.Ozon
{
    public class OzonClientStub : IOzonClient
    {
        public Task<int> GetRatingAsync(long sku)
        {
            // TODO: запрос к Ozon API для получения рейтинга
            return Task.FromResult(0);
        }

        public Task UpdateCardAsync(long sku)
        {
            // TODO: обновление карточки товара через Ozon API
            return Task.CompletedTask;
        }
    }
}
