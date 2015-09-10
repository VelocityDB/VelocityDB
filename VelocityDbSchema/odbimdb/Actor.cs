using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Collection;
using VelocityDb.Session;

namespace VelocityDbSchema.odbimdb
{
  public class Actor : Man
  {
    public Actor(string name, SessionBase session) : base(name, session)
    {
    }
  }
}
