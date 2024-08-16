using Microsoft.AspNetCore.Mvc;
using static MyService;

namespace JsonPlaceholderAPI.Controllers
{
    [ApiController]
    [Route("api/jsonplaceholder/users")]
    public class UserController : ControllerBase
    {
        private readonly MyServiceRedis _myService;

        public UserController(MyServiceRedis myService)
        {
            _myService = myService;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserById(int userId)
        {
            var user = await _myService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }
    }
}
