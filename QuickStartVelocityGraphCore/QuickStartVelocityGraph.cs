using VelocityGraph.Frontenac.Blueprints.Util.IO.GraphJson;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VelocityDb.Session;
using VelocityGraph;

namespace QuickStartVelocityGraph
{
  class QuickStartVelocityGraph
  {
    static readonly string systemDir = "QuickStartVelocityGraphCore"; // appended to SessionBase.BaseDatabasePath

    static void CreateGraph()
    {
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        if (Directory.Exists(session.SystemDirectory))
          Directory.Delete(session.SystemDirectory, true); // remove systemDir from prior runs and all its databases.
        // Start an update transaction
        session.BeginUpdate();
        Graph g = new Graph(session);
        session.Persist(g);

        // Add a node type for the movies, with a unique identifier and two indexed Propertys
        VertexType movieType = g.NewVertexType("Movie");
        PropertyType movieTitleType = g.NewVertexProperty(movieType, "title", DataType.String, PropertyKind.Indexed);
        PropertyType movieYearType = g.NewVertexProperty(movieType, "year", DataType.Integer, PropertyKind.Indexed);

        // Add a node type for the actor
        VertexType actorType = g.NewVertexType("Actor");
        PropertyType actorNameType = g.NewVertexProperty(actorType, "name", DataType.String, PropertyKind.Indexed);

        // Add a directed edge type with a Property for the cast of a movie
        EdgeType castType = g.NewEdgeType("ACTS_IN", false);
        PropertyType castCharacterType = g.NewEdgeProperty(castType, "role", DataType.String, PropertyKind.Indexed);

        // Add some Movies
        Vertex matrix1 = movieType.NewVertex();
        matrix1.SetProperty(movieTitleType, "The Matrix");
        matrix1.SetProperty(movieYearType, (int)1999);

        Vertex matrix2 = movieType.NewVertex();
        matrix2.SetProperty(movieTitleType, "The Matrix Reloaded");
        matrix2.SetProperty(movieYearType, (int)2003);

        Vertex matrix3 = movieType.NewVertex();
        matrix3.SetProperty(movieTitleType, "The Matrix  Revolutions");
        matrix3.SetProperty(movieYearType, (int)2003);

        // Add some Actors
        Vertex keanu = actorType.NewVertex();
        keanu.SetProperty(actorNameType, "Keanu Reeves");

        Vertex laurence = actorType.NewVertex();
        laurence.SetProperty(actorNameType, "Laurence Fishburne");

        Vertex carrieanne = actorType.NewVertex();
        carrieanne.SetProperty(actorNameType, "Carrie-Anne Moss");

        // Add some edges
        Edge keanuAsNeo = castType.NewEdge(keanu, matrix1);
        keanuAsNeo.SetProperty(castCharacterType, "Neo");
        keanuAsNeo = castType.NewEdge(keanu, matrix2);
        keanuAsNeo.SetProperty(castCharacterType, "Neo");
        keanuAsNeo = castType.NewEdge(keanu, matrix3);
        keanuAsNeo.SetProperty(castCharacterType, "Neo");

        Edge laurenceAsMorpheus = castType.NewEdge(laurence, matrix1);
        laurenceAsMorpheus.SetProperty(castCharacterType, "Morpheus");
        laurenceAsMorpheus = castType.NewEdge(laurence, matrix2);
        laurenceAsMorpheus.SetProperty(castCharacterType, "Morpheus");
        laurenceAsMorpheus = castType.NewEdge(laurence, matrix3);
        laurenceAsMorpheus.SetProperty(castCharacterType, "Morpheus");

        Edge carrieanneAsTrinity = castType.NewEdge(carrieanne, matrix1);
        carrieanneAsTrinity.SetProperty(castCharacterType, "Trinity");
        carrieanneAsTrinity = castType.NewEdge(carrieanne, matrix2);
        carrieanneAsTrinity.SetProperty(castCharacterType, "Trinity");
        carrieanneAsTrinity = castType.NewEdge(carrieanne, matrix3);
        carrieanneAsTrinity.SetProperty(castCharacterType, "Trinity");

        // Commit the transaction
        session.Commit();
      }
    }

    static void QueryGraph()
    {
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        // Start a read only transaction
        session.BeginRead();
        Graph g = Graph.Open(session);
        // Cache SCHEMA
        VertexType movieType = g.FindVertexType("Movie");
        PropertyType movieTitleType = movieType.FindProperty("title");
        VertexType actorType = g.FindVertexType("Actor");
        PropertyType actorNameType = actorType.FindProperty("name");

        // How many vertices do we have?
        Console.WriteLine("Number of Vertices: " + g.CountVertices());

        // Find a movie by name
        Vertex movie = movieTitleType.GetPropertyVertex("The Matrix");

        // Get all actors
        var actors = actorType.GetVertices();

        // Count the actors
        int actorCount = actors.Count();
        Console.WriteLine("Number of Actors: " + actorCount);

        // Get only the actors whose names end with “s”
        foreach (Vertex vertex in actors)
        {
          string actorName = (string) actorNameType.GetPropertyValue(vertex.VertexId);
          if (actorName.EndsWith("s"))
            Console.WriteLine("Found actor with name ending with \"s\" " + actorName);
        }

        // Get a count of property types
        var properties = session.AllObjects<PropertyType>();
        Console.WriteLine("Number of Property types: " + properties.Count());

        // All vertices and their edges
        var edges = g.GetEdges();
        int edgeCount = edges.Count();
        g.ExportToGraphJson("c:/QuickStart.json");
        string exported;
        using (MemoryStream ms = new MemoryStream())
        {
          g.ExportToGraphJson(ms);
          exported = Encoding.UTF8.GetString(ms.ToArray());
        }
        session.Commit();
      }
    }

    static void Main(string[] args)
    {
      CreateGraph();
      QueryGraph();
    }
  }
}
