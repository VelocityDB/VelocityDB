using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Session;

namespace VelocityDbSchema.odbimdb
{
  public class Woman : Person
  {
    public Woman(string name, SessionBase session) : base(name, session) { }
  }
}
