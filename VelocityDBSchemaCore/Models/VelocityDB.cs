#if !NET35
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VelocityDb.Session;
using System.Configuration;
using System.IO;

namespace MvcMusicStore
{
    #region Documentation
    ///<summary>
    /// Static class that provide a session of velocity to application.
    ///</summary>
    #endregion
    public static class VelocityDB
    {
        public static SessionNoServer Session
        { 
            get;
            private set; 
        }

        static VelocityDB()
        {
            string systemDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "VelocityDB", "Databases", "MvCSample");
            if (Directory.Exists(systemDir))
              Directory.Delete(systemDir, true); // remove systemDir from prior runs and all its databases.
            Session = new SessionNoServer(systemDir);
            Session.BeginUpdate();
            Session.Commit();
        }
    }
}
#endif