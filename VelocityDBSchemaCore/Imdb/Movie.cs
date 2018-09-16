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
  public class Movie : OptimizedPersistable
  {
    string m_title;
    BTreeSet<ActingPerson> m_cast;
    Int16 m_year;

    public Movie(UInt64 id): base(id)
    {
    }

    public Movie(string title, Int16 year, SessionBase session)
    {
      m_cast = new BTreeSet<ActingPerson>(null, session);
      m_title = title;
      m_year = year;
    }
    
    public string Title
    {
      get
      {
        return m_title;
      }
    }

    public Int16 ReleaseYear
    {
      get
      {
        return m_year;
      }
    }

    public BTreeSet<ActingPerson> Cast
    {
      get
      {
        return m_cast;
      }
    }

    public override CacheEnum Cache
    {
      get
      {
        return CacheEnum.is64Bit;
      }
    }
  }
}
