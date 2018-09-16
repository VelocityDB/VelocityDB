using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Collection;
using VelocityDb.Collection.BTree;
using VelocityDb.Session;
using VelocityDb.Collection.Comparer;

namespace VelocityDbSchema.Imdb
{
  public abstract class ActingPerson : OptimizedPersistable
  {
    string name;
    public BTreeMap<Movie, string> inMovieAs;

    public ActingPerson(string name, SessionBase session)
    {
      this.name = name;
      inMovieAs = new BTreeMap<Movie, string>(null, session);
    }

    public string Name
    {
      get
      {
        return name;
      }
    }

    public BTreeMap<Movie, string> InMovieAs
    {
      get
      {
        return inMovieAs;
      }
    }
  }
}
