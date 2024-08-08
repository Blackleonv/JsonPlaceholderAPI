using JsonPlaceholderAPI.Models;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace JsonPlaceholderAPI.Services
{
    public class MyServiceRedis
    {
        private readonly IDistributedCache _cache;
        private readonly HttpClient _httpClient;

        public MyServiceRedis(IDistributedCache cache, HttpClient httpClient)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<User> GetUserByIdAsync(int userId)
        {
            string cacheKey = $"User_{userId}";
            var cachedUser = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedUser))
            {
                var userFromCache = JsonSerializer.Deserialize<User>(cachedUser);
                if (userFromCache != null)
                {
                    return userFromCache;
                }
            }

            var response = await _httpClient.GetAsync($"https://jsonplaceholder.typicode.com/users/{userId}");
            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();
            var user = JsonSerializer.Deserialize<User>(jsonString);

            if (user != null)
            {
                var cacheOptions = new DistributedCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(1));

                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(user), cacheOptions);
            }

            // Eğer 'user' null ise özel bir durum veya varsayılan bir değer döndürün.
            return user ?? throw new InvalidOperationException("User not found or could not be deserialized.");
        }
    }
}
