using Identity.Shared.ViewModel;
using Identity.WebApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Identity.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private IUserService _userService;
        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync(RegisterVM model)
        {
            if (ModelState.IsValid)
            {
                var result = await _userService.RegisterUserAsycn(model);

                if(result.IsSuccess)
                    return Ok(result);
                return BadRequest(result);
            }
            return BadRequest(ModelState);
        }
    }
}
