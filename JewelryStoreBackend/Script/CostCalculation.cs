namespace JewelryStoreBackend.Script;

public class CostCalculation
{
    public static int CalculateDiscountPercentage(double cost, double costDiscount)
    {
        if (cost <= 0)
            throw new ArgumentException("Цена должна быть больше нуля.");
        
        return (int)((cost - costDiscount) / cost * 100);
    }
    
    public static double CalculateDiscountedPrice(double originalPrice, int discountPercent)
    {
        if (originalPrice < 0)
        {
            throw new ArgumentException("Цена не может быть отрицательной", nameof(originalPrice));
        }

        if (discountPercent < 0 || discountPercent > 100)
        {
            throw new ArgumentException("Процент скидки должен быть в диапазоне от 0 до 100", nameof(discountPercent));
        }

        double discount = originalPrice * discountPercent / 100;
        return Math.Round(originalPrice - discount, 2);
    }
}