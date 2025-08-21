using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ai_it_wiki.Models.Ozon
{
    public class OzonResponse<T>
    {
        [JsonPropertyName("result")]
        public T Result { get; set; }
    }

    public class OzonResponseResult<T>
    {
        [JsonPropertyName("items")]
        public List<T> Items { get; set; }

        [JsonPropertyName("total")]
        public int Total { get; set; }

        [JsonPropertyName("last_id")]
        public string LastId { get; set; }
    }

    public class ProductInfoListResponse
    {
        [JsonPropertyName("items")]
        public List<ProductItem> Items { get; set; }

        //[JsonPropertyName("total")]
        //public int Total { get; set; }

        //[JsonPropertyName("last_id")]
        //public string LastId { get; set; }
    }

    //public class ProductItem
    //{
    //  [JsonPropertyName("offer_id")]
    //  public string OfferId { get; set; }

    //  [JsonPropertyName("product_id")]
    //  public long ProductId { get; set; }

    //  [JsonPropertyName("sku")]
    //  public string Sku { get; set; }

    //  [JsonPropertyName("name")]
    //  public string Name { get; set; }

    //  [JsonPropertyName("description_category_id")]
    //  public long DescriptionCategoryId { get; set; }

    //  [JsonPropertyName("attributes")]
    //  public List<ProductAttribute> Attributes { get; set; }
    //}

    public class ProductItem
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("offer_id")]
        public string OfferId { get; set; }

        [JsonPropertyName("is_archived")]
        public bool IsArchived { get; set; }

        [JsonPropertyName("is_autoarchived")]
        public bool IsAutoArchived { get; set; }

        [JsonPropertyName("barcodes")]
        public List<string> Barcodes { get; set; }

        [JsonPropertyName("description_category_id")]
        public long DescriptionCategoryId { get; set; }

        [JsonPropertyName("type_id")]
        public long TypeId { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("images")]
        public List<string> Images { get; set; }

        [JsonPropertyName("currency_code")]
        public string CurrencyCode { get; set; }

        [JsonPropertyName("marketing_price")]
        public string MarketingPrice { get; set; }

        [JsonPropertyName("min_price")]
        public string MinPrice { get; set; }

        [JsonPropertyName("old_price")]
        public string OldPrice { get; set; }

        [JsonPropertyName("price")]
        public string Price { get; set; }

        [JsonPropertyName("sources")]
        public List<ProductSource> Sources { get; set; }

        [JsonPropertyName("model_info")]
        public ProductModelInfo ModelInfo { get; set; }

        [JsonPropertyName("commissions")]
        public List<ProductCommission> Commissions { get; set; }

        [JsonPropertyName("is_prepayment_allowed")]
        public bool IsPrepaymentAllowed { get; set; }

        [JsonPropertyName("volume_weight")]
        public double VolumeWeight { get; set; }

        [JsonPropertyName("has_discounted_fbo_item")]
        public bool HasDiscountedFboItem { get; set; }

        [JsonPropertyName("is_discounted")]
        public bool IsDiscounted { get; set; }

        [JsonPropertyName("discounted_fbo_stocks")]
        public int DiscountedFboStocks { get; set; }

        [JsonPropertyName("stocks")]
        public ProductStocks Stocks { get; set; }

        [JsonPropertyName("errors")]
        public List<object> Errors { get; set; }

        [JsonPropertyName("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [JsonPropertyName("vat")]
        public string Vat { get; set; }

        [JsonPropertyName("visibility_details")]
        public ProductVisibilityDetails VisibilityDetails { get; set; }

        [JsonPropertyName("price_indexes")]
        public ProductPriceIndexes PriceIndexes { get; set; }

        [JsonPropertyName("images360")]
        public List<string> Images360 { get; set; }

        [JsonPropertyName("is_kgt")]
        public bool IsKgt { get; set; }

        [JsonPropertyName("color_image")]
        public List<string> ColorImage { get; set; }

        [JsonPropertyName("primary_image")]
        public List<string> PrimaryImage { get; set; }

        [JsonPropertyName("statuses")]
        public ProductStatuses Statuses { get; set; }

        [JsonPropertyName("is_super")]
        public bool IsSuper { get; set; }

        [JsonPropertyName("is_seasonal")]
        public bool IsSeasonal { get; set; }

        //[JsonPropertyName("promotions")]
        //public List<ProductPromotion> Promotions { get; set; }

        [JsonPropertyName("sku")]
        public long Sku { get; set; }

        /// <summary>
        /// Описание товара, доступно при запросе поля <c>description</c>.
        /// </summary>
        [JsonPropertyName("description"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string? Description { get; set; }
    }

    public class ProductSource
    {
        [JsonPropertyName("sku")]
        public long Sku { get; set; }

        [JsonPropertyName("source")]
        public string Source { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("shipment_type")]
        public string ShipmentType { get; set; }

        [JsonPropertyName("quant_code")]
        public string QuantCode { get; set; }
    }

    public class ProductModelInfo
    {
        [JsonPropertyName("model_id")]
        public long ModelId { get; set; }

        [JsonPropertyName("count")]
        public int Count { get; set; }
    }

    public class ProductCommission
    {
        [JsonPropertyName("delivery_amount")]
        public decimal? DeliveryAmount { get; set; }

        [JsonPropertyName("percent")]
        public decimal Percent { get; set; }

        [JsonPropertyName("return_amount")]
        public decimal? ReturnAmount { get; set; }

        [JsonPropertyName("sale_schema")]
        public string SaleSchema { get; set; }

        [JsonPropertyName("value")]
        public decimal Value { get; set; }
    }

    public class ProductStocks
    {
        [JsonPropertyName("has_stock")]
        public bool HasStock { get; set; }

        [JsonPropertyName("stocks")]
        public List<ProductStockItem> Stocks { get; set; }
    }

    public class ProductStockItem
    {
        [JsonPropertyName("present")]
        public int Present { get; set; }

        [JsonPropertyName("reserved")]
        public int Reserved { get; set; }

        [JsonPropertyName("sku")]
        public long Sku { get; set; }

        [JsonPropertyName("source")]
        public string Source { get; set; }
    }

    public class ProductVisibilityDetails
    {
        [JsonPropertyName("has_price")]
        public bool HasPrice { get; set; }

        [JsonPropertyName("has_stock")]
        public bool HasStock { get; set; }
    }

    public class ProductPriceIndexes
    {
        [JsonPropertyName("color_index")]
        public string ColorIndex { get; set; }

        [JsonPropertyName("external_index_data")]
        public ProductExternalIndexData ExternalIndexData { get; set; }

        [JsonPropertyName("ozon_index_data")]
        public ProductOzonIndexData OzonIndexData { get; set; }

        [JsonPropertyName("self_marketplaces_index_data")]
        public ProductSelfMarketplacesIndexData SelfMarketplacesIndexData { get; set; }
    }

    public class ProductExternalIndexData
    {
        [JsonPropertyName("minimal_price")]
        public string MinimalPrice { get; set; }

        [JsonPropertyName("minimal_price_currency")]
        public string MinimalPriceCurrency { get; set; }

        [JsonPropertyName("price_index_value")]
        public decimal PriceIndexValue { get; set; }
    }

    public class ProductOzonIndexData
    {
        [JsonPropertyName("minimal_price")]
        public string MinimalPrice { get; set; }

        [JsonPropertyName("minimal_price_currency")]
        public string MinimalPriceCurrency { get; set; }

        [JsonPropertyName("price_index_value")]
        public decimal PriceIndexValue { get; set; }
    }

    public class ProductSelfMarketplacesIndexData
    {
        [JsonPropertyName("minimal_price")]
        public string MinimalPrice { get; set; }

        [JsonPropertyName("minimal_price_currency")]
        public string MinimalPriceCurrency { get; set; }

        [JsonPropertyName("price_index_value")]
        public decimal PriceIndexValue { get; set; }
    }

    public class ProductStatuses
    {
        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("status_failed")]
        public string StatusFailed { get; set; }

        [JsonPropertyName("moderate_status")]
        public string ModerateStatus { get; set; }

        [JsonPropertyName("validation_status")]
        public string ValidationStatus { get; set; }

        [JsonPropertyName("status_name")]
        public string StatusName { get; set; }

        [JsonPropertyName("status_description")]
        public string StatusDescription { get; set; }

        [JsonPropertyName("is_created")]
        public bool IsCreated { get; set; }

        [JsonPropertyName("status_tooltip")]
        public string StatusTooltip { get; set; }

        [JsonPropertyName("status_updated_at")]
        public DateTime StatusUpdatedAt { get; set; }
    }

    public class ProductAttribute
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("value")]
        public string Value { get; set; }

        [JsonPropertyName("values")]
        public List<string> Values { get; set; }
    }

    public class ProductListItem
    {
        [JsonPropertyName("product_id")]
        public long ProductId { get; set; }

        [JsonPropertyName("offer_id")]
        public string OfferId { get; set; }

        [JsonPropertyName("has_fbo_stocks")]
        public bool HasFboStocks { get; set; }

        [JsonPropertyName("has_fbs_stocks")]
        public bool HasFbsStocks { get; set; }

        [JsonPropertyName("archived")]
        public bool Archived { get; set; }

        [JsonPropertyName("is_discounted")]
        public bool IsDiscounted { get; set; }

        [JsonPropertyName("quants")]
        public List<object> Quants { get; set; }

        [JsonPropertyName("sku")]
        public long Sku { get; set; }

        [JsonPropertyName("rating"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? Rating { get; set; }

        [JsonPropertyName("groups"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<RatingGroup>? Groups { get; set; }
    }

    public class ProductInfoListRequest
    {
        [JsonPropertyName("product_id")]
        public List<long> ProductId { get; set; } = new List<long>();

        /// <summary>
        /// Набор полей, которые необходимо вернуть. Например, <c>description</c>.
        /// </summary>
        [JsonPropertyName("fields"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public List<string>? Fields { get; set; }
    }

    public class RatingRequest
    {
        [JsonPropertyName("skus")]
        public List<long> Skus { get; set; } = new List<long>();
    }

    public class RatingResponse
    {
        [JsonPropertyName("products")]
        public List<ProductRating> Products { get; set; }
    }

    public class ProductRating
    {
        [JsonPropertyName("sku")]
        public long Sku { get; set; }

        [JsonPropertyName("rating")]
        public double Rating { get; set; }

        [JsonPropertyName("groups")]
        public List<RatingGroup> Groups { get; set; }

        /// <summary>
        /// Описание товара, подставляется вручную при запросе расширенной информации.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string? Description { get; set; }
    }

    public class RatingGroup
    {
        [JsonPropertyName("key")]
        public string Key { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("rating")]
        public double Rating { get; set; }

        [JsonPropertyName("weight")]
        public int Weight { get; set; }

        [JsonPropertyName("conditions")]
        public List<RatingCondition> Conditions { get; set; }

        [JsonPropertyName("improve_attributes")]
        public List<RatingImproveAttribute> ImproveAttributes { get; set; }

        [JsonPropertyName("improve_at_least")]
        public int ImproveAtLeast { get; set; }
    }

    public class RatingCondition
    {
        [JsonPropertyName("key")]
        public string Key { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("fulfilled")]
        public bool Fulfilled { get; set; }

        [JsonPropertyName("cost")]
        public int Cost { get; set; }
    }

    public class RatingImproveAttribute
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}
