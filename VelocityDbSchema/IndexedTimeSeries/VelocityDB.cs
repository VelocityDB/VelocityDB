#if !NET35
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VelocityDb.Session;
using System.Configuration;
using System.IO;

namespace VelocityDbSchema.IndexedTimeSeries
{
    #region Documentation
    ///<summary>
    /// Static class that provide a session of velocity to application.
    ///</summary>
    #endregion
    public static class VelocityDBStatic
    {
        public static SessionNoServer Session
        { 
            get;
            private set; 
        }

        static VelocityDBStatic()
        {
            string systemDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "VelocityDB", "Databases", "MvCSample");
            foreach (string f in Directory.EnumerateFiles(systemDir))
              if (f.Contains("4.odb") == false)
                File.Delete(f);
            Session = new SessionNoServer(systemDir);
            Session.BeginUpdate();
            Session.Commit();
        }
    }
}
#endif