using Identity.Shared.ViewModel;

namespace Identity.WebApi.Services
{
    public interface IUserService
    {
        Task<UserManagerResponseVM> RegisterUserAsycn(RegisterVM model);
        Task<UserManagerResponseVM> LoginUserAsycn(LoginVM model);
        Task<UserManagerResponseVM> ConfirmEmailAsync(string userId, string token);
    }
}
