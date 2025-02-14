using System.Text.Json.Serialization;

namespace JewelryStoreBackend.Models.GeoCoordinations;

public class Route
{
    [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
    public string geometry { get; set; }
    public List<Leg> legs { get; set; }
    public string weight_name { get; set; }
    public double weight { get; set; }
    public double duration { get; set; }
    public double distance { get; set; }
    public Midpoint midpoint { get; set; }
}