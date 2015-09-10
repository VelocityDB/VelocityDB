// This sample is a preview of VelocityGraph, help us define the best possible graph api! The start of this api is inspired by Dex, see http://www.sparsity-technologies.com/dex.php,
// the sample here mimics what the dex sample is doing. Dex is considered by some to have the best performing graph api currently available. Unlike Dex, which is a C++/C implemented api,
// VelocityGraph is all implemented in C# and enable any type of Element values. VelocityGraph is provided as open source on GitHub, https://github.com/VelocityDB/VelocityGraph. 
// and implements the graph API Blueprints standard interfaces as provided in https://github.com/Loupi/Frontenac
// Anyone is welcome to contribute! VelocityGraph is built on top of VelocityDB.
// You will need a VelocityDB license to use VelocityGraph. Eventually VelocityGraph may have its own web site, http://www.VelocityGraph.com/

using System;
using System.IO;
using System.Linq;
using VelocityDb.Session;
using VelocityGraph;

namespace VelocityGraphSample
{
  using Frontenac.Blueprints;
  using System.Collections.Generic;
  class VelocityGraphSample
  {
    static readonly string systemDir = "VelocityGraphSample"; // appended to SessionBase.BaseDatabasePath

    static void Main(string[] args)
    {
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        if (Directory.Exists(session.SystemDirectory))
          Directory.Delete(session.SystemDirectory, true); // remove systemDir from prior runs and all its databases.
        Console.WriteLine("Running with databases in directory: " + session.SystemDirectory);
        session.BeginUpdate();
        Graph g = new Graph(session);
        session.Persist(g);

        // SCHEMA
        // Add a node type for the movies, with a unique identifier and two indexed Propertys
        VertexType movieType = g.NewVertexType("MOVIE");
        PropertyType movieTitleType = g.NewVertexProperty(movieType, "TITLE", DataType.String, PropertyKind.Indexed);
        PropertyType movieYearType = g.NewVertexProperty(movieType, "YEAR", DataType.Integer, PropertyKind.Indexed);

        // Add a node type for the people, with a unique identifier and an indexed Property
        VertexType peopleType = g.NewVertexType("PEOPLE");
        PropertyType peopleNameType = g.NewVertexProperty(peopleType, "NAME", DataType.String, PropertyKind.Indexed);

        // Add an undirected edge type with a Property for the cast of a movie
        EdgeType castType = g.NewEdgeType("CAST", false);
        PropertyType castCharacterType = g.NewEdgeProperty(castType, "CHARACTER", DataType.String, PropertyKind.Indexed);

        // Add a directed edge type restricted to go from people to movie for the director of a movie
        EdgeType directsType = g.NewEdgeType("DIRECTS", true, peopleType, movieType);

        // DATA
        // Add some MOVIE nodes

        Vertex mLostInTranslation = movieType.NewVertex();
        mLostInTranslation.SetProperty(movieTitleType, "Lost in Translation");
        mLostInTranslation.SetProperty(movieYearType, (int)2003);

        Vertex mVickyCB = movieType.NewVertex();
        mVickyCB.SetProperty(movieTitleType, "Vicky Cristina Barcelona");
        mVickyCB.SetProperty(movieYearType, (int)2008);

        Vertex mManhattan = movieType.NewVertex();
        mManhattan.SetProperty(movieTitleType, "Manhattan");
        mManhattan.SetProperty(movieYearType, (int)1979);

        // Add some PEOPLE nodes
        Vertex pScarlett = peopleType.NewVertex();
        pScarlett.SetProperty(peopleNameType, "Scarlett Johansson");

        Vertex pBill = peopleType.NewVertex();
        pBill.SetProperty(peopleNameType, "Bill Murray");

        Vertex pSofia = peopleType.NewVertex();
        pSofia.SetProperty(peopleNameType, "Sofia Coppola");

        Vertex pWoody = peopleType.NewVertex();
        pWoody.SetProperty(peopleNameType, "Woody Allen");

        Vertex pPenelope = peopleType.NewVertex();
        pPenelope.SetProperty(peopleNameType, "Penélope Cruz");

        Vertex pDiane = peopleType.NewVertex();
        pDiane.SetProperty(peopleNameType, "Diane Keaton");

        // Add some CAST edges
        Edge anEdge;
        anEdge = g.NewEdge(castType, mLostInTranslation, pScarlett);
        anEdge.SetProperty(castCharacterType, "Charlotte");

        anEdge = g.NewEdge(castType, mLostInTranslation, pBill);
        anEdge.SetProperty(castCharacterType, "Bob Harris");

        anEdge = g.NewEdge(castType, mVickyCB, pScarlett);
        anEdge.SetProperty(castCharacterType, "Cristina");

        anEdge = g.NewEdge(castType, mVickyCB, pPenelope);
        anEdge.SetProperty(castCharacterType, "Maria Elena");

        anEdge = g.NewEdge(castType, mManhattan, pDiane);
        anEdge.SetProperty(castCharacterType, "Mary");

        anEdge = g.NewEdge(castType, mManhattan, pWoody);
        anEdge.SetProperty(castCharacterType, "Isaac");

        // Add some DIRECTS edges
        anEdge = g.NewEdge(directsType, pSofia, mLostInTranslation);
        anEdge = g.NewEdge(directsType, pWoody, mVickyCB);
        anEdge = g.NewEdge(directsType, pWoody, mManhattan);

        // QUERIES
        // Get the movies directed by Woody Allen
        Dictionary<Vertex, HashSet<Edge>> directedByWoody = pWoody.Traverse(directsType, Direction.Out);

        // Get the cast of the movies directed by Woody Allen
        Dictionary<Vertex, HashSet<Edge>> castDirectedByWoody = g.Traverse(directedByWoody.Keys.ToArray(), castType, Direction.Both);

        // Get the movies directed by Sofia Coppola
        Dictionary<Vertex, HashSet<Edge>> directedBySofia = pSofia.Traverse(directsType, Direction.Out);

        // Get the cast of the movies directed by Sofia Coppola
        Dictionary<Vertex, HashSet<Edge>> castDirectedBySofia = g.Traverse(directedBySofia.Keys.ToArray(), castType, Direction.Both);

        // We want to know the people that acted in movies directed by Woody AND in movies directed by Sofia.
        IEnumerable<Vertex> castFromBoth = castDirectedByWoody.Keys.Intersect(castDirectedBySofia.Keys);

        // Say hello to the people found
        foreach (Vertex person in castFromBoth)
        {
          object value = person.GetProperty(peopleNameType);
          System.Console.WriteLine("Hello " + value);
        }

        var billM = g.Traverse(directedBySofia.Keys.ToArray(), castType, Direction.Both).Keys.Where(vertex => vertex.GetProperty(peopleNameType).Equals("Bill Murray"));

        // Say hello to Bill Murray
        foreach (Vertex person in billM)
        {
          object value = person.GetProperty(peopleNameType);
          System.Console.WriteLine("Hello " + value);
        }

        session.Commit();
      }

      using (ServerClientSession session = new ServerClientSession(systemDir, System.Net.Dns.GetHostName()))
      {
        session.BeginRead();
        Graph g = Graph.Open(session);
        VertexType movieType = g.FindVertexType("MOVIE");
        PropertyType movieTitleProperty = g.FindVertexProperty(movieType, "TITLE");
        Vertex obj = g.FindVertex(movieTitleProperty, "Manhattan");
        session.Commit();
      }
    }
  }
}
