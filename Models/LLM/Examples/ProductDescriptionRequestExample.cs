using Swashbuckle.AspNetCore.Filters;

namespace ai_it_wiki.Models.LLM.Examples
{
    public class ProductDescriptionRequestExample : IExamplesProvider<ProductDescriptionRequestDto>
    {
        public ProductDescriptionRequestDto GetExamples()
        {
            return new ProductDescriptionRequestDto { Sku = "2000000015156" };
        }
    }
}
