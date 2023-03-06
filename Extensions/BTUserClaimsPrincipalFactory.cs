using CSBugTracker.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace CSBugTracker.Extensions
{
    public class BTUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<BTUser, IdentityRole>
    {
        public BTUserClaimsPrincipalFactory(UserManager<BTUser> userManager,
                                            RoleManager<IdentityRole> roleManager,
                                            IOptions<IdentityOptions> optionsAccessor)
            : base(userManager, roleManager, optionsAccessor)
            // passes parameters along to the parent as well
        {
        }

        // protected - only this class and the child classes can use this method, nothing outside this 
        // virtual keyword allows for override, entity framework is allowing for us to use virtual and it changes things on the fly 
        // GenerateClaimsAsync is the only method from the parent we can override because it's the only virtual one
        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(BTUser user)
        {
            ClaimsIdentity identity = await base.GenerateClaimsAsync(user);
            
            // adds a company ID to our identity, so we can get the ID from any User now 
            identity.AddClaim(new Claim("CompanyId", user.CompanyId.ToString()));
            
            return identity;
        }

    }
}
