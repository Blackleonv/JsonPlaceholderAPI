using JsonPlaceholderAPI.Models; // User sınıfının tanımlı olduğu ad alanı
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

public class MyService
{
    private readonly IMemoryCache _cache;
    private readonly HttpClient _httpClient;

    public MyService(IMemoryCache cache, HttpClient httpClient)
    {
        _cache = cache;
        _httpClient = httpClient;
    }

    public async Task<User?> GetUserByIdAsync(int userId)
    {
        string cacheKey = $"User_{userId}";

        if (_cache.TryGetValue(cacheKey, out User? cachedUser))
        {
            return cachedUser;
        }

        var response = await _httpClient.GetAsync($"https://jsonplaceholder.typicode.com/users/{userId}");
        response.EnsureSuccessStatusCode();

        var jsonString = await response.Content.ReadAsStringAsync();
        var user = JsonSerializer.Deserialize<User?>(jsonString);

        if (user != null)
        {
            _cache.Set(cacheKey, user, TimeSpan.FromMinutes(1));
        }

        return user;
    }
}

public class MyServiceRedis
{
    private readonly IDistributedCache _cache;
    private readonly HttpClient _httpClient;

    public MyServiceRedis(IDistributedCache cache, HttpClient httpClient)
    {
        _cache = cache;
        _httpClient = httpClient;
    }

    public async Task<User?> GetUserByIdAsync(int userId)
    {
        string cacheKey = $"User_{userId}";

        var cachedUser = await _cache.GetStringAsync(cacheKey);
        if (!string.IsNullOrEmpty(cachedUser))
        {
            var user = JsonSerializer.Deserialize<User?>(cachedUser);
            if (user != null)
            {
                return user;
            }
        }

        var response = await _httpClient.GetAsync($"https://jsonplaceholder.typicode.com/users/{userId}");
        response.EnsureSuccessStatusCode();

        var jsonString = await response.Content.ReadAsStringAsync();
        var userFromApi = JsonSerializer.Deserialize<User?>(jsonString);

        if (userFromApi != null)
        {
            var cacheOptions = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(1));

            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(userFromApi), cacheOptions);
        }

        return userFromApi;
    }
}
