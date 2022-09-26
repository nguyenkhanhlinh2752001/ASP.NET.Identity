using Identity.Shared.ViewModel;
using Microsoft.AspNetCore.Identity;

namespace Identity.WebApi.Services
{
    public class UserService : IUserService
    {
        private UserManager<IdentityUser> _userManager;
        public UserService(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
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
    }
}
