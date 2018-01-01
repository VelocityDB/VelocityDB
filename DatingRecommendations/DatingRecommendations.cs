/*
Dating Recommendations
http://www.occamslab.com/petricek/data/

Dating site with ratings. Can be used to recommend other people a user might like.
17 million ratings, 168 000 profiles that are rated by 135 000 users.

Complex queries
-	Given a user id, and based on the rated profiles, find other users who have rated similarly on the same profiles, and find other profiles to recommend based on what those other users have rated
-	Get all female users with less than 50 ratings
-	Get all male users with at least one 10 rating
-	Get the first 10 male users who have rated at least 3 of the same profiles as the given user.

Statistics queries
-	Get the 20 profiles with the most ratings
-	Get the 20 best rated profiles regardless of gender
-	Get the 20 best rated males
-	Get the 20 best rated females
*/

using System;
using System.IO;
using System.Linq;
using VelocityDb.Session;
using VelocityGraph;
using Frontenac.Blueprints;
using System.Collections.Generic;
using VelocityDb;

namespace DatingRecommendations
{

  class DatingRecommendations
  {
    static readonly string s_systemDir = SessionBase.BaseDatabasePath + "/DatingRecommendations";
    static readonly string s_inputDataDir = "c:/SampleData/libimseti"; // change if you need to, download dataset from http://www.occamslab.com/petricek/data/
    static readonly string s_licenseDbFile = "c:/4.odb";
    static readonly int s_highestRating = 10;
    enum Gender { Male, Female, Unknown };

    static void Main(string[] args)
    {
      bool import = args.Length > 0 && args[0].ToLower() == "-import";
      bool dirExist = Directory.Exists(s_systemDir);
      SessionBase.ClearAllCachedObjectsWhenDetectingUpdatedDatabase = false;
      if (import || !dirExist)
      {
        if (dirExist)
          Directory.Delete(s_systemDir, true); // remove systemDir from prior runs and all its databases.
        Directory.CreateDirectory(s_systemDir);
        File.Copy(s_licenseDbFile, Path.Combine(s_systemDir, "4.odb"));
        using (SessionNoServer session = new SessionNoServer(s_systemDir))
        {
          DataCache.MaximumMemoryUse = 12000000000; // 12 GB, set this to what fits your case
          SessionBase.BTreeAddFastTransientBatchSize = 10; // reduces memory usage
          Vertex[] ratingVertices = new Vertex[10];
          session.BeginUpdate();
          session.DefaultDatabaseLocation().CompressPages = PageInfo.compressionKind.LZ4;
          Graph g = new Graph(session);
  
          // SCHEMA
          VertexType userType = g.NewVertexType("User");
          PropertyType genderType = userType.NewProperty("Gender", DataType.Integer, PropertyKind.Indexed);

          VertexType ratingType = g.NewVertexType("Rating");
          PropertyType ratingValuePropertyType = ratingType.NewProperty("RatingValue", DataType.Integer, PropertyKind.Indexed);

          EdgeType ratingEdgeType = g.NewEdgeType("UserToRating", true, userType, ratingType);

          EdgeType ratingOfType = g.NewEdgeType("RatingOf", false, userType, userType);
          PropertyType ratingEdgePropertyType = ratingOfType.NewProperty("Rating", DataType.Integer, PropertyKind.Indexed);

          // DATA
          using (FileStream stream = File.OpenRead(System.IO.Path.Combine(s_inputDataDir, "gender.dat")))
          {
            using (StreamReader file = new System.IO.StreamReader(stream))
            {
              string line;
              int lineNumber = 0;
              while ((line = file.ReadLine()) != null)
              {
                lineNumber++;
                string[] fields = line.Split(',');
                Vertex aUser = userType.NewVertex();
                aUser.SetProperty(genderType, (int)fields[1][0] == 'M' ? Gender.Male : fields[1][0] == 'F' ? Gender.Female : Gender.Unknown);
              }
              Console.WriteLine("Done importing " + lineNumber + " users");
            }
          }

          using (FileStream stream = File.OpenRead(System.IO.Path.Combine(s_inputDataDir, "ratings.dat")))
          {
            using (StreamReader file = new System.IO.StreamReader(stream))
            {
              string line;
              int lineNumber = 0;
              Vertex rater = null;
              int raterId;
              int priorRaterId = -1;

              while ((line = file.ReadLine()) != null)
              {
                lineNumber++;
                if (lineNumber % 1000000 == 0)
                  Console.WriteLine("Parsing rating # " + lineNumber);
                string[] fields = line.Split(',');
                raterId = int.Parse(fields[0]);
                if (raterId != priorRaterId)
                  rater = userType.GetVertex(raterId);
                priorRaterId = raterId;
                int ratedId = int.Parse(fields[1]);
                int rating = int.Parse(fields[2]);
                Vertex ratingVertex = ratingVertices[rating - 1];
                if (ratingVertex == null)
                {
                  ratingVertex = ratingType.NewVertex();
                  ratingVertex.SetProperty(ratingValuePropertyType, rating);
                  ratingVertices[rating - 1] = ratingVertex;
                }
                Vertex rated = userType.GetVertex(ratedId);
                Edge aRatingOf = ratingOfType.NewEdge(rater, rated);
                aRatingOf.SetProperty(ratingEdgePropertyType, rating);
                Edge userRating = ratingEdgeType.NewEdge(rated, ratingVertex);
              }
              Console.WriteLine("Done importing " + lineNumber + " ratings");
            }
          }
          session.Commit();
        }
      }
      // Query
      using (SessionNoServer session = new SessionNoServer(s_systemDir))
      {
        session.BeginRead();
        Graph g = Graph.Open(session);
        VertexType userType = g.FindVertexType("User");
        PropertyType genderType = userType.FindProperty("Gender");

        VertexType ratingType = g.FindVertexType("Rating");
        PropertyType ratingValuePropertyType = ratingType.FindProperty("RatingValue");

        EdgeType ratingEdgeType = g.FindEdgeType("UserToRating");

        EdgeType ratingOfType = g.FindEdgeType("RatingOf");
        PropertyType ratingEdgePropertyType = ratingOfType.FindProperty("Rating");
        // Complex queries
        int ct = 0;

// Given a user id, and based on the rated profiles, find other users who have rated similarly on the same profiles, and find other profiles to recommend based on what those other users have rated
        Vertex someUser = userType.GetVertex(1);
        var similarRatings = (from Edge e in someUser.GetEdges(ratingOfType, Direction.Out)
                              from Edge edge in e.Head.GetEdges(ratingOfType, Direction.Out)
                              where someUser != edge.Head
                              where ((int)e.GetProperty(ratingEdgePropertyType) == (int)edge.GetProperty(ratingEdgePropertyType))
                              select edge.Tail).Distinct();

        var someUserRated = from Edge e in someUser.GetEdges(ratingOfType, Direction.Out)
                            select e.Head;

        var recommendedProfles = from v in similarRatings
                                 from Edge e in v.GetEdges(ratingOfType, Direction.Out)
                                 where someUserRated.Contains(e.Head) == false
                                 select e.Head;

        Console.WriteLine("Some user's rated profiles");
        ct = 0;
        foreach (Vertex v in someUserRated)
        {
          if (ct++ % 50 == 0) // don't print them all !
            Console.WriteLine("User id: " + v.VertexId);
        }
        Console.WriteLine("Number of some user's rated profiles: " + ct);

        Console.WriteLine("Given a user id, and based on the rated profiles, find other users who have rated similarly on the same profiles");
        ct = 0;
        foreach (Vertex v in similarRatings)
        {
          if (ct++ % 50 == 0) // don't print them all !
            Console.WriteLine("User id: " + v.VertexId);
        }
        Console.WriteLine("Number of matching profiles: " + ct);

        Console.WriteLine("Given a user id, and based on the rated profiles, find other users who have rated similarly on the same profiles, and find other profiles to recommend based on what those other users have rated");
        ct = 0;
        foreach (Vertex v in recommendedProfles)
        {
          if (ct++ % 5000 == 0) // don't print them all !
            Console.WriteLine("User id: " + v.VertexId);
        }
        Console.WriteLine("Number of matching profiles: " + ct);

// Get all female users with less than 50 ratings
        Console.WriteLine();
        var females = from u in userType.GetVertices()
                      where ((Gender)u.GetProperty(genderType)) == Gender.Female
                      select u;
        var femalesLess50Ratings = from f in females
                                   where f.GetNumberOfEdges(ratingEdgeType, Direction.Out) < 50
                                   select f;
        Console.WriteLine("Female users with less than 50 ratings");
        ct = 0;
        foreach (Vertex f in femalesLess50Ratings)
        {
          long count = f.GetNumberOfEdges(ratingEdgeType, Direction.Out);
          if (ct++ % 5000 == 0) // don't print them all !
            Console.WriteLine("User id: " + f.VertexId + "\tnumber of ratings: " + count);
        }
        Console.WriteLine("Number of females with fewer than 50 ratings: " + ct);
        Console.WriteLine();

// Get all male users with at least one 10 rating
        Console.WriteLine();
        var rated10vertex = (from v in ratingType.GetVertices()
                             where ((int)v.GetProperty(ratingValuePropertyType)) == 10
                             select v).First();

        var rated10 = (from e in rated10vertex.GetEdges(ratingEdgeType, Direction.In)
                       select e.GetVertex(Direction.Out)).Distinct();

        var rated10male = from Vertex v in rated10
                          where ((Gender)v.GetProperty(genderType)) == Gender.Male
                          select v;

        Console.WriteLine("Males with at least one 10 rating");
        ct = 0;
        foreach (Vertex v in rated10male)
        {
          if (ct++ % 5000 == 0) // don't print them all !
            Console.WriteLine("User id: " + v.VertexId);
        }
        Console.WriteLine("Number of males with at least one 10 rating: " + ct);
        Console.WriteLine();

// Get the first 10 male users who have rated at least 3 of the same profiles as the given user.
        Console.WriteLine("10 male users who have rated at least 3 of the same profiles as the given user");
        var males = from u in userType.GetVertices()
                    where ((Gender)u.GetProperty(genderType)) == Gender.Male
                    select u;

        var someUserHasRated = from Edge o in someUser.GetEdges(ratingOfType, Direction.Out)
                               select o.Head;

        var first10withSame3ratedAs = from m in males
                                      where (from Edge r in m.GetEdges(ratingOfType, Direction.Out) select r.Head).Intersect(someUserHasRated).ToArray().Length >= 3
                                      select m;
        ct = 0;
        foreach (Vertex v in first10withSame3ratedAs)
        {
          if (++ct > 10)
            break;
          Console.WriteLine("User id: " + v.VertexId);
        }
        Console.WriteLine();

        // Statistical queries
// Get the 20 profiles with the most ratings
        var top20mostRatings = (from v in userType.GetVertices()
                               orderby v.GetNumberOfEdges(ratingEdgeType, Direction.Out) descending
                               select v).Take(20);
        Console.WriteLine("20 profiles with the most ratings");
        ct = 0;
        foreach (Vertex v in top20mostRatings)
        {
          int count = (int)v.GetNumberOfEdges(ratingEdgeType, Direction.Out);  
          Console.WriteLine("User id: " + v.VertexId + "\tnumber of ratings: " + count);
        }
        Console.WriteLine();

        var ratingsVertexEnum = from v in ratingType.GetVertices() orderby v.GetProperty(ratingValuePropertyType) descending select v;
        Vertex rating10Vertex = ratingsVertexEnum.First();
// Get the 20 best rated profiles regardless of gender
        var top = from u in userType.GetVertices()
                  let edgeCt = u.GetNumberOfEdges(ratingEdgeType, rating10Vertex, Direction.Out)
                  orderby edgeCt descending
                  select new { u, edgeCt };
                   
        Console.WriteLine("20 best rated profiles regardless of gender");
        ct = 0;
        foreach (var v in top)
        {
          if (++ct > 20)
            break;
          Console.WriteLine("User id: " + v.u.VertexId + "\t10 ratings: " + v.edgeCt);
        }
        Console.WriteLine();

// Get the 20 best rated males
        Console.WriteLine("20 best rated male profiles");
        var top20males = from u in userType.GetVertices()
                         where ((Gender)u.GetProperty(genderType)) == Gender.Male
                         let edgeCt = u.GetNumberOfEdges(ratingEdgeType, rating10Vertex, Direction.Out)
                         orderby edgeCt descending
                         select new { u, edgeCt };

        ct = 0;
        foreach (var v in top20males)
        {
          if (++ct > 20)
            break;
          Console.WriteLine("Male User id: " + v.u.VertexId + " \t10 ratings: " + v.edgeCt);
        }
        Console.WriteLine();

// Get the 20 best rated females
        Console.WriteLine("20 best rated female profiles");
        var top20females = from u in userType.GetVertices()
                           where ((Gender)u.GetProperty(genderType)) == Gender.Female
                           let edgeCt = u.GetNumberOfEdges(ratingEdgeType, rating10Vertex, Direction.Out)
                           orderby edgeCt descending
                           select new { u, edgeCt };
        ct = 0;
        foreach (var v in top20females)
        {
          if (++ct > 20)
            break;
          Console.WriteLine("Female User id: " + v.u.VertexId + "\t10 ratings: " + v.edgeCt);
        }
        session.Commit();
      }
    }
  }
}
