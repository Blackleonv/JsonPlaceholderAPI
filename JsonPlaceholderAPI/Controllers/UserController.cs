using FluentValidation;
using FluentValidation.Results;
using JsonPlaceholderAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace UserApiExample.Controllers
{
    [ApiController]
    [Route("api/userapi/users")]
    public class UserController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly AppDbContext _context;
        private readonly IValidator<User> _validator;

        public UserController(HttpClient httpClient, AppDbContext context, IValidator<User> validator)
        {
            _httpClient = httpClient;
            _context = context;
            _validator = validator;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"https://jsonplaceholder.typicode.com/users/{id}");
                response.EnsureSuccessStatusCode();

                var jsonString = await response.Content.ReadAsStringAsync();

                try
                {
                    var user = JsonSerializer.Deserialize<User>(jsonString);

                    if (user == null)
                    {
                        return NotFound();
                    }

                    return Ok(user);
                }
                catch (JsonException ex)
                {
                    return StatusCode(500, $"Deserialization error: {ex.Message}");
                }
            }
            catch (HttpRequestException e)
            {
                return StatusCode(500, $"Internal server error: {e.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] User user)
        {
            if (user == null)
            {
                return BadRequest("User object is null");
            }

            // Kullanıcı doğrulamasını yap
            ValidationResult validationResult = await _validator.ValidateAsync(user);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            // Email kontrolü: Aynı email ile başka bir kullanıcı olup olmadığını kontrol et
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == user.Email);

            if (existingUser != null)
            {
                return BadRequest("Email is already in use.");
            }

            // Eğer kullanıcı bir adres içeriyorsa, adresi veritabanına ekleyin
            if (user.Address != null)
            {
                _context.Add(user.Address);  // Adres ekleniyor
            }

            // Kullanıcıyı veritabanına ekleyin
            _context.Users.Add(user);  // Kullanıcı ekleniyor

            // Değişiklikleri kaydedin
            await _context.SaveChangesAsync();

            // Başarıyla eklenen kullanıcıyı döndürün
            return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] User user)
        {
            if (user == null || user.Id != id)
            {
                return BadRequest("User object is null or Id does not match.");
            }

            // Kullanıcı doğrulamasını yap
            ValidationResult validationResult = await _validator.ValidateAsync(user);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
            {
                return NotFound();
            }

            // Mevcut kullanıcı bilgilerini güncelle
            existingUser.Name = user.Name;
            existingUser.Username = user.Username;
            existingUser.Email = user.Email;
            existingUser.Phone = user.Phone;
            existingUser.Website = user.Website;

            // Adresi güncelle
            if (user.Address != null)
            {
                _context.Entry(existingUser.Address).CurrentValues.SetValues(user.Address);
            }

            // Değişiklikleri kaydet
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
