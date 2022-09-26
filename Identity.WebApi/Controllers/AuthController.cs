using Identity.Shared.ViewModel;
using Identity.WebApi.Services;
using Microsoft.AspNetCore.Authorization;
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
        //api/auth/register
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

        [HttpPost("login")]
        //api/auth/login
        public async Task<IActionResult> LoginAsync(LoginVM model)
        {
            if (ModelState.IsValid)
            {
                var rs = await _userService.LoginUserAsycn(model);

                if (rs.IsSuccess)
                    return Ok(rs);
                return BadRequest(rs);
            }
            return BadRequest(ModelState);
        }


    }
}
