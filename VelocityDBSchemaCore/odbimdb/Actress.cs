using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb.Collection;
using VelocityDb.Session;

namespace VelocityDbSchema.odbimdb
{
  public class Actress : Woman
  {
    public Actress(string name, SessionBase session)
      : base(name, session)
    {
    }
  }
}
