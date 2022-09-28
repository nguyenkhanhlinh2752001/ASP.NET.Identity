using Identity.Shared.ViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
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
        private IMailService _mailService;
        public UserService(UserManager<IdentityUser> userManager, IConfiguration configuration, IMailService mailService)
        {
            _userManager = userManager;
            _configuration = configuration;
            _mailService=mailService;
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
                var confirmEmailToken = await _userManager.GenerateEmailConfirmationTokenAsync(identityUser);
                var encodeEmailToken=Encoding.UTF8.GetBytes(confirmEmailToken);
                var validEmailToken=WebEncoders.Base64UrlEncode(encodeEmailToken);

                string url = $"{_configuration["AppUrl"]}/api/auth/confirmemail?userid={identityUser.Id}&token={validEmailToken}";
                await _mailService.SendEmailAsync(identityUser.Email, "Confirm your email",
                    "<h1>Welcome to Auth demo<h1>" + $"<p>please comfirm your email by <a href='{url}'>Click here</a></p>");

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

        public async Task<UserManagerResponseVM> ConfirmEmailAsync(string userId, string token)
        {

            var user=await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new UserManagerResponseVM
                {
                    Message = "User not found",
                    IsSuccess = false
                };
            }

            var decodedToken = WebEncoders.Base64UrlDecode(token);
            string normalToken=Encoding.UTF8.GetString(decodedToken);

            var rs = await _userManager.ConfirmEmailAsync(user, normalToken);

            if (rs.Succeeded)
            {
                return new UserManagerResponseVM
                {
                    Message = "Email confirmed successfully",
                    IsSuccess = true,
                };
            }
            return new UserManagerResponseVM
            {
                Message = "Email confirmed failed",
                IsSuccess = false,
                Errors = rs.Errors.Select(e => e.Description)
            };


        }

        public async Task<UserManagerResponseVM> ForgetPasswordAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return new UserManagerResponseVM
                {
                    Message = "No user associated with this email",
                    IsSuccess = false,
                };
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = Encoding.UTF8.GetBytes(token);
            var validToken=WebEncoders.Base64UrlEncode(encodedToken);

            string url = $"{_configuration["AppUrl"]}/ResetPassword?email={email}&token={validToken}";
            await _mailService.SendEmailAsync(email, "Reset Password",
                "<h1>Follow the instructions to reset your password</h1>"+
                $"<p>To reset your password <a href='{url}'>Click here</a></p>");

            return new UserManagerResponseVM
            {
                Message = "Reset password URL has been sent to your email",
                IsSuccess = true,
            };
        }

        public async Task<UserManagerResponseVM> ResetPasswordAsync(ResetPasswordVM model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return new UserManagerResponseVM
                {
                    Message = "No user associated with this email",
                    IsSuccess = false,
                };
            if (model.NewPassword != model.ConfirmPassword)
                return new UserManagerResponseVM
                {
                    Message = "Password doesn't match its confirmation",
                    IsSuccess = false,
                };
            var rs = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
            if (rs.Succeeded)
                return new UserManagerResponseVM
                {
                    Message = "Password has been reseted successfully",
                    IsSuccess = true,
                };

            return new UserManagerResponseVM
            {
                Message = "Reset password faild",
                IsSuccess = false,
                Errors = rs.Errors.Select(e => e.Description)
            };
        }
    }
}
