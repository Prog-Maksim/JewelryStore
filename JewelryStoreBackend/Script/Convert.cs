using JewelryStoreBackend.Models.DB.Product;
using Price = JewelryStoreBackend.Models.Response.ProductStructure.Price;

namespace JewelryStoreBackend.Script;

public class Convert
{
    public static Models.Response.Product ConvertToSimpleModel(ProductDb productDb)
    {
        return new Models.Response.Product
        {
            ProductId = productDb.ProductId,
            Sku = productDb.Specifications.FirstOrDefault()?.Sku ?? string.Empty,
            Title = productDb.Title,
            InStock = productDb.Specifications.FirstOrDefault()?.InStock ?? false, 
            OnSale = productDb.OnSale,
            Price = new Price
            {
                Cost = productDb.Price.Cost,
                Currency = productDb.Price.Currency,
                Discount = productDb.Price.Discount,
                Percent = productDb.Price.Percent,
                CostDiscount = productDb.Price.CostDiscount
            }
        };
    }
    
    // Метод для преобразования продукта, удаляя все Specifications и оставляя одну
    public static ProductDb ConvertProductWithSingleSpecification(ProductDb productDb, string? sku = null)
    {
        if (productDb.Specifications == null || !productDb.Specifications.Any())
            return productDb;
        
        if (!string.IsNullOrEmpty(sku))
        {
            var specification = productDb.Specifications.FirstOrDefault(spec => spec.Sku == sku);

            if (specification != null)
                productDb.Specifications = new List<Specifications> { specification };
            else
                productDb.Specifications = new List<Specifications>();
        }
        else
        {
            var firstSpecification = productDb.Specifications.First();
            productDb.Specifications = new List<Specifications> { firstSpecification };
        }

        return productDb;
    }

    public static List<ProductDb> ConvertProductWithSingleSpecification(List<ProductDb> products)
    {
        List<ProductDb> productsUpdate = new List<ProductDb>();

        foreach (var product in products)
            productsUpdate.Add(ConvertProductWithSingleSpecification(product));
        
        return productsUpdate;
    }
}