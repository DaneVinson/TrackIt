using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TrackIt.Service.CoreWebApi.Controllers
{
    public class BaseController : Controller
    {
        protected string GetEmailClaimValue()
        {
            return GetClaimValue("emails");
        }

        protected string GetIdClaimValue()
        {
            return GetClaimValue("http://schemas.microsoft.com/identity/claims/objectidentifier");
        }

        protected string GetNameClaimValue()
        {
            return GetClaimValue("name");
        }

        /// <summary>
        /// Get the value of the specifed claims type from User.Claims if available.
        /// Otherwise return null.
        /// </summary>
        private string GetClaimValue(string claimType)
        {
#if DEBUG
            switch (claimType)
            {
                case "emails":
                    return "bilbo.baggins@shire.me";
                case "http://schemas.microsoft.com/identity/claims/objectidentifier":
                    return "537bd906-5a99-413d-823d-f7a4ae36ddfa";  // Bilbo in Azure SQL DB Alpha
                case "name":
                    return "Bilbo";
                default:
                    return null;
            }
#endif
            claimType = (claimType ?? String.Empty).ToLower();
            var claims = User?.Claims;
            if (claims == null) { return null; }
            return claims.FirstOrDefault(c => c.Type.Equals(claimType))?.Value;
        }
    }
}
