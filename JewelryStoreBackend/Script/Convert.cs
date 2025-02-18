using JewelryStoreBackend.Models.DB.Product;
using Price = JewelryStoreBackend.Models.Response.ProductStructure.Price;

namespace JewelryStoreBackend.Script;

public class Convert
{
    public static Models.Response.Product ConvertToSimpleModel(ProductDB productDb)
    {
        return new Models.Response.Product
        {
            ProductId = productDb.productId,
            SKU = productDb.specifications.FirstOrDefault()?.sku ?? string.Empty,
            Title = productDb.title,
            InStock = productDb.specifications.FirstOrDefault()?.inStock ?? false, 
            OnSale = productDb.onSale,
            Price = new Price
            {
                Cost = productDb.price.cost,
                Currency = productDb.price.currency,
                Discount = productDb.price.discount,
                Percent = productDb.price.percent,
                CostDiscount = productDb.price.costDiscount
            }
        };
    }
    
    // Метод для преобразования продукта, удаляя все Specifications и оставляя одну
    public static ProductDB ConvertProductWithSingleSpecification(ProductDB productDb, string? sku = null)
    {
        if (productDb.specifications == null || !productDb.specifications.Any())
            return productDb;
        
        if (!string.IsNullOrEmpty(sku))
        {
            var specification = productDb.specifications.FirstOrDefault(spec => spec.sku == sku);

            if (specification != null)
                productDb.specifications = new List<Specifications> { specification };
            else
                productDb.specifications = new List<Specifications>();
        }
        else
        {
            var firstSpecification = productDb.specifications.First();
            productDb.specifications = new List<Specifications> { firstSpecification };
        }

        return productDb;
    }

    public static List<ProductDB> ConvertProductWithSingleSpecification(List<ProductDB> products)
    {
        List<ProductDB> productsUpdate = new List<ProductDB>();

        foreach (var product in products)
            productsUpdate.Add(ConvertProductWithSingleSpecification(product));
        
        return productsUpdate;
    }
}