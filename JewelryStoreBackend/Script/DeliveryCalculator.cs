using JewelryStoreBackend.Models.Response.Order;

namespace JewelryStoreBackend.Script;

public class DeliveryCalculator
{
    public static DetailsShippingOrderData CalculateDeliveryCost(double distance, DeliveryType deliveryType)
    {
        double distanceCost = distance * DeliveryPrices.KilometerRate;

        var shippingPrice = Math.Round((DeliveryPrices.BasePrice + distanceCost) * DeliveryPrices.GetRate(deliveryType), 2);

        DetailsShippingOrderData shippingOrderData = new DetailsShippingOrderData
        {
            Distance = distance,
            DistanceCost = distanceCost,
            ShippingBasePrice = DeliveryPrices.BasePrice,
            DeliveryRate = DeliveryPrices.GetRate(deliveryType),
            KilometerRate = DeliveryPrices.KilometerRate,
            TotalCost = shippingPrice
        };

        return shippingOrderData;
    }
}

public class DeliveryPrices
{
    // Базовая стоимость доставки
    public const double BasePrice = 100;

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