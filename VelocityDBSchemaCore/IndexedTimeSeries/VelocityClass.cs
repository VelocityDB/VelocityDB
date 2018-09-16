using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VelocityDb;
using VelocityDb.Session;

namespace VelocityDbSchema.IndexedTimeSeries
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
          VelocityDBStatic.Session.BeginUpdate();
            {
              this.Save(VelocityDBStatic.Session);
            }
            VelocityDBStatic.Session.Commit();

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
          SessionBase session = VelocityDBStatic.Session;
          return VelocityDBStatic.Session.OfType<T>();
        }

        public static T FindById(ulong id)
        {
          return VelocityDBStatic.Session.OfType<T>().Where(i => i.Id == id).FirstOrDefault();
        }
    }
}
