using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using JsonPlaceholderAPI.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace JsonPlaceholderAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        public UserController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [HttpGet]
        public async Task<IEnumerable<User>> Get()
        {
            var response = await _httpClient.GetAsync("https://jsonplaceholder.typicode.com/users");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var users = JsonSerializer.Deserialize<List<User>>(json, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            return users;
        }
    }
}
