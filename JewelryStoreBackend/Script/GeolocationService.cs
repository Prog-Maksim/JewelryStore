using System.Net.Http.Headers;
using JewelryStoreBackend.Models.AddressModel;
using Newtonsoft.Json;
using Address = JewelryStoreBackend.Models.DB.User.Address;

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
        HttpClient client = new HttpClient();
        
        string url = "https://api.distance.services/api/v2/directions/get/route/v1/driving/" +
                     $"{lonStart},{latStart};{lonEnd},{latEnd}?apiKey=JMI5pUoitWW2eyMr7y4JWk29rhLYmIQ0&midpoint=true";
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);

        request.Headers.Add("distance-token", "0C7aw48QlR#6!n#2GnEB8kSJAa2!3br2");

        request.Content = new StringContent($"route%5B%5D={lonStart}%2C{latStart}&route%5B%5D={lonEnd}%2C{latEnd}");
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

        HttpResponseMessage response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        string responseBody = await response.Content.ReadAsStringAsync();

        return JsonConvert.DeserializeObject<Models.GeoCoordinations.Root>(responseBody);
    }
}