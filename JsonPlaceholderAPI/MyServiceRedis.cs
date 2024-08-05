using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using JsonPlaceholderAPI.Models;

namespace JsonPlaceholderAPI
{
    public class MyServiceRedis
    {
        private readonly IDistributedCache _cache;
        private readonly HttpClient _httpClient;

        public MyServiceRedis(IDistributedCache cache, HttpClient httpClient)
        {
            _cache = cache;
            _httpClient = httpClient;
        }

        public async Task<User> GetUserByIdAsync(int userId)
        {
            string cacheKey = $"User_{userId}";
            var cachedUser = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedUser))
            {
                return JsonSerializer.Deserialize<User>(cachedUser);
            }

            var response = await _httpClient.GetAsync($"https://jsonplaceholder.typicode.com/users/{userId}");
            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();
            var user = JsonSerializer.Deserialize<User>(jsonString);

            var cacheOptions = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(1));

            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(user), cacheOptions);

            return user;
        }
    }
}
