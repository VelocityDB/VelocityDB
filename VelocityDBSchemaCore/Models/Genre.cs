#if !NET35
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VelocityDb;
using VelocityDb.Session;

namespace MvcMusicStore.Models
{
    public class Genre : VelocityClass<Genre>
    {
        #region Fields

        private string name;

        private string description;
        #endregion

        #region Properties
        public string Name { get { return this.name; } set { this.name = value; } }
        public string Description { get { return this.description; } set { this.description = value; } }
        #endregion

        #region Constructors
        public Genre()
            : base()
        { }

        #endregion

        public static Genre FindByName(string name)
        {
            return (from g in VelocityDB.Session.OfType<Genre>() where g.Name == name select g).FirstOrDefault();
        }

        public IEnumerable<Album> Albums()
        {
            return Album.FindByGenre(this);
        }
    }
}
#endif