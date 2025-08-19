using Swashbuckle.AspNetCore.Filters;

namespace ai_it_wiki.Models.LLM.Examples
{
    public class ProductDescriptionResponseExample : IExamplesProvider<ProductDescriptionResponseDto>
    {
        public ProductDescriptionResponseDto GetExamples()
        {
            return new ProductDescriptionResponseDto
            {
                Sku = "2000000015156",
                Description = "Трёхколодочное сцепление для скутеров Honda Dio...",
                OfferId = "2000000015156",
                Name = "Плата сцепления TWH тюнинг на скутер Honda Dio..."
            };
        }
    }
}
