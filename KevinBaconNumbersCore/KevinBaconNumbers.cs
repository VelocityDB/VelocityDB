// This application imports actor, actress and movie data from two compressed text files.
// You can get them from http://www.imdb.com/interfaces#plain, you only need the files: actors.list.gz and actresses.list.gz
// Put them in a folder directly under d: drive as d:\Imdb (or change the path used by this application)
// Alternativly you can download from: http://www.VelocityDB.com/public/actors.list.gz and http://www.VelocityDB.com/public/actresses.list.gz

// This application calculates the degree of seperation of actors and actresses to Kevin Bacon, the connection considered here is only movies that an actor/actress has worked on.
// 0 degrees means you are Kevin Bacon
// 1 degree, you acted in one of Kevin Bacon's movies
// 2 degrees, you acted in a movie with someone who acted in the same movie as Kevin Bacon
// and so on...
// The output looks similar to http://oracleofbacon.org/cgi-bin/center-cgi?who=Kevin+Bacon
// There are some differences but it may be due to us not including TV shows, also I noted some actors/actresses that are listed more than once in the input text files

// How would you do this only using SQL code ??? I bet it wouldn't compute as fast as we do, the entire matrix is calcaulated in about 1 minute.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb.Session;
using VelocityDb;
using System.Threading.Tasks;
using VelocityDbSchema.Imdb;
using System.Threading;
using VelocityDb.Collection.BTree;
using System.IO;

namespace KevinBaconNumbers
{
  class KevinBaconNumbers
  {
    static readonly string s_systemDir = "KevinBaconNumbersCore"; // appended to SessionBase.BaseDatabasePath
    Int32[] resultArray = new Int32[10];
    int bacon = 0;
    HashSet<UInt32> chasedActress = new HashSet<UInt32>();
    HashSet<UInt32> chasedActor = new HashSet<UInt32>();
    HashSet<UInt32> chasedMovie = new HashSet<UInt32>();
    List<ActingPerson> unchasedPerson = new List<ActingPerson>();
    List<Movie> unchasedMovie;

    void processsMovies(SessionBase session)
    {
      UInt32 actorDbNum = session.DatabaseNumberOf(typeof(Actor));
      foreach (Movie movie in unchasedMovie)
      {
        if (chasedMovie.Contains(movie.ShortId) == false)
        {
          chasedMovie.Add(movie.ShortId);
          foreach (ActingPerson acting in movie.Cast)
          {
            if (acting.DatabaseNumber == actorDbNum)
            {
              if (chasedActor.Contains(acting.ShortId) == false)
              {
                unchasedPerson.Add(acting);
                chasedActor.Add(acting.ShortId);
                resultArray[bacon]++;
              }
            }
            else
              if (chasedActress.Contains(acting.ShortId) == false)
              {
                unchasedPerson.Add(acting);
                chasedActress.Add(acting.ShortId);
                resultArray[bacon]++;
              }
          }
        }
      }
      unchasedMovie.Clear();
    }

    void calculateNumbers(SessionBase session)
    {
      ImdbRoot imdbRoot = (ImdbRoot)session.Open(session.DatabaseNumberOf(typeof(ImdbRoot)), 2, 1, false);
      ActingPerson kevin = new Actor("Bacon, Kevin (I)", session);
      if (!imdbRoot.ActingByNameSet.TryGetKey(kevin, ref kevin))
        Console.WriteLine("Couldn't find actor Kevin Bacon!");
      else
      {
        unchasedMovie = kevin.InMovieAs.ToList<Movie>();
        resultArray[bacon]++; // Kevin Bacon himself       
        while (unchasedMovie.Count > 0)
        {
          bacon++;
          processsMovies(session);
          foreach (ActingPerson acting in unchasedPerson)
          {
            foreach (Movie movie in (IEnumerable<Movie>)acting.InMovieAs)
              if (chasedMovie.Contains(movie.ShortId) == false)
                unchasedMovie.Add(movie);
          }
        }
      }
    }

    void printResults()
    {
      int degree = 0;
      foreach (Int32 hit in resultArray)
      {
        Console.WriteLine("Degree " + degree++ + " has # of people: " + hit);
      }
    }

    static void Main(string[] args)
    {
      bool dirExist = Directory.Exists(Path.Combine(SessionBase.BaseDatabasePath, s_systemDir));
      if (!dirExist)
        ImdbImport.ImprortImdb(s_systemDir);
      KevinBaconNumbers kevinBaconNumbers = new KevinBaconNumbers();
      using (SessionNoServer session = new SessionNoServer(s_systemDir))
      {
        try
        {
          session.BeginRead();
          kevinBaconNumbers.calculateNumbers(session);
          kevinBaconNumbers.printResults();
          session.Commit();
        }
        catch (Exception e)
        {
          session.Abort();
          Console.WriteLine(e.ToString());
        }
      }
    }
  }
}
