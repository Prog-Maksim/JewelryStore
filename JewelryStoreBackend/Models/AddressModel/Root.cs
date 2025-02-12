namespace JewelryStoreBackend.Models.AddressModel;

public class Root
{
    public int place_id { get; set; }
    public string licence { get; set; }
    public string osm_type { get; set; }
    public int osm_id { get; set; }
    public string lat { get; set; }
    public string lon { get; set; }
    public string @class { get; set; }
    public string type { get; set; }
    public int place_rank { get; set; }
    public double importance { get; set; }
    public string addresstype { get; set; }
    public string name { get; set; }
    public string display_name { get; set; }
    public Address address { get; set; }
    public List<string> boundingbox { get; set; }
}