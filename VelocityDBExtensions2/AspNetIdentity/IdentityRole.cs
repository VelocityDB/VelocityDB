using System;
using Microsoft.AspNet.Identity;
using VelocityDb;

namespace VelocityDBExtensions2.AspNet.Identity
{
  /// <summary>
  /// Contains properties for roles that can be assigned to a user.
  /// </summary>
  public class IdentityRole : OptimizedPersistable, IRole<UInt64>
  {
    string m_name;

    /// <summary>
    /// Initializes a new instance of the <see cref="IdentityRole"/> class.
    /// </summary>
    public IdentityRole()
    {
    }

    internal IdentityRole(UInt64 id):base(id)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="IdentityRole"/> class.
    /// </summary>
    /// <param name="name">The name.</param>
    public IdentityRole(string name)
    {
      m_name = name;
    }

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    /// <value>
    /// The name.
    /// </value>
    public string Name
    {
      get
      {
        return m_name;
      }
      set
      {
        Update();
        m_name = value;
      }
    }
  }
}
