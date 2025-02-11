using JewelryStoreBackend.Models.DB.Product;
using Price = JewelryStoreBackend.Models.Response.ProductStructure.Price;

namespace JewelryStoreBackend.Script;

public class Convert
{
    public static Models.Response.Product ConvertToSimpleModel(Product product)
    {
        return new Models.Response.Product
        {
            ProductId = product.productId,
            SKU = product.specifications.FirstOrDefault()?.sku ?? string.Empty,
            Title = product.title,
            InStock = product.specifications.FirstOrDefault()?.inStock ?? false, 
            OnSale = product.onSale,
            Price = new Price
            {
                Cost = product.price.cost,
                Currency = product.price.currency,
                Discount = product.price.discount,
                Percent = product.price.percent,
                CostDiscount = product.price.costDiscount
            }
        };
    }
    
    public static List<Models.Response.Product> ConvertToSimplifiedModel(List<Product> products)
    {
        return products.Select(ConvertToSimpleModel).ToList();
    }
    
    // Метод для преобразования продукта, удаляя все Specifications и оставляя одну
    public static Product ConvertProductWithSingleSpecification(Product product, string? sku = null)
    {
        if (product.specifications == null || !product.specifications.Any())
            return product;
        
        if (!string.IsNullOrEmpty(sku))
        {
            var specification = product.specifications.FirstOrDefault(spec => spec.sku == sku);

            if (specification != null)
                product.specifications = new List<Specifications> { specification };
            else
                product.specifications = new List<Specifications>();
        }
        else
        {
            var firstSpecification = product.specifications.First();
            product.specifications = new List<Specifications> { firstSpecification };
        }

        return product;
    }

    public static List<Product> ConvertProductWithSingleSpecification(List<Product> products)
    {
        List<Product> productsUpdate = new List<Product>();

        foreach (var product in products)
            productsUpdate.Add(ConvertProductWithSingleSpecification(product));
        
        return productsUpdate;
    }
}