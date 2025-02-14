namespace JewelryStoreBackend.Models.GeoCoordinations;

public class Waypoint
{
    public string hint { get; set; }
    public double distance { get; set; }
    public string name { get; set; }
    public List<double> location { get; set; }
}