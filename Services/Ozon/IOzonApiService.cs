using System.Threading;
using System.Threading.Tasks;
using ai_it_wiki.Models.Ozon;

namespace ai_it_wiki.Services.Ozon
{
    public interface IOzonApiService
    {
        /// <summary>
        /// Получить контент-рейтинг по SKU
        /// </summary>
        Task<int> GetContentRatingAsync(string sku, CancellationToken cancellationToken = default);

        /// <summary>
        /// Получить информацию о товаре
        /// </summary>
        Task<string> GetProductInfoAsync(string sku, CancellationToken cancellationToken = default);

        /// <summary>
        /// Получить текст описания товара
        /// </summary>
        Task<string> GetProductDescriptionAsync(
            string sku,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Отправить обновление карточки товара
        /// </summary>
        /// <returns>Идентификатор задания импорта</returns>
        Task<string> ImportProductAsync(
            string sku,
            string improvedContent,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Дождаться завершения импорта
        /// </summary>
        Task WaitForImportAsync(string taskId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Обновить карточку товара
        /// </summary>
        Task UpdateCardAsync(string sku, CancellationToken cancellationToken = default);

        // TODO[moderate]: Реализовать взаимодействие с Ozon API

        /// <summary>
        /// Получить список товаров (объекты Ozon API) с поддержкой пагинации и фильтров.
        /// </summary>
        Task<ProductInfoListResponse> GetProductInfoListAsync(
            ProductInfoListRequest request,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Получить детальный рейтинг контента для набора SKU из Ozon API.
        /// </summary>
        Task<RatingBySkuResponse> GetRatingBySkusAsync(
            IEnumerable<long> skus,
            CancellationToken cancellationToken = default
        );
    }
}
