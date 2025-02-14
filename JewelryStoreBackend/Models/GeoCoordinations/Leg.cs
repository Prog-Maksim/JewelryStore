namespace JewelryStoreBackend.Models.GeoCoordinations;

public class Leg
{
    public string summary { get; set; }
    public double weight { get; set; }
    public double duration { get; set; }
    public List<object> steps { get; set; }
    public double distance { get; set; }
    public Midpoint midpoint { get; set; }
}