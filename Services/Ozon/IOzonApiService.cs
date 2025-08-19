using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ai_it_wiki.Models.Ozon;

namespace ai_it_wiki.Services.Ozon
{
    public interface IOzonApiService
    {
        //Task<double> GetContentRatingAsync(
        //    string sku,
        //    CancellationToken cancellationToken = default
        //);
        //Task<string> GetProductInfoAsync(string sku, CancellationToken cancellationToken = default);
        Task<string> GetProductDescriptionAsync(
            string sku,
            CancellationToken cancellationToken = default
        );
        Task<string> ImportProductAsync(
            string sku,
            string improvedContent,
            CancellationToken cancellationToken = default
        );
        Task WaitForImportAsync(string taskId, CancellationToken cancellationToken = default);

        // Add the missing method definition to resolve CS1061
        Task<ProductInfoListResponse> GetProductInfoListAsync(
            ProductInfoListRequest productInfoListRequest,
            CancellationToken cancellationToken = default
        );
        Task<RatingResponse> GetRatingBySkusAsync(
            RatingRequest ratingRequest,
            CancellationToken cancellationToken
        );
        Task<List<ProductListItem>> GetProductsAsync(
            ProductListRequest request,
            CancellationToken cancellationToken = default
        );

        Task<ProductAttributesResponse> GetAttributesAsync(
            ProductAttributesRequest request,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Получить описания товаров по их идентификаторам.
        /// </summary>
        /// <param name="productIds">Список идентификаторов товаров.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        Task<Dictionary<long, string?>> GetProductDescriptionsAsync(
            IEnumerable<long> productIds,
            CancellationToken cancellationToken = default
        );

        //TODO[critical]: исправить методы получения информации о продуктах и самого списка продуктов
    }
}
