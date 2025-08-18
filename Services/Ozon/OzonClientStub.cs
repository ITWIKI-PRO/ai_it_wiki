using System;
using System.Threading.Tasks;

namespace ai_it_wiki.Services.Ozon
{
    public class OzonClientStub : IOzonClient
    {
        public Task<int> GetRatingAsync(string sku)
        {
            // TODO[critical]: реализовать запрос к Ozon API для получения рейтинга
            return Task.FromResult(0);
        }

        public Task UpdateCardAsync(string sku)
        {
            // TODO[critical]: реализовать обновление карточки товара через Ozon API
            return Task.CompletedTask;
        }
    }
}
