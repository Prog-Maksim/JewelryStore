namespace JewelryStoreBackend.Script;

public class CostCalculation
{
    public static int CalculateDiscountPercentage(double cost, double costDiscount)
    {
        if (cost <= 0)
            throw new ArgumentException("Цена должна быть больше нуля.");
        
        return (int)((cost - costDiscount) / cost * 100);
    }
}