using Microsoft.Extensions.Caching.Memory;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using JsonPlaceholderAPI.Models;

public class MyService
{
    private readonly IMemoryCache _cache;
    private readonly HttpClient _httpClient;

    public MyService(IMemoryCache cache, HttpClient httpClient)
    {
        _cache = cache;
        _httpClient = httpClient;
    }

    public async Task<User> GetUserByIdAsync(int userId)
    {
        // Cache key'ini oluşturma
        string cacheKey = $"User_{userId}";

        // Cache'de var mı kontrol et
        if (_cache.TryGetValue(cacheKey, out User cachedUser))
        {
            return cachedUser;
        }

        // Cache'de yoksa API'den veri al
        var response = await _httpClient.GetAsync($"https://jsonplaceholder.typicode.com/users/{userId}");
        response.EnsureSuccessStatusCode();

        var jsonString = await response.Content.ReadAsStringAsync();
        var user = JsonSerializer.Deserialize<User>(jsonString);

        // Cache'e ekle ve 1 dakika boyunca sakla
        _cache.Set(cacheKey, user, TimeSpan.FromMinutes(1));

        return user;
    }
}
