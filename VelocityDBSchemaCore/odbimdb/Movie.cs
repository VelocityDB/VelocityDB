using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Collection;
using VelocityDb.Collection.BTree;
using VelocityDb.Session;

namespace VelocityDbSchema.odbimdb
{
  public class Movie : OptimizedPersistable
  {
    public Movie(UInt64 id): base(id)
    {
    }
    public Movie(string title, SessionBase session)
    {
      cast = new BTreeSet<Person>(null, session);
      Title = title;
    }
    BTreeSet<Person> cast;

    public string MovieId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime ReleaseDate { get; set; }

    public BTreeSet<Person> Directors { get; set; }
    public BTreeSet<Person> Cast
    {
      get
      {
        return cast;
      }
    }
    public BTreeSet<Person> Producers { get; set; }
    public BTreeSet<Person> Writers { get; set; }
    public BTreeSet<Person> MusicBy { get; set; }
    public BTreeSet<Person> CastingBy { get; set; }
    public BTreeSet<Person> SpecialEffects { get; set; }
    public BTreeSet<Person> Stunts { get; set; }
    public BTreeSet<Person> CostumeDesignBy { get; set; }

    public VelocityDbList<string> ProductionCompanies { get; set; }
    public VelocityDbList<string> Distributors { get; set; }
    public VelocityDbList<string> SpecialEffectsCompanies { get; set; }
    public VelocityDbList<string> OtherCompanies { get; set; }

    public VelocityDbList<string> Genres { get; set; }
    public VelocityDbList<string> Languages { get; set; }
    public string Rated { get; set; }

    public int Year { get; set; }
    public double UserRating { get; set; }
    public double Votes { get; set; }

    public Image Poster { get; set; }
    public string PosterUrl { get; set; }
    public bool HasTrailer { get; set; }
    public string TrailerUrl { get; set; }

    public VelocityDbList<string> KnownTitles { get; set; }
    public BTreeSet<Movie> RecommendedFilms { get; set; }

    public int Runtime { get; set; }
    public string Tagline { get; set; }

    public bool IsTvSerie { get; set; }
    public VelocityDbList<int> Seasons { get; set; }

    public object Tag { get; set; }
  }
}
