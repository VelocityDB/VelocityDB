using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb.Collection;
using VelocityDb.Session;
using VelocityDb.Collection.Comparer;

namespace VelocityDbSchema.Imdb
{
  public class Actress : ActingPerson
  {
    public Actress(string name, SessionBase session)
      : base(name, session)
    {
    }
  }
}
