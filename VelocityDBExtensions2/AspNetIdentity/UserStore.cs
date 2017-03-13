using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using VelocityDb.Session;

namespace VelocityDBExtensions2.AspNet.Identity
{
  /// <summary>
  /// Contains methods for adding, removing, updating and retrieving users from VelocityDB.
  /// </summary>
  /// <typeparam name="T">The <see cref="IdentityUser"/> being retrieved or mutated.</typeparam>
  public class UserStore<T> : IUserLoginStore<T, UInt64>,
      IUserClaimStore<T, UInt64>,
      IUserRoleStore<T, UInt64>,
      IUserSecurityStampStore<T, UInt64>,
      IQueryableUserStore<T, UInt64>,
      IUserPasswordStore<T, UInt64>,
      IUserPhoneNumberStore<T, UInt64>,
      IUserStore<T, UInt64>,
      IUserLockoutStore<T, UInt64>,
      IUserTwoFactorStore<T, UInt64>,
      IUserEmailStore<T, UInt64> where T : IdentityUser
  {
    private readonly AspNetIdentity m_aspNetIdentity;

    public UserStore(AspNetIdentity identity)
    {
      m_aspNetIdentity = identity;
    }

    public void Dispose()
    {
      //Nothing to dispose; VelocityDB dependencies managed at the App level through Global.asax
    }

    /// <summary>
    /// Creates a user.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns></returns>
    /// <exception cref="NullObjectException">If user is null.</exception>
    /// <exception cref="Exception">Any client error condition.</exception>
    public async Task CreateAsync(T user)
    {
      SessionBase session = Session;
      bool inUpdate = session.InUpdateTransaction;
      if (inUpdate == false)
      {
        if (session.InTransaction)
          session.Commit();
        session.BeginUpdate();
      }
      session.Persist(user);
      m_aspNetIdentity.EmailToId.Add(user.Email, user.Id);
      m_aspNetIdentity.UserSet.Add(user);

      if (user.Email != user.UserName)
        m_aspNetIdentity.UserNameToId.Add(user.UserName, user.Id);
      if (inUpdate == false)
        session.Commit();
      await Task.FromResult(true);
    }

    /// <summary>
    /// Updates a user.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException">Any client error condition.</exception>
    public async Task UpdateAsync(T user)
    {
      SessionBase session = Session;
      bool inUpdate = session.InUpdateTransaction;
      if (inUpdate == false)
      {
        if (session.InTransaction)
          session.Commit();
        session.BeginUpdate();
      }
      m_aspNetIdentity.UserSet.Remove(user);
      await Task.FromResult(m_aspNetIdentity.UserSet.Add(user));
      if (inUpdate == false)
        session.Commit();
    }

    /// <summary>
    /// Deletes a user.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException">All server responses other than Success.</exception>
    /// <exception cref="Exception">Any client error condition.</exception>
    public async Task DeleteAsync(T user)
    {
      SessionBase session = Session;
      bool inUpdate = session.InUpdateTransaction;
      if (inUpdate == false)
      {
        if (session.InTransaction)
          session.Commit();
        session.BeginUpdate();
      }
      m_aspNetIdentity.UserNameToId.Remove(user.UserName);
      m_aspNetIdentity.EmailToId.Remove(user.Email);
      await Task.FromResult(m_aspNetIdentity.UserSet.Remove(user));
      if (inUpdate == false)
        session.Commit();
    }

    /// <summary>
    /// Finds the user by it's id (key).
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException">All server responses other than Success.</exception>
    /// <exception cref="Exception">Any client error condition.</exception>
    public async Task<T> FindByIdAsync(UInt64 userId)
    {
      SessionBase session = Session;
      if (session.InTransaction == false)
        session.BeginRead(); 
      IdentityUser user = new IdentityUser(userId);
      if (m_aspNetIdentity.UserSet.TryGetKey(user, ref user))
        return (T)await Task.FromResult(user);
      return null;
    }

    /// <summary>
    /// Finds a user by it's name.
    /// </summary>
    /// <param name="userName">Name of the user.</param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException">All server responses other than Success.</exception>
    /// <exception cref="Exception">Any client error condition that cannot be resolved.</exception>
    public async Task<T> FindByNameAsync(string userName)
    {
      SessionBase session = Session;
      if (session.InTransaction == false)
        session.BeginRead(); 
      UInt64 userId;
      if (m_aspNetIdentity.UserNameToId.TryGetValue(userName, out userId) || m_aspNetIdentity.EmailToId.TryGetValue(userName, out userId))
      {
        IdentityUser user = new IdentityUser(userId);
        if (m_aspNetIdentity.UserSet.TryGetKey(user, ref user))
          return (T)await Task.FromResult(user);
      }
      return default(T);
    }

    /// <summary>
    /// Adds the login to a user.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="login">The login.</param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException">All server responses other than Success.</exception>
    /// <exception cref="Exception">Any client error condition.</exception>
    public async Task AddLoginAsync(T user, UserLoginInfo login)
    {
      var adapter = new UserLoginInfoAdapter
      {
        LoginInfo = login,
        UserId = user.Id
      };
      SessionBase session = Session;
      bool inUpdate = session.InUpdateTransaction;
      if (inUpdate == false)
      {
        if (session.InTransaction)
          session.Commit();
        session.BeginUpdate();
      }
      var key = KeyFormats.GetKey(login.LoginProvider, login.ProviderKey);
      m_aspNetIdentity.AdapterMap.Add(key, adapter);

      user.UserLoginIds.Add(login);
      await Task.FromResult(m_aspNetIdentity.UserSet.Add(user));
      if (inUpdate == false)
        session.Commit();
    }

    /// <summary>
    /// Removes a given login
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="login">The login.</param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException">All server responses other than Success.</exception>
    /// <exception cref="Exception">Any client error condition.</exception>
    public async Task RemoveLoginAsync(T user, UserLoginInfo login)
    {
      SessionBase session = Session;
      bool inUpdate = session.InUpdateTransaction;
      if (inUpdate == false)
      {
        if (session.InTransaction)
          session.Commit();
        session.BeginUpdate();
      }
      string key = KeyFormats.GetKey(login.ProviderKey, user.Id);
      m_aspNetIdentity.AdapterMap.Remove(key);

      user.UserLoginIds.Remove(login);
      await Task.FromResult(m_aspNetIdentity.UserSet.Add(user));
      if (inUpdate == false)
        session.Commit();
    }

    /// <summary>
    /// Gets the login.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns></returns>
    public async Task<IList<UserLoginInfo>> GetLoginsAsync(T user)
    {
      return await Task.FromResult(user.UserLoginIds);
    }

    /// <summary>
    /// Finds the login.
    /// </summary>
    /// <param name="login">The login.</param>
    /// <returns></returns>
    public async Task<T> FindAsync(UserLoginInfo login)
    {
      SessionBase session = Session;
      if (session.InTransaction == false)
        session.BeginRead(); 
      var key = KeyFormats.GetKey(login.LoginProvider, login.ProviderKey);
      var adapter = m_aspNetIdentity.AdapterMap[key];
      IdentityUser user = new IdentityUser(adapter.UserId);
      if (m_aspNetIdentity.UserSet.TryGetKey(user, ref user))
        return (T)await Task.FromResult(user);
      return null;
    }

    /// <summary>
    /// Gets all of the claims for a user.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns></returns>
    public Task<IList<Claim>> GetClaimsAsync(T user)
    {
      return Task.FromResult((IList<Claim>)user.Claims);
    }

    /// <summary>
    /// Adds a claim to a user.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="claim">The claim.</param>
    /// <returns></returns>
    public Task AddClaimAsync(T user, Claim claim)
    {
      SessionBase session = Session;
      bool inUpdate = session.InUpdateTransaction;
      if (inUpdate == false)
      {
        if (session.InTransaction)
          session.Commit();
        session.BeginUpdate();
      }
      user.Claims.Add(claim);
      if (inUpdate == false)
        session.Commit();
      return Task.FromResult(0);
    }

    /// <summary>
    /// Removes a claim from a user.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="claim">The claim.</param>
    /// <returns></returns>
    public Task RemoveClaimAsync(T user, Claim claim)
    {
      SessionBase session = Session;
      bool inUpdate = session.InUpdateTransaction;
      if (inUpdate == false)
      {
        if (session.InTransaction)
          session.Commit();
        session.BeginUpdate();
      }
      user.Claims.Remove(claim);
      if (inUpdate == false)
        session.Commit();
      return Task.FromResult(0);
    }

    /// <summary>
    /// Adds a user to a role.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="roleName">Name of the role.</param>
    /// <returns></returns>
    public Task AddToRoleAsync(T user, string roleName)
    {
      SessionBase session = Session;
      bool inUpdate = session.InUpdateTransaction;
      if (inUpdate == false)
      {
        if (session.InTransaction)
          session.Commit();
        session.BeginUpdate();
      }
      user.Roles.Add(new IdentityRole(roleName));
      if (inUpdate == false)
        session.Commit();
      return Task.FromResult(0);
    }

    /// <summary>
    /// Removes a role from a user.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="roleName">Name of the role.</param>
    /// <returns></returns>
    public Task RemoveFromRoleAsync(T user, string roleName)
    {
      SessionBase session = Session;
      bool inUpdate = session.InUpdateTransaction;
      if (inUpdate == false)
      {
        if (session.InTransaction)
          session.Commit();
        session.BeginUpdate();
      }
      var role = user.Roles.FirstOrDefault(x => x.Name.Equals(roleName));
      if (role != null)
      {
        user.Roles.Remove(role);
      }
      if (inUpdate == false)
        session.Commit();
      return Task.FromResult(0);
    }

    /// <summary>
    /// Gets all of the roles for a user.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns></returns>
    public Task<IList<string>> GetRolesAsync(T user)
    {
      return Task.FromResult((IList<string>)user.Roles.Select(x => x.Name).ToList());
    }

    /// <summary>
    /// Determines whether the user has a role.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="roleName">Name of the role.</param>
    /// <returns></returns>
    public Task<bool> IsInRoleAsync(T user, string roleName)
    {
      SessionBase session = Session;
      if (session.InTransaction == false)
        session.BeginRead(); 
      return Task.FromResult(user.Roles.Any(x => x.Name.Equals(roleName)));
    }

    /// <summary>
    /// Sets the security stamp.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="stamp">The stamp.</param>
    /// <returns></returns>
    public Task SetSecurityStampAsync(T user, string stamp)
    {
      SessionBase session = Session;
      bool inUpdate = session.InUpdateTransaction;
      if (inUpdate == false)
      {
        if (session.InTransaction)
          session.Commit();
        session.BeginUpdate();
      }
      user.SecurityStamp = stamp;
      if (inUpdate == false)
        session.Commit();
      return Task.FromResult(0);
    }

    /// <summary>
    /// Gets the security stamp.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns></returns>
    public Task<string> GetSecurityStampAsync(T user)
    {
      SessionBase session = Session;
      if (session.InTransaction == false)
        session.BeginRead(); 
      return Task.FromResult(user.SecurityStamp);
    }

    public IQueryable<T> Users
    {
      get { throw new NotImplementedException(); }
    }

    /// <summary>
    /// Sets the password hash for a user.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="passwordHash">The password hash.</param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException">All server responses other than Success.</exception>
    /// <exception cref="Exception">Any client error condition.</exception>
    public Task SetPasswordHashAsync(T user, string passwordHash)
    {
      SessionBase session = Session;
      bool inUpdate = session.InUpdateTransaction;
      if (inUpdate == false)
      {
        if (session.InTransaction)
          session.Commit();
        session.BeginUpdate();
      }
      user.PasswordHash = passwordHash;
      if (inUpdate == false)
        session.Commit();
      return Task.FromResult(0);
    }

    /// <summary>
    /// Gets the password hash for a user.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns></returns>
    public Task<string> GetPasswordHashAsync(T user)
    {
      return Task.FromResult(user.PasswordHash);
    }

    /// <summary>
    /// Determines whether a user has a password hash.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns></returns>
    public Task<bool> HasPasswordAsync(T user)
    {
      return Task.FromResult(!string.IsNullOrWhiteSpace(user.PasswordHash));
    }

    /// <summary>
    /// Sets the phone number for a user.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="phoneNumber">The phone number.</param>
    /// <returns></returns>
    /// <exception cref="NullObjectException">If user is null.</exception>
    /// <exception cref="Exception">Any client error condition.</exception>
    public Task SetPhoneNumberAsync(T user, string phoneNumber)
    {
      SessionBase session = Session;
      bool inUpdate = session.InUpdateTransaction;
      if (inUpdate == false)
      {
        if (session.InTransaction)
          session.Commit();
        session.BeginUpdate();
      }
      user.PhoneNumber = phoneNumber;
      if (inUpdate == false)
        session.Commit();
      return Task.FromResult(0);
    }

    /// <summary>
    /// Gets the phone number for a user.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns></returns>
    public Task<string> GetPhoneNumberAsync(T user)
    {
      return Task.FromResult(user.PhoneNumber);
    }

    /// <summary>
    /// Gets the <see cref="IdentityUser.PhoneNumberConfirmed"/> for a user.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns></returns>
    public Task<bool> GetPhoneNumberConfirmedAsync(T user)
    {
      return Task.FromResult(user.PhoneNumberConfirmed);
    }

    /// <summary>
    /// Sets the <see cref="IdentityUser.PhoneNumberConfirmed"/> for a user.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="confirmed">if set to <c>true</c> [confirmed].</param>
    /// <returns></returns>
    /// <exception cref="NullObjectException">If user is null.</exception>
    /// <exception cref="Exception">Any client error condition.</exception>
    public Task SetPhoneNumberConfirmedAsync(T user, bool confirmed)
    {
      SessionBase session = Session;
      bool inUpdate = session.InUpdateTransaction;

      user.PhoneNumberConfirmed = confirmed;
      if (inUpdate == false)
        session.Commit();
      return Task.FromResult(0);
    }

    /// <summary>
    /// Gets the lockout end date for a user.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns></returns>
    public Task<DateTimeOffset> GetLockoutEndDateAsync(T user)
    {
      var lockoutEndDateUtc = user.LockoutEndDateUtc;
      return Task.FromResult(lockoutEndDateUtc.HasValue ?
          new DateTimeOffset(DateTime.SpecifyKind(lockoutEndDateUtc.Value, DateTimeKind.Utc)) :
          new DateTimeOffset());
    }

    /// <summary>
    /// Sets the lockout end date.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="lockoutEnd">The lockout end.</param>
    /// <returns></returns>
    /// <exception cref="NullObjectException">If user is null.</exception>
    /// <exception cref="Exception">Any client error condition.</exception>
    public Task SetLockoutEndDateAsync(T user, DateTimeOffset lockoutEnd)
    {
      SessionBase session = Session;
      bool inUpdate = session.InUpdateTransaction;
      if (inUpdate == false)
      {
        if (session.InTransaction)
          session.Commit();
        session.BeginUpdate();
      }
      user.LockoutEndDateUtc = lockoutEnd.UtcDateTime;
      if (inUpdate == false)
        session.Commit();
      return Task.FromResult(0);
    }

    /// <summary>
    /// Increments the access failed count for a user.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns></returns>
    /// <exception cref="NullObjectException">If user is null.</exception>
    /// <exception cref="Exception">Any client error condition.</exception>
    public async Task<int> IncrementAccessFailedCountAsync(T user)
    {
      SessionBase session = Session;
      bool inUpdate = session.InUpdateTransaction;
      if (inUpdate == false)
      {
        if (session.InTransaction)
          session.Commit();
        session.BeginUpdate();
      }
      user.AccessFailedCount = user.AccessFailedCount + 1;
      if (inUpdate == false)
        session.Commit();
      return await Task.FromResult(user.AccessFailedCount);
    }

    /// <summary>
    /// Resets the access failed count.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns></returns>
    /// <exception cref="NullObjectException">If user is null.</exception>
    /// <exception cref="Exception">Any client error condition.</exception>
    public async Task ResetAccessFailedCountAsync(T user)
    {
      await Task.FromResult(user.AccessFailedCount = 0);
    }

    /// <summary>
    /// Gets the access failed count.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns></returns>
    public Task<int> GetAccessFailedCountAsync(T user)
    {
      return Task.FromResult(user.AccessFailedCount);
    }

    /// <summary>
    /// Gets the lockout enabled flag for a user.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns></returns>
    public Task<bool> GetLockoutEnabledAsync(T user)
    {
      return Task.FromResult(user.LockoutEnabled);
    }

    /// <summary>
    /// Sets the lockout enabled flag for a user.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="enabled">if set to <c>true</c> [enabled].</param>
    /// <returns></returns>
    /// <exception cref="NullObjectException">If user is null.</exception>
    /// <exception cref="Exception">Any client error condition.</exception>
    public Task SetLockoutEnabledAsync(T user, bool enabled)
    {
      SessionBase session = Session;
      bool inUpdate = session.InUpdateTransaction;
      if (inUpdate == false)
      {
        if (session.InTransaction)
          session.Commit();
        session.BeginUpdate();
      }
      user.LockoutEnabled = enabled;
      if (inUpdate == false)
        session.Commit();
      return Task.FromResult(0);
    }

    /// <summary>
    /// Sets a flag indicating two factor authentication is enabled for a user.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="enabled">if set to <c>true</c> <see cref="IdentityUser.TwoFactorEnabled"/>enabled.</param>
    /// <returns></returns>
    /// <exception cref="NullObjectException">If user is null.</exception>
    /// <exception cref="Exception">Any client error condition.</exception>
    public Task SetTwoFactorEnabledAsync(T user, bool enabled)
    {
      SessionBase session = Session;
      bool inUpdate = session.InUpdateTransaction;
      if (inUpdate == false)
      {
        if (session.InTransaction)
          session.Commit();
        session.BeginUpdate();
      }
      user.TwoFactorEnabled = enabled;
      if (inUpdate == false)
        session.Commit();
      return Task.FromResult(0);
    }

    /// <summary>
    /// Gets a flag indicating two factor authentication is enabled for a user.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns></returns>
    public Task<bool> GetTwoFactorEnabledAsync(T user)
    {
      return Task.FromResult(user.TwoFactorEnabled);
    }

    /// <summary>
    /// Sets the user's email.
    /// </summary>
    /// <param name="user">The user's email.</param>
    /// <param name="email">The email to set to the user's identity.</param>
    /// <returns></returns>
    /// <exception cref="NullObjectException">If user is null.</exception>
    /// <exception cref="Exception">Any client error condition.</exception>
    public Task SetEmailAsync(T user, string email)
    {
      SessionBase session = Session;
      bool inUpdate = session.InUpdateTransaction;
      if (inUpdate == false)
      {
        if (session.InTransaction)
          session.Commit();
        session.BeginUpdate();
      }
      user.Email = email;
      if (inUpdate == false)
        session.Commit();
      return Task.FromResult(0);
    }

    /// <summary>
    /// Gets the user's email.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns></returns>
    public Task<string> GetEmailAsync(T user)
    {
      return Task.FromResult(user.Email);
    }

    /// <summary>
    /// Gets the confirmed status of the user's email.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns></returns>
    public Task<bool> GetEmailConfirmedAsync(T user)
    {
      return Task.FromResult(user.EmailConfirmed);
    }

    /// <summary>
    /// Sets the user's email confirmed flag.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="confirmed">if set to <c>true</c> the user's email has been confirmed.</param>
    /// <returns></returns>
    /// <exception cref="NullObjectException">if user is null.</exception>
    /// <exception cref="Exception">Any client error condition.</exception>
    public Task SetEmailConfirmedAsync(T user, bool confirmed)
    {
      SessionBase session = Session;
      bool inUpdate = session.InUpdateTransaction;
      if (inUpdate == false)
      {
        if (session.InTransaction)
          session.Commit();
        session.BeginUpdate();
      }
      user.EmailConfirmed = confirmed;
      if (inUpdate == false)
        session.Commit();
      return Task.FromResult(0);
    }

    /// <summary>
    /// Finds the user by email.
    /// </summary>
    /// <param name="email">The email.</param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException">If email isn't a valid key.</exception>
    /// <exception cref="Exception">Any client error condition.</exception>
    public async Task<T> FindByEmailAsync(string email)
    {
      SessionBase session = Session;
      if (session.InTransaction == false)
        session.BeginRead(); 
      UInt64 userId;
      if (m_aspNetIdentity.EmailToId.TryGetValue(email, out userId))
      {
        IdentityUser user = new IdentityUser(userId);
        if (m_aspNetIdentity.UserSet.TryGetKey(user, ref user))
          return (T)await Task.FromResult(user);
      }
      return null;
    }

    public SessionBase Session
    {
      get
      {
        return m_aspNetIdentity.Session;
      }
    }
  }
}
