using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using VelocityDBExtensions2.AspNet.Identity;
using VelocityDb.Session;
using System;

namespace AspNetIdentitySample.Models
{
  // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
  public class ApplicationUser : IdentityUser
  {
    public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser, UInt64> manager)
    {
      // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
      var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
      // Add custom user claims here
      return userIdentity;
    }
  }

  public class ApplicationDbContext: SessionNoServer
  {
    AspNetIdentity m_aspNetIdentity;
    public ApplicationDbContext():base("AspNetIdentitySample")
    {
      BeginUpdate();
      m_aspNetIdentity = AllObjects<AspNetIdentity>().FirstOrDefault();
      if (m_aspNetIdentity == null)
      {
        m_aspNetIdentity = new AspNetIdentity(this);
        Persist(m_aspNetIdentity);
      }
      Commit();
    }

    public static ApplicationDbContext Create()
    {
      return new ApplicationDbContext();
    }

    public AspNetIdentity AspNetIdentity
    {
      get
      {
        return m_aspNetIdentity;
      }
    }
  }
}