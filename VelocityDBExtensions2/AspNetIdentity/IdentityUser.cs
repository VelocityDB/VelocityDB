using System;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNet.Identity;
using VelocityDb;
using VelocityDb.Collection;

namespace VelocityDB.AspNet.Identity
{
    /// <summary>
    /// Represents a user on the system and properties associated with that user.
    /// </summary>
  public class IdentityUser : OptimizedPersistable, IUser<UInt64>
    {
      VelocityDbList<UserLoginInfo> m_userLoginIds;
      VelocityDbList<IdentityRole> m_roles;
      VelocityDbList<Claim> m_claims;
      string m_userName;
      string m_email;
      bool m_emailConfirmed;
      string m_passwordHash;
      string m_phoneNumber;
      bool m_phoneNumberConfirmed;
      DateTime? m_lockoutEndDateUtc;
      bool m_lockoutEnabled;
      int m_accessFailedCount;
      bool m_twoFactorEnabled;
      string m_securityStamp;

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityUser"/> class.
        /// </summary>
        public IdentityUser()
        {
          m_userLoginIds = new VelocityDbList<UserLoginInfo>();
          m_roles = new VelocityDbList<IdentityRole>();
          m_claims = new VelocityDbList<Claim>();
          m_lockoutEndDateUtc = null;
        }

        internal IdentityUser(UInt64 id):base(id)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityUser"/> class.
        /// </summary>
        /// <param name="username">The username.</param>
        public IdentityUser(string username): this()
        {
          m_userName = username;
        }

        /// <summary>
        /// Gets or sets the name of the user.
        /// </summary>
        /// <value>
        /// The name of the user.
        /// </value>
        public string UserName
        {
          get
          {
            return m_userName;
          }
          set
          {
            Update();
            m_userName = value;
          }
        }

        /// <summary>
        /// Gets or sets the email for the user.
        /// </summary>
        /// <value>
        /// The email.
        /// </value>
        public string Email
        {
          get
          {
            return m_email;
          }
          set
          {
            Update();
            m_email = value;
          }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the user's email has been confirmed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the user's email has been confirmed; otherwise, <c>false</c>.
        /// </value>
        public bool EmailConfirmed
        {
          get
          {
            return m_emailConfirmed;
          }
          set
          {
            Update();
            m_emailConfirmed = value;
          }
        }

        /// <summary>
        /// Gets or sets the password hash for the user.
        /// </summary>
        /// <value>
        /// The password hash.
        /// </value>
        public string PasswordHash
        {
          get
          {
            return m_passwordHash;
          }
          set
          {
            Update();
            m_passwordHash = value;
          }
        }

        /// <summary>
        /// Gets or sets the phone number for the user.
        /// </summary>
        /// <value>
        /// The phone number.
        /// </value>
        public string PhoneNumber
        {
          get
          {
            return m_phoneNumber;
          }
          set
          {
            Update();
            m_phoneNumber = value;
          }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="PhoneNumber"/> was confirmed.
        /// </summary>
        /// <value>
        /// <c>true</c> if the  <see cref="PhoneNumber"/>confirmed; otherwise, <c>false</c>.
        /// </value>
        public bool PhoneNumberConfirmed
        {
          get
          {
            return m_phoneNumberConfirmed;
          }
          set
          {
            Update();
            m_phoneNumberConfirmed = value;
          }
        }

        /// <summary>
        /// DateTime in UTC when lockout ends, any time in the past is considered not locked out.
        /// </summary>
        public DateTime? LockoutEndDateUtc
        {
          get
          {
            return m_lockoutEndDateUtc;
          }
          set
          {
            Update();
            m_lockoutEndDateUtc = value;
          }
        }

        /// <summary>
        /// Is lockout enabled for this user
        /// </summary>
        public bool LockoutEnabled
        {
          get
          {
            return m_lockoutEnabled;
          }
          set
          {
            Update();
            m_lockoutEnabled = value;
          }
        }

        /// <summary>
        /// Used to record failures for the purposes of lockout
        /// </summary>
        public int AccessFailedCount
        {
          get
          {
            return m_accessFailedCount;
          }
          set
          {
            Update();
            m_accessFailedCount = value;
          }
        }

        /// <summary>
        /// Gets or sets a value indicating whether two factor authentication is enabled for a user.
        /// </summary>
        /// <value>
        ///   <c>true</c> if two factor authentication is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool TwoFactorEnabled
        {
          get
          {
            return m_twoFactorEnabled;
          }
          set
          {
            Update();
            m_twoFactorEnabled = value;
          }
        }

        /// <summary>
        /// Gets or sets the user login infos.
        /// </summary>
        /// <value>
        /// The user login infos.
        /// </value>
        public IList<UserLoginInfo> UserLoginIds
        {
          get
          {
            return m_userLoginIds;
          }
          set
          {
            m_userLoginIds.Clear();
            foreach (UserLoginInfo info in value)
              m_userLoginIds.Add(info);
          }
        }

        /// <summary>
        /// Gets or sets the security stamp.
        /// </summary>
        /// <value>
        /// The security stamp.
        /// </value>
        public string SecurityStamp
        {
          get
          {
            return m_securityStamp;
          }
          set
          {
            Update();
            m_securityStamp = value;
          }
        }

        /// <summary>
        /// Gets or sets the roles the user belongs to.
        /// </summary>
        /// <value>
        /// The roles.
        /// </value>
        public ICollection<IdentityRole> Roles
        {
          get
          {
            return m_roles;
          }
          set
          {
            m_roles.Clear();
            foreach (IdentityRole role in value)
              m_roles.Add(role);
          }
        }

        /// <summary>
        /// Gets or sets the claims for the user.
        /// </summary>
        /// <value>
        /// The claims.
        /// </value>
        public ICollection<Claim> Claims
        {
          get
          {
            return m_claims;
          }
          set
          {
            m_claims.Clear();
            foreach (Claim claim in value)
              m_claims.Add(claim);
          }
        }
    }
}