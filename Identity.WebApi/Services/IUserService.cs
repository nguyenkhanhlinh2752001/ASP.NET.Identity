using Identity.Shared.ViewModel;

namespace Identity.WebApi.Services
{
    public interface IUserService
    {
        Task<UserManagerResponseVM> RegisterUserAsycn(RegisterVM model);
    }
}
