#if !NET35
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VelocityDb;
using VelocityDb.Session;

namespace MvcMusicStore
{
    #region Documentation
    ///<summary>
    /// Wrapper class that include cool functionalities.
    ///</summary>
    #endregion
    public abstract class VelocityClass : OptimizedPersistable
    {
        protected VelocityClass():base(){}
        protected VelocityClass(ulong id):base(id){}

        public void Save()
        {
            VelocityDB.Session.BeginUpdate();
            {
                this.Save(VelocityDB.Session);
            }
            VelocityDB.Session.Commit();

        }
        public void Save(SessionBase session)
        {
          session.Persist(this);
        }
    }

    public abstract class VelocityClass<T> : VelocityClass where T: VelocityClass
    {
        protected VelocityClass() : base() { }
        protected VelocityClass(ulong id) : base(id) { }

        public static IEnumerable<T> FindAll()
        {
            SessionBase session = VelocityDB.Session;
            return VelocityDB.Session.OfType<T>();
        }

        public static T FindById(ulong id)
        {
            return VelocityDB.Session.OfType<T>().Where(i => i.Id == id).FirstOrDefault();
        }
    }
}
#endif