using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using OneSms.Domain;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OneSms.Extensions
{
    public static class IdentityExtensions
    {
        public static async Task SignInUserAsync(this SignInManager<AppUser> signInManager, AppUser user, bool isPersistent, IEnumerable<Claim> customClaims)
        {
            var claimsPrincipal = await signInManager.CreateUserPrincipalAsync(user);
            if (customClaims != null && claimsPrincipal?.Identity is ClaimsIdentity claimsIdentity)
            {
                claimsIdentity.AddClaims(customClaims);
            }
            await signInManager.Context.SignInAsync(IdentityConstants.ApplicationScheme,claimsPrincipal,new AuthenticationProperties { IsPersistent = isPersistent });
        }
    }
}
