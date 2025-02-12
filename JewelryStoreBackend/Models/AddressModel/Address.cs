using Newtonsoft.Json;

namespace JewelryStoreBackend.Models.AddressModel;

public class Address
{
    public string house_number { get; set; }
    public string road { get; set; }
    public string suburb { get; set; }
    public string city_district { get; set; }
    public string city { get; set; }
    public string county { get; set; }
    public string state { get; set; }
    
    [JsonProperty("ISO3166-2-lvl4")]
    public string ISO31662lvl4 { get; set; }
    public string postcode { get; set; }
    public string country { get; set; }
    public string country_code { get; set; }
}