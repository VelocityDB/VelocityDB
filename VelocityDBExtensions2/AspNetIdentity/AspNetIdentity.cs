using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using VelocityDb;
using VelocityDb.Collection.BTree;
using VelocityDb.Session;

namespace VelocityDB.AspNet.Identity
{
  public class AspNetIdentity : OptimizedPersistable
  {
    string m_name;
    BTreeMap<string, UserLoginInfoAdapter> m_adapterMap;
    BTreeSet<IdentityUser> m_userSet;
    BTreeSet<IdentityRole> m_roleSet;
    BTreeMap<string, UInt64> m_emailToId;
    BTreeMap<string, UInt64> m_userNameToId;

    public AspNetIdentity(SessionBase session)
    {
      m_adapterMap = new BTreeMap<string, UserLoginInfoAdapter>(null, session);
      m_userSet = new BTreeSet<IdentityUser>(null, session);
      m_roleSet = new BTreeSet<IdentityRole>(null, session);
      m_emailToId = new BTreeMap<string, ulong>(null, session);
      m_userNameToId = new BTreeMap<string, ulong>(null, session);
    }

    public BTreeMap<string, UserLoginInfoAdapter> AdapterMap
    {
      get
      {
        return m_adapterMap;
      }
    }

    public BTreeSet<IdentityRole> RoleSet
    {
      get
      {
        return m_roleSet;
      }
    }
    public BTreeSet<IdentityUser> UserSet
    {
      get
      {
        return m_userSet;
      }
    }

    public BTreeMap<string, UInt64> EmailToId
    {
      get
      {
        return m_emailToId;
      }
    }
    public BTreeMap<string, UInt64> UserNameToId
    {
      get
      {
        return m_userNameToId;
      }
    }

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

    public override UInt64 Persist(Placement place, SessionBase session, bool persistRefs = true, bool disableFlush = false, Queue<IOptimizedPersistable> toPersist = null)
    {
      if (IsPersistent == false)
      {
        session.RegisterClass(typeof(AspNetIdentity));
        session.RegisterClass(typeof(UserLoginInfoAdapter));
        session.RegisterClass(typeof(IdentityUser));
        session.RegisterClass(typeof(IdentityRole));
        session.RegisterClass(typeof(BTreeMap<string, UserLoginInfoAdapter>));
        session.RegisterClass(typeof(BTreeMap<string, UInt64>));
        session.RegisterClass(typeof(BTreeSet<IdentityUser>));
        session.RegisterClass(typeof(BTreeSet<IdentityRole>));
        return base.Persist(place, session, persistRefs, disableFlush, toPersist);
      }
      return Id;
    }
  }
}
