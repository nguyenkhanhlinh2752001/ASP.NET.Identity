using Identity.Shared.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Identity.WebApi.Services
{
    public class UserService : IUserService
    {
        private UserManager<IdentityUser> _userManager;
        private IConfiguration _configuration;
        public UserService(UserManager<IdentityUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        

        public async Task<UserManagerResponseVM> RegisterUserAsycn(RegisterVM model)
        { 

            if (model == null)
                throw new NullReferenceException(nameof(model));

            if (model.Password != model.ConfirmPassword)
                return new UserManagerResponseVM
                {
                    Message = "Confirm password doesn't match the password",
                    IsSuccess = false
                };
            

            var identityUser = new IdentityUser
            {
                Email = model.Email,
                UserName = model.Email,
            };

            var result = await _userManager.CreateAsync(identityUser, model.Password);

            if (result.Succeeded)
            {
                return new UserManagerResponseVM
                {
                    Message = "User created successfully",
                    IsSuccess = true
                };
            }
            return new UserManagerResponseVM
            {
                Message = "Created user faild",
                IsSuccess = false,
                Errors = result.Errors.Select(e => e.Description)
            };


        }


        public async Task<UserManagerResponseVM> LoginUserAsycn(LoginVM model)
        {
            var user=await _userManager.FindByEmailAsync(model.Email);
            if(user == null)
            {
                return new UserManagerResponseVM
                {
                    Message = "Email not found",
                    IsSuccess = false
                };
            }

            var rs = await _userManager.CheckPasswordAsync(user, model.Password);
            if (!rs)
                return new UserManagerResponseVM
                {
                    Message = "Invalid password",
                    IsSuccess = false,
                };

            var claims = new[]
            {
                new Claim("Email",model.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["AuthSettings:Key"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["AuthSetiings:Issuer"],
                audience: _configuration["AuthSettings:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(30),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

            string TokenAsstring=new JwtSecurityTokenHandler().WriteToken(token);
            return new UserManagerResponseVM
            {
                Message = TokenAsstring,
                IsSuccess = true,
                ExpireDate = token.ValidTo
            };

        }
    }
}
