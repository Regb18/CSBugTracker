using System.Security.Claims;
using System.Security.Principal;

namespace CSBugTracker.Extensions
{
    public static class IdentityExtensions
    {
        // GetCompanyId now becomes a part of the identity interface because we're using "this" 
        public static int GetCompanyId(this IIdentity identity)
        {
            // cast identity with claims identity, lets us not interact with IIdentity and allows us to use the Class that implements it
            // 
            Claim claim = ((ClaimsIdentity)identity).FindFirst("CompanyId")!;
            return int.Parse(claim.Value);
        }
    }
}
