using System.Threading;
using System.Threading.Tasks;

namespace ai_it_wiki.Services.OpenAI
{
  public interface IOpenAiService
  {
    /// <summary>
    /// Генерирует улучшенный контент карточки товара
    /// </summary>
    /// <param name="productInfo">Информация о товаре</param>
    /// <param name="productDescription">Текущее описание товара</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Строка с улучшенным контентом</returns>
    Task<string> GenerateImprovedContentAsync(string productInfo, string productDescription, CancellationToken cancellationToken = default);
    // TODO[moderate]: Реализовать метод генерации контента через OpenAI
  }
}

