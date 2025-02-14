namespace JewelryStoreBackend.Script;

public class DeliveryCalculator
{
    public static double CalculateDeliveryCost(double weight, double distance, DeliveryType deliveryType)
    {
        double weightCost = weight * DeliveryPrices.WeightRate;
        double distanceCost = distance * DeliveryPrices.KilometerRate;

        return Math.Round((DeliveryPrices.BasePrice + weightCost + distanceCost) * DeliveryPrices.GetRate(deliveryType), 2);
    }
}

public class DeliveryPrices
{
    // Базовая стоимость доставки
    public const double BasePrice = 100;

    // Коэффицент за вес, гр
    public const double WeightRate = 2.0;

    // Коэффицент за километр пути, км
    public const double KilometerRate = 24.9;

    // Коэффиценты для доставок
    public const double BaseDeliveryRate = 1.0;
    public const double ExpressDeliveryRate = 1.5;
    public const double LightningDeliveryRate = 3.0;

    public static double GetRate(DeliveryType deliveryType)
    {
        if (deliveryType == DeliveryType.Express)
            return ExpressDeliveryRate;
        if (deliveryType == DeliveryType.Lightning)
            return LightningDeliveryRate;

        return BaseDeliveryRate;
    }
}

public enum DeliveryType
{
    Base,
    Express,
    Lightning,
}