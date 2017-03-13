using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using VelocityDb;

namespace VelocityDBExtensions2.AspNet.Identity
{
    public class UserLoginInfoAdapter : OptimizedPersistable
    {
      UserLoginInfo m_lognInfo;
      UInt64 m_userId;
      public UserLoginInfo LoginInfo
      {
        get
        {
          return m_lognInfo;
        }
        set
        {
          Update();
          m_lognInfo = value;
        }
      }

      public UInt64 UserId
      {
        get
        {
          return m_userId;
        }
        set
        {
          Update();
          m_userId = value;
        }
      }
    }
}

