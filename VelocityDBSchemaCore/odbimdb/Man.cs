using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb.Session;

namespace VelocityDbSchema.odbimdb
{
  public class Man : Person
  {
    public Man(string name, SessionBase session) : base(name, session) { }
  }
}
