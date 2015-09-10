
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using VelocityDb.Session;

namespace VelocityDB.AspNet.Identity
{

  /// <summary>
  /// Contains methods for creating, deleting, updating and retrieving roles.
  /// </summary>
  /// <typeparam name="T">The <see cref="IdentityRole"/> being retrieved or mutated.</typeparam>
  public class RoleStore<T> : IQueryableRoleStore<T, UInt64> where T : IdentityRole
  {
    private AspNetIdentity m_aspNetIdentity;

    /// <summary>
    /// Initializes a new instance of the <see cref="RoleStore{T}"/> class.
    /// </summary>
    /// <param name="identity">The identity.</param>
    public RoleStore(AspNetIdentity identity)
    {
      m_aspNetIdentity = identity;
    }

    public SessionBase Session
    {
      get
      {
        return m_aspNetIdentity.Session;
      }
    }

    public IQueryable<T> Roles
    {
      get { throw new System.NotImplementedException(); }
    }

    /// <summary>
    /// Creates a role as an asynchronous operation.
    /// </summary>
    /// <param name="role">The role.</param>
    /// <returns></returns>
    /// <exception cref="VelocityDBAspNetIdentityException"></exception>
    public async Task CreateAsync(T role)
    {
      SessionBase session = Session;
      bool inUpdate = session.InUpdateTransaction;
      if (inUpdate == false)
      {
        if (session.InTransaction)
          session.Commit();
        session.BeginUpdate();
      }
      m_aspNetIdentity.RoleSet.Add(role);
      if (inUpdate == false)
        session.Commit();
      await Task.FromResult(true);
    }

    /// <summary>
    /// Deletes a role as an asynchronous operation.
    /// </summary>
    /// <param name="role">The role.</param>
    /// <returns></returns>
    /// <exception cref="VelocityDBAspNetIdentityException"></exception>
    public async Task DeleteAsync(T role)
    {
      SessionBase session = Session;
      bool inUpdate = session.InUpdateTransaction;
      if (inUpdate == false)
      {
        if (session.InTransaction)
          session.Commit();
        session.BeginUpdate();
      }
      await Task.FromResult(m_aspNetIdentity.RoleSet.Remove(role));
      if (inUpdate == false)
        session.Commit();
    }

    /// <summary>
    /// Finds the role by it's unique identifier (key).
    /// </summary>
    /// <param name="roleId">The key for the role.</param>
    /// <returns></returns>
    /// <exception cref="VelocityDBAspNetIdentityException"></exception>
    public async Task<T> FindByIdAsync(UInt64 roleId)
    {
      SessionBase session = Session;
      if (session.InTransaction == false)
        session.BeginRead(); 
      IdentityRole role = new IdentityRole(roleId);
      if (m_aspNetIdentity.RoleSet.TryGetKey(role, ref role))
        return (T)await Task.FromResult(role);
      return null;
    }

    /// <summary>
    /// Finds the role by it's name.
    /// </summary>
    /// <param name="roleName">Name of the role.</param>
    /// <returns>An <see cref="IdentityRole"/> matching the name property.</returns>
    /// <exception cref="VelocityDBAspNetIdentityException"></exception>
    /// <remarks>Finds and returns the first valid match. If no match is found, will throw a <see cref="CouchbaseException"/>.</remarks>
    public async Task<T> FindByNameAsync(string roleName)
    {
      SessionBase session = Session;
      if (session.InTransaction == false)
        session.BeginRead(); 
      var roles = m_aspNetIdentity.Session.AllObjects<IdentityRole>();
      return (T)await Task.FromResult((from role in roles where role.Name == roleName select role).FirstOrDefault());
    }

    /// <summary>
    /// Updates the role by it's unique identifier (key).
    /// </summary>
    /// <param name="role">The role.</param>
    /// <returns></returns>
    /// <exception cref="VelocityDBAspNetIdentityException"></exception>
    public async Task UpdateAsync(T role)
    {
      SessionBase session = Session;
      bool inUpdate = session.InUpdateTransaction;
      if (inUpdate == false)
      {
        if (session.InTransaction)
          session.Commit();
        session.BeginUpdate();
      }
      m_aspNetIdentity.RoleSet.Remove(role);
      await Task.FromResult(m_aspNetIdentity.RoleSet.Add(role));
      if (inUpdate == false)
        session.Commit();
    }

    public void Dispose()
    {
      GC.SuppressFinalize(this);
    }
  }
}

