using FluentValidation;
using FluentValidation.Results;
using JsonPlaceholderAPI.Models;
using JsonPlaceholderAPI.Repositories;  // IRepository<User> için
using Microsoft.AspNetCore.Mvc;
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
        private readonly IValidator<User> _validator;
        private readonly IRepository<User> _userRepository;

        public UserController(HttpClient httpClient, IValidator<User> validator, IRepository<User> userRepository)
        {
            _httpClient = httpClient;
            _validator = validator;
            _userRepository = userRepository;
        }

        // Harici API'den kullanıcıyı getirir
        [HttpGet("external/{id}")]
        public async Task<IActionResult> GetUserFromExternalApi(int id)
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

        // Veritabanından kullanıcıyı getirir
        [HttpGet("db/{id}")]
        public async Task<IActionResult> GetUserFromDatabase(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        // Yeni kullanıcı oluşturur
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
            var existingUser = await _userRepository.GetAllAsync();
            if (existingUser.Any(u => u.Email == user.Email))
            {
                return BadRequest("Email is already in use.");
            }

            // Kullanıcıyı ekle
            await _userRepository.AddAsync(user);

            // Başarıyla eklenen kullanıcıyı döndürün
            return CreatedAtAction(nameof(GetUserFromDatabase), new { id = user.Id }, user);
        }

        // Mevcut kullanıcıyı günceller
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

            var existingUser = await _userRepository.GetByIdAsync(id);
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

            // Güncellenmiş kullanıcıyı veritabanına kaydet
            _userRepository.Update(existingUser);

            return NoContent();
        }

        // Kullanıcıyı siler
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            await _userRepository.DeleteAsync(id);

            return NoContent();
        }
    }
}
