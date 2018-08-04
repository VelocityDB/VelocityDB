using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb.Session;
using VelocityDbSchema.Imdb;
using System.IO;
using System.IO.Compression;
using VelocityDb;
using System.Text.RegularExpressions;

namespace KevinBaconNumbers
{
  class ImdbImport
  {
    static readonly string imdbTextFilesDir = "c:/SampleData/imdb"; // change if you need to
    static readonly char[] trimEndChars = new char[] { ';', '.', '"', ',', '\r', ':', ':', ']', '!', '?', '+', '(', '\'', '{', '}', '-', ' ' };

    void parseMovie(SessionBase session, string line, ImdbRoot imdbRoot, ActingPerson acting)
    {
      line = new string(line.SkipWhile(aChar => aChar == '\t').ToArray<char>()); // skip any leading tabs
      string movieName = new string(line.TakeWhile(aChar => aChar != '(').ToArray<char>()); // start of year
      if (movieName.Length == 0 || movieName[0] != '\"') // then it is a TV series - skipping for now
      {
        line = line.Substring(movieName.Length + 1);
        string yearString = new string(line.TakeWhile(aChar => aChar != ')').ToArray<char>()); // end of year
        bool unknownYear = yearString == "????";
        bool notEndOfMovieName = (yearString.Length < 4 || (yearString.Length > 4 && yearString[4] != '/') || Char.IsNumber(yearString[0]) == false || Char.IsNumber(yearString[1]) == false) && unknownYear == false;
        while (notEndOfMovieName)
        {
          movieName += "(";
          string extendedName = new string(line.TakeWhile(aChar => aChar != '(').ToArray<char>());
          movieName += extendedName;
          line = line.Substring(extendedName.Length + 1);
          yearString = new string(line.TakeWhile(aChar => aChar != ')').ToArray<char>()); // end of year
          unknownYear = yearString == "????";
          notEndOfMovieName = (yearString.Length < 4 || (yearString.Length > 4 && yearString[4] != '/') || Char.IsNumber(yearString[0]) == false || Char.IsNumber(yearString[1]) == false) && unknownYear == false;
        }
        movieName = movieName.TrimEnd(trimEndChars);    
        line = line.Substring(yearString.Length + 1);
        yearString = new string(yearString.TakeWhile(aChar => aChar != '/').ToArray<char>()); // skip year string like 2010/I
        Int16 year;
        if (unknownYear)
          year = 0;
        else
          year = Int16.Parse(yearString);
        line = new string(line.SkipWhile(aChar => aChar != '(' && aChar != '[').ToArray<char>()); // start of role
        bool video = line.Length > 1 && line[0] == '(' && line[1] == 'V';
        bool tv = line.Length > 2 && line[0] == '(' && line[1] == 'T' && line[2] == 'V';
        if (tv == false && video == false)
        {
          string role = null;
          if (line.Length > 1 && line[0] == '[')
          {
            line = line.Substring(1);
            role = new string(line.TakeWhile(aChar => aChar != ']').ToArray<char>()); // end of role
          }
          Movie movie = new Movie(movieName, year, session);
          if (!imdbRoot.MovieSet.TryGetKey(movie, ref movie))
          {
            session.Persist(movie);
            imdbRoot.MovieSet.Add(movie);
          }
          if (role != null)
            acting.InMovieAs.Add(movie, role);
          movie.Cast.Add(acting);
        }
      }
    }

    void ParseActors(SessionBase session, ImdbRoot imdbRoot)
    {
      using (FileStream stream = File.OpenRead(System.IO.Path.Combine(imdbTextFilesDir, "actors.list.gz")))
      {
        using (GZipStream decompress = new GZipStream(stream, CompressionMode.Decompress))
        {
          using (System.IO.StreamReader file = new System.IO.StreamReader(decompress))
          {
            string line;
            int lineNumber = 0;
            while ((line = file.ReadLine()) != null)
            { // skip all the intro stuff
              lineNumber++;
              if (line.Length > 5 && line[0] == '-' && line[5] == '\t')
                break;
            }
            while ((line = file.ReadLine()) != null)
            {
              lineNumber++;
              string actorName = new string(line.TakeWhile(aChar => aChar != '\t').ToArray<char>()); // end of name
              if (line.Length > 10 && line[0] == '-' && line[1] == '-' && line[2] == '-' && line[3] == '-')
                break; // signals end of input
              line = line.Substring(actorName.Length + 1);
              Actor actor = new Actor(actorName, session);
              session.Persist(actor);
              imdbRoot.ActorSet.Add(actor);
              parseMovie(session, line, imdbRoot, actor);
              while ((line = file.ReadLine()) != null)
              {
                if (line.Length == 0)
                  break;
                lineNumber++;
                parseMovie(session, line, imdbRoot, actor);
              }
            }
          }
        }
      }
    }

    void ParseActresses(SessionBase session, ImdbRoot imdbRoot)
    {
      using (FileStream stream = File.OpenRead(System.IO.Path.Combine(imdbTextFilesDir, "actresses.list.gz")))
      {
        using (GZipStream decompress = new GZipStream(stream, CompressionMode.Decompress))
        {
          using (System.IO.StreamReader file = new System.IO.StreamReader(decompress))
          {
            string line;
            int lineNumber = 0;
            while ((line = file.ReadLine()) != null)
            { // skip all the intro stuff
              lineNumber++;
              if (line.Length > 5 && line[0] == '-' && line[5] == '\t')
                break;
            }
            while ((line = file.ReadLine()) != null)
            {
              lineNumber++;
              string actorName = new string(line.TakeWhile(aChar => aChar != '\t').ToArray<char>()); // end of name
              if (line.Length > 10 && line[0] == '-' && line[1] == '-' && line[2] == '-' && line[3] == '-')
                break; // signals end of input
              line = line.Substring(actorName.Length + 1);
              Actress actress = new Actress(actorName, session);
              session.Persist(actress);
              imdbRoot.ActressSet.Add(actress);
              parseMovie(session, line, imdbRoot, actress);
              while ((line = file.ReadLine()) != null)
              {
                if (line.Length == 0)
                  break;
                lineNumber++;
                parseMovie(session, line, imdbRoot, actress);
              }
            }
          }
        }
      }
    }

    public static void ImprortImdb(string systemDir)
    {
      ImdbImport imdbImport = new ImdbImport();
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        try
        {
          session.BeginUpdate();
          ImdbRoot imdbRoot = new ImdbRoot(session);
          session.Persist(imdbRoot);
          imdbImport.ParseActors(session, imdbRoot);
          imdbImport.ParseActresses(session, imdbRoot);
          foreach (ActingPerson acting in imdbRoot.ActorSet)
            if (!imdbRoot.ActingByNameSet.Add(acting))
              Console.WriteLine("Dublicate ActingPerson found (in ActorSet): " + acting.Name);          
          foreach (ActingPerson acting in imdbRoot.ActressSet)
            if (!imdbRoot.ActingByNameSet.Add(acting))
              Console.WriteLine("Dublicate ActingPerson found (in ActressSet): " + acting.Name);
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
