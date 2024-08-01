using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class MyController : ControllerBase
{
    private readonly MyService _myService;

    public MyController(MyService myService)
    {
        _myService = myService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var user = await _myService.GetUserByIdAsync(id);
        return Ok(user);
    }
}
