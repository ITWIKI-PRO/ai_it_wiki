using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ai_it_wiki.Models.Ozon
{
    /// <summary>
    /// Ответ на запрос характеристик товаров
    /// </summary>
    public class ProductAttributesResponse
    {
        [JsonPropertyName("result")]
        public List<ProductAttributesItem>? Result { get; set; }

        [JsonPropertyName("total")]
        public int? Total { get; set; }

        [JsonPropertyName("last_id")]
        public string? LastId { get; set; }
    }

    /// <summary>
    /// Элемент списка характеристик товара
    /// </summary>
    public class ProductAttributesItem
    {
        [JsonPropertyName("id")]
        public long? Id { get; set; }

        [JsonPropertyName("barcode")]
        public string? Barcode { get; set; }

        [JsonPropertyName("barcodes")]
        public List<string>? Barcodes { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("offer_id")]
        public string? OfferId { get; set; }

        [JsonPropertyName("type_id")]
        public long? TypeId { get; set; }

        [JsonPropertyName("height")]
        public decimal? Height { get; set; }

        [JsonPropertyName("depth")]
        public decimal? Depth { get; set; }

        [JsonPropertyName("width")]
        public decimal? Width { get; set; }

        [JsonPropertyName("dimension_unit")]
        public string? DimensionUnit { get; set; }

        [JsonPropertyName("weight")]
        public decimal? Weight { get; set; }

        [JsonPropertyName("weight_unit")]
        public string? WeightUnit { get; set; }

        [JsonPropertyName("primary_image")]
        public string? PrimaryImage { get; set; }

        [JsonPropertyName("sku")]
        public long? Sku { get; set; }

        [JsonPropertyName("model_info")]
        public ModelInfo? ModelInfo { get; set; }

        [JsonPropertyName("images")]
        public List<string>? Images { get; set; }

        [JsonPropertyName("pdf_list")]
        public List<string>? PdfList { get; set; }

        [JsonPropertyName("attributes")]
        public List<ProductAttributeInfo>? Attributes { get; set; }

        [JsonPropertyName("attributes_with_defaults")]
        public List<long>? AttributesWithDefaults { get; set; }

        [JsonPropertyName("complex_attributes")]
        public List<ProductAttributeInfo>? ComplexAttributes { get; set; }

        [JsonPropertyName("color_image")]
        public string? ColorImage { get; set; }

        [JsonPropertyName("description_category_id")]
        public long? DescriptionCategoryId { get; set; }
    }

    /// <summary>
    /// Информация о модели товара
    /// </summary>
    public class ModelInfo
    {
        [JsonPropertyName("model_id")]
        public long? ModelId { get; set; }

        [JsonPropertyName("count")]
        public int? Count { get; set; }
    }

    /// <summary>
    /// Характеристика товара
    /// </summary>
    public class ProductAttributeInfo
    {
        [JsonPropertyName("id")]
        public long? Id { get; set; }

        [JsonPropertyName("complex_id")]
        public long? ComplexId { get; set; }

        [JsonPropertyName("values")]
        public List<AttributeValue>? Values { get; set; }
    }

    /// <summary>
    /// Значение характеристики товара
    /// </summary>
    public class AttributeValue
    {
        [JsonPropertyName("dictionary_value_id")]
        public long? DictionaryValueId { get; set; }

        [JsonPropertyName("value")]
        public string? Value { get; set; }
    }
}

