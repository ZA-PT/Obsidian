using System.Threading.Tasks;
using Obsidian.Domain;

namespace Obsidian.Application.Services
{
    public interface IIdentityService
    {
        Task CookieSignInAsync(string scheme, User user, bool isPersistent);

        Task CookieSignOutCurrentUserAsync(string scheme);

        Task<User> GetCurrentUserAsync();

        Task ChangeCurrentUserPasswordAsync(string oldPassword,string newPassword);

        Task SetCurrentUsernameAsync(string newUsername);

        Task SetCurrentUserProfile(UserProfile profile);
    }
}