using System.Threading;
using System.Threading.Tasks;

using ai_it_wiki.Models.Ozon;

namespace ai_it_wiki.Services.Ozon
{
  public interface IOzonApiService
  {
    Task<int> GetContentRatingAsync(string sku, CancellationToken cancellationToken = default);
    Task<string> GetProductInfoAsync(string sku, CancellationToken cancellationToken = default);
    Task<string> GetProductDescriptionAsync(string sku, CancellationToken cancellationToken = default);
    Task<string> ImportProductAsync(string sku, string improvedContent, CancellationToken cancellationToken = default);
    Task WaitForImportAsync(string taskId, CancellationToken cancellationToken = default);

    // Add the missing method definition to resolve CS1061  
    Task<ProductInfoListResponse> GetProductInfoListAsync(ProductInfoListRequest productInfoListRequest, CancellationToken cancellationToken = default);
    Task<RatingBySkuResponse> GetRatingBySkusAsync(IEnumerable<long> skus, CancellationToken cancellationToken);
    Task<List<ProductListItem>> GetProductsAsync(ProductListRequest request, CancellationToken cancellationToken = default);

    //TODO[critical]: исправить методы получения информации о продуктах и самого списка продуктов
  }
}
