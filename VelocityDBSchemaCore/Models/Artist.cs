#if !NET35
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VelocityDb.Session;
using VelocityDb;

namespace MvcMusicStore.Models
{
    public class Artist : VelocityClass<Artist>
    {
        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public Artist() : base() { }

        public static Artist FindByName(string name)
        {
            return (from a in VelocityDB.Session.OfType<Artist>() where a.Name == name select a).FirstOrDefault();
        }
    }
}
#endif