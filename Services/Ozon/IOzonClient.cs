using System.Threading.Tasks;

namespace ai_it_wiki.Services.Ozon
{
    public interface IOzonClient
    {
        Task<int> GetRatingAsync(long sku);
        Task UpdateCardAsync(long sku);
    }
}
