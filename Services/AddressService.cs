using ClaimsPortal.Models;

namespace ClaimsPortal.Services;

/// <summary>
/// Interface for address geocoding services
/// Supports address search and validation
/// </summary>
public interface IAddressService
{
    /// <summary>
    /// Search for addresses based on partial input
    /// </summary>
    /// <param name="query">Partial address query (street, city, state, zip)</param>
    /// <returns>List of matching address results</returns>
    Task<List<AddressSearchResult>> SearchAddressesAsync(string query);

    /// <summary>
    /// Get remaining API calls for the day
    /// </summary>
    Task<int> GetRemainingCallsAsync();
}

/// <summary>
/// Geocodio Address Service Implementation
/// Free tier: 2,500 calls/day
/// Lite tier: 10,000 calls/day
/// Plus tier: Unlimited
/// For this application: 200 calls/day maximum
/// </summary>
public class GeocodioAddressService : IAddressService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly string _apiKey;
    private const string BaseUrl = "https://api.geocod.io/v1.7/geocode";
    private const string AutocompleteUrl = "https://api.geocod.io/v1.7/geocode/autocomplete";

    public GeocodioAddressService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _apiKey = configuration["Geocodio:ApiKey"] ?? throw new InvalidOperationException("Geocodio API key not configured");
    }

    public async Task<List<AddressSearchResult>> SearchAddressesAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 3)
            return [];

        try
        {
            // Use autocomplete endpoint for partial address matching
            var requestUrl = $"{AutocompleteUrl}?q={Uri.EscapeDataString(query)}&api_key={_apiKey}&country=US&limit=5";

            var response = await _httpClient.GetAsync(requestUrl);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Geocodio API error: {response.StatusCode}");
                return [];
            }

            var content = await response.Content.ReadAsStringAsync();
            var jsonDocument = System.Text.Json.JsonDocument.Parse(content);
            var root = jsonDocument.RootElement;

            if (!root.TryGetProperty("results", out var results))
                return [];

            var addressResults = new List<AddressSearchResult>();

            foreach (var result in results.EnumerateArray())
            {
                try
                {
                    var addressParts = new Dictionary<string, string>();

                    if (result.TryGetProperty("address_components", out var components))
                    {
                        if (components.TryGetProperty("number", out var number))
                            addressParts["number"] = number.GetString() ?? "";
                        if (components.TryGetProperty("predirectional", out var predirectional))
                            addressParts["predirectional"] = predirectional.GetString() ?? "";
                        if (components.TryGetProperty("street", out var street))
                            addressParts["street"] = street.GetString() ?? "";
                        if (components.TryGetProperty("suffix", out var suffix))
                            addressParts["suffix"] = suffix.GetString() ?? "";
                        if (components.TryGetProperty("city", out var city))
                            addressParts["city"] = city.GetString() ?? "";
                        if (components.TryGetProperty("state", out var state))
                            addressParts["state"] = state.GetString() ?? "";
                        if (components.TryGetProperty("zip", out var zip))
                            addressParts["zip"] = zip.GetString() ?? "";
                        if (components.TryGetProperty("county", out var county))
                            addressParts["county"] = county.GetString() ?? "";
                    }

                    var formattedAddress = string.Empty;
                    if (addressParts.ContainsKey("number"))
                        formattedAddress += addressParts["number"] + " ";
                    if (addressParts.ContainsKey("predirectional"))
                        formattedAddress += addressParts["predirectional"] + " ";
                    if (addressParts.ContainsKey("street"))
                        formattedAddress += addressParts["street"] + " ";
                    if (addressParts.ContainsKey("suffix"))
                        formattedAddress += addressParts["suffix"];

                    var latitude = 0.0;
                    var longitude = 0.0;

                    if (result.TryGetProperty("location", out var location))
                    {
                        if (location.TryGetProperty("lat", out var lat))
                            latitude = lat.GetDouble();
                        if (location.TryGetProperty("lng", out var lng))
                            longitude = lng.GetDouble();
                    }

                    addressResults.Add(new AddressSearchResult
                    {
                        Address = formattedAddress.Trim(),
                        City = addressParts.ContainsKey("city") ? addressParts["city"] : "",
                        State = addressParts.ContainsKey("state") ? addressParts["state"] : "",
                        ZipCode = addressParts.ContainsKey("zip") ? addressParts["zip"] : "",
                        County = addressParts.ContainsKey("county") ? addressParts["county"] : "",
                        Latitude = latitude,
                        Longitude = longitude,
                        Accuracy = "rooftop" // Geocodio accuracy level
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error parsing address result: {ex.Message}");
                    continue;
                }
            }

            return addressResults;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error searching addresses: {ex.Message}");
            return [];
        }
    }

    public async Task<int> GetRemainingCallsAsync()
    {
        // For demonstration, return a fixed value
        // In a real implementation, you would track API calls against the daily limit
        // Geocodio provides usage info in response headers
        return 200; // Assuming 200 call limit per day for this app
    }
}

/// <summary>
/// Mock Address Service for Development/Testing
/// Provides simulated address search results
/// </summary>
public class MockAddressService : IAddressService
{
    private readonly List<AddressSearchResult> _mockAddresses = new()
    {
        new AddressSearchResult
        {
            Address = "123 Main Street",
            City = "Springfield",
            State = "IL",
            ZipCode = "62701",
            County = "Sangamon",
            Latitude = 39.7817,
            Longitude = -89.6501,
            Accuracy = "rooftop"
        },
        new AddressSearchResult
        {
            Address = "456 Oak Avenue",
            City = "Springfield",
            State = "IL",
            ZipCode = "62702",
            County = "Sangamon",
            Latitude = 39.7842,
            Longitude = -89.6489,
            Accuracy = "rooftop"
        },
        new AddressSearchResult
        {
            Address = "789 Elm Street",
            City = "Springfield",
            State = "IL",
            ZipCode = "62703",
            County = "Sangamon",
            Latitude = 39.7891,
            Longitude = -89.6520,
            Accuracy = "rooftop"
        },
        new AddressSearchResult
        {
            Address = "321 Pine Road",
            City = "Springfield",
            State = "IL",
            ZipCode = "62704",
            County = "Sangamon",
            Latitude = 39.7768,
            Longitude = -89.6450,
            Accuracy = "rooftop"
        }
    };

    public Task<List<AddressSearchResult>> SearchAddressesAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Task.FromResult(new List<AddressSearchResult>());

        // Filter mock addresses based on query
        var results = _mockAddresses
            .Where(a => a.Address.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                        a.City.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                        a.ZipCode.Contains(query, StringComparison.OrdinalIgnoreCase))
            .Take(5)
            .ToList();

        return Task.FromResult(results);
    }

    public Task<int> GetRemainingCallsAsync()
    {
        return Task.FromResult(200);
    }
}
