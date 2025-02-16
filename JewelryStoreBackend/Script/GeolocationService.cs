using JewelryStoreBackend.Models.GeoCoordinations;
using Newtonsoft.Json;
using Root = JewelryStoreBackend.Models.AddressModel.Root;
using Route = JewelryStoreBackend.Models.GeoCoordinations.Route;

namespace JewelryStoreBackend.Script;

public class GeolocationService
{
    public static async Task<List<Root>?> GetGeolocatesAsync(string address)
    {
        string url = $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(address)}&format=json&addressdetails=1";
        
        using (var client = new HttpClient())
        {
            var response = await client.GetStringAsync(url);
            var results = JsonConvert.DeserializeObject<List<Root>>(response);

            return results;
        }
    }

    public static async Task<Models.GeoCoordinations.Root?> GetGeolocateDistanceAsync(string lonStart, string latStart, string lonEnd, string latEnd)
    {
        Leg leg = new Leg { distance = 8000, duration = 1};
        Route route = new Route { legs = new List<Leg> { leg } };
        Models.GeoCoordinations.Root root = new() { routes = new List<Route> { route } };

        return root;
    }
}