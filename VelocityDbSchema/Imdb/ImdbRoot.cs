using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Collection.BTree;
using VelocityDb.Session;
using VelocityDb.Collection.Comparer;

namespace VelocityDbSchema.Imdb
{
  public class ImdbRoot : OptimizedPersistable
  {
    BTreeSet<Actor> actorSet;
    BTreeSet<Actress> actressSet;
    BTreeSet<ActingPerson> actingByNameSet;
    BTreeSet<Movie> movieSet;
    HashCodeComparer<string> stringHashComparer = new HashCodeComparer<string>();
    MovieNameHashComparer movieNameHashComparer = new MovieNameHashComparer();
    ActingByNameComparer actingByNameComparer = new ActingByNameComparer();

    public ImdbRoot(SessionBase session)
    {   
      actorSet = new BTreeSet<Actor>(null, session);
      actressSet = new BTreeSet<Actress>(null, session);
      actingByNameSet = new BTreeSet<ActingPerson>(actingByNameComparer, session);
      movieSet = new BTreeSet<Movie>(movieNameHashComparer, session, 10000, sizeof(Int32));
    }

    public BTreeSet<Actor> ActorSet
    {
      get
      {
        return actorSet;
      }
    }

    public BTreeSet<ActingPerson> ActingByNameSet
    {
      get
      {
        return actingByNameSet;
      }
    }

    public BTreeSet<Actress> ActressSet
    {
      get
      {
        return actressSet;
      }
    }
    public BTreeSet<Movie> MovieSet
    {
      get
      {
        return movieSet;
      }
    }

    public HashCodeComparer<string> StringHashComparer
    {
      get
      {
        return stringHashComparer;
      }
    }
  }
}
