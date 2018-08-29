using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace RasmedTestEngine.Core.Entities
{
    public class Member : IdentityUser
    {
        public virtual MemberProfile MemberProfile { get; set; }

        public virtual ICollection<Examination> Examinations { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<Member> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
                ClaimsIdentity userIdentity =
                await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }
}