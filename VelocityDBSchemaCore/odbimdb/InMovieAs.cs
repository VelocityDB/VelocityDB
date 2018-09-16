using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Session;

namespace VelocityDbSchema.odbimdb
{
  public class InMovieAs : OptimizedPersistable
  {
    string character;
    Movie movie;

    public InMovieAs(string asCharater, Movie inMovie)
    {
      character = asCharater;
      movie = inMovie;
    }

    public InMovieAs(string asCharater, UInt64 oid)
    {
      character = asCharater;
      movie = new Movie(oid);
    }

    public string Character
    {
      get
      {
        return character;
      }
    }

    public Movie InMovie
    {
      get
      {
        return movie;
      }
    }
  }
}
