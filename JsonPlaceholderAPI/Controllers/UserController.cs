using FluentValidation;
using FluentValidation.Results;
using JsonPlaceholderAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace UserApiExample.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
                var user = JsonSerializer.Deserialize<User>(jsonString);

                if (user == null)
                {
                    return NotFound();
                }

                return Ok(user);
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

            // Kullanıcıyı veritabanına ekle
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
        }
    }
}
