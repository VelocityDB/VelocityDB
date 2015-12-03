/*
Social graph test
http://odysseas.calit2.uci.edu/doku.php/public:online_social_networks#facebook_social_graph_-_breadth_first_search

Contains a set of unique users on Facebook, and all the userids of their friends. Can be used as a good example for finding shortest path queries.
queries:
- Find Shortest path between two given user ids
- Get all the paths between two user ids
- Get the number of unique 2nd level friends a given user has (friends of my friends)
- Get the top 10 users with most friends
*/

using Frontenac.Blueprints;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VelocityDb;
using VelocityDb.Session;
using VelocityGraph;

namespace FacebookGraph
{
  class FacebookGraph
  {
    static readonly string s_systemDir = "FacebookGraph";
    static readonly string s_inputData = "D:/bfs-28-socialgraph-release-corrected"; // change if you need to, download dataset from http://www.VelocityDB.com/Public/bfs-28-socialgraph-release-corrected.zip (and uncompress with 7-zip http://www.7-zip.org/)
    //static readonly string inputData = "D:\\bfs-28-socialgraph-release";// change if you need to, download dataset from http://odysseas.calit2.uci.edu/doku.php/public:online_social_networks#facebook_social_graph_-_breadth_first_search
    static readonly string s_licenseDbFile = "c:/4.odb";
    enum Gender { Male, Female, Unknown };

    static void Main(string[] args)
    {
      DataCache.MaximumMemoryUse = 12000000000; // 12 GB, set this to what fits your case
      FacebookGraph facebookGraph = new FacebookGraph();
      SessionBase.BaseDatabasePath = "d:/Databases";
      SessionBase.BTreeAddFastTransientBatchSize = 10; // reduces memory usage
      bool import = args.Length > 0 && args[0].ToLower() == "-import";
      bool dirExist = Directory.Exists(Path.Combine(SessionBase.BaseDatabasePath, s_systemDir));
      if (import || !dirExist)
        facebookGraph.ingestData();
      facebookGraph.doQueries();
    }

    public void ingestData()
    {
      if (Directory.Exists(Path.Combine(SessionBase.BaseDatabasePath, s_systemDir)))
        Directory.Delete(Path.Combine(SessionBase.BaseDatabasePath, s_systemDir), true); // remove systemDir from prior runs and all its databases.
      Directory.CreateDirectory(Path.Combine(SessionBase.BaseDatabasePath, s_systemDir));
      File.Copy(s_licenseDbFile, Path.Combine(SessionBase.BaseDatabasePath, s_systemDir, "4.odb"));

      using (SessionNoServer session = new SessionNoServer(s_systemDir, 5000, false, true))
      {
        session.BeginUpdate();
        session.DefaultDatabaseLocation().CompressPages = PageInfo.compressionKind.LZ4;
        Graph g = new Graph(session);

        // SCHEMA
        VertexType userType = g.NewVertexType("User");

        EdgeType friendEdgeType = g.NewEdgeType("Friend", true, userType, userType);

        PropertyType countryProperty = userType.NewProperty("country", DataType.String, PropertyKind.NotIndexed);

        PropertyType incomeProperty = userType.NewProperty("income", DataType.Long, PropertyKind.NotIndexed);

        PropertyType friendshipStartProperty = friendEdgeType.NewProperty("start", DataType.DateTime, PropertyKind.NotIndexed);

        // DATA
        int lineNumber = 0;
        long fiendsCt = 0;
        int stop = (int)Math.Pow(2, 26); // make sure to create enough of these
        for (int i = 1; i < stop; i++)
          g.NewVertex(userType);
        session.Commit();
        session.BeginUpdate();
        foreach (string line in File.ReadLines(s_inputData))
        {
          if (++lineNumber % 10000 == 0)
            Console.WriteLine("Parsing user " + lineNumber + ", friends total: " + fiendsCt + " at " + DateTime.Now);
          string[] fields = line.Split(' ');
          Vertex aUser = null;
          foreach (string s in fields)
          {
            if (s.Length > 0)
            {
              if (aUser == null)
                aUser = new Vertex(g, userType, int.Parse(s));
              else
              {
                ++fiendsCt;
                Vertex aFriend = new Vertex(g, userType, int.Parse(s));
                if (fiendsCt % 2 == 0)
                  aFriend.SetProperty(countryProperty, "Sweden"); // just some random stuff
                else
                  aFriend.SetProperty(incomeProperty, fiendsCt); // just some random stuff
                Edge edge = friendEdgeType.NewEdge(aUser, aFriend);
                if (fiendsCt % 2 == 0)
                  edge.SetProperty(friendshipStartProperty, DateTime.Now);
              }
            }
          }
          if (DataCache.MaximumMemoryUse <= 27000000000)
          {
            if (lineNumber >= 100000) // remove this condition if you have time to wait a long while...
              break;
          }
        }
        Console.WriteLine("Done importing " + lineNumber + " users with " + fiendsCt + " friends");
        session.Commit();
      }
    }

    // Queries
    public void doQueries()
    {
      using (SessionNoServer session = new SessionNoServer(s_systemDir))
      {
        session.BeginRead();
        Graph g = Graph.Open(session); // it takes a while to open graph fresh from databases
        VertexType userType = g.FindVertexType("User");
        EdgeType friendEdgeType = g.FindEdgeType("Friend");
        PropertyType countryProperty = userType.FindProperty("country");
        PropertyType incomeProperty = userType.FindProperty("income");
        PropertyType friendshipStartProperty = friendEdgeType.FindProperty("start");

        Vertex someUser = userType.GetVertex(1);
        Vertex someUser2 = userType.GetVertex(12282);

        var someUserFriends = from Edge e in someUser.GetEdges(friendEdgeType, Direction.Out)
                              select e.Head;
        var someUser3LevelNetwork = someUser.Traverse(3, true, Direction.Out);
        HashSet<Edge> edges = new HashSet<Edge>();
        Edge edge = (Edge)someUser.GetEdges(friendEdgeType, Direction.Out).Skip(1).First();
        edges.Add(edge);
        HashSet<EdgeType> edgeTypesToTraverse = new HashSet<EdgeType>();
        edgeTypesToTraverse.Add(friendEdgeType);
        // Find Shortest path between two given user ids
        List<List<Edge>> path = someUser.Traverse(4, false, Direction.Out, someUser2, edgeTypesToTraverse);
        Debug.Assert(path.Count > 0);
        var path2 = someUser.Traverse(4, false, Direction.Out, userType.GetVertex(41), edgeTypesToTraverse);
        HashSet<Vertex> vertices = new HashSet<Vertex>();
        vertices.Add(someUser2);
        var path3 = someUser.Traverse(4, false, Direction.Out, userType.GetVertex(9465810), edgeTypesToTraverse, vertices); // path must include vertices
        var path3b = someUser.Traverse(4, false, Direction.Out, userType.GetVertex(9465810), edgeTypesToTraverse, null, vertices); // path must NOT include vertices
        var path3c = someUser.Traverse(4, false, Direction.Out, userType.GetVertex(9465810), edgeTypesToTraverse, null, null, edges); // path must include edges
        var path3d = someUser.Traverse(4, false, Direction.Out, userType.GetVertex(9465810), edgeTypesToTraverse, null, null, null, edges); // path must NOT include edges
        HashSet<PropertyType> vertexPropertyTypes = new HashSet<PropertyType>();
        vertexPropertyTypes.Add(incomeProperty);
        vertexPropertyTypes.Add(countryProperty);
        var path3e = someUser.Traverse(4, false, Direction.Out, userType.GetVertex(9465810), edgeTypesToTraverse, null, null, null, edges, vertexPropertyTypes); // path must NOT include edges and at least one vertex in path must have property in propertyTypes
        var path3f = someUser.Traverse(4, false, Direction.Out, userType.GetVertex(9465810), edgeTypesToTraverse, null, null, null, edges, null, vertexPropertyTypes); // path must NOT include edges and no vertex in path must have any property in propertyTypes
        HashSet<PropertyType> edgePropertyTypes = new HashSet<PropertyType>();
        edgePropertyTypes.Add(friendshipStartProperty);
        var path3g = someUser.Traverse(4, false, Direction.Out, userType.GetVertex(9465810), edgeTypesToTraverse, null, null, null, edges, null, null, edgePropertyTypes);
        var path3h = someUser.Traverse(4, false, Direction.Out, userType.GetVertex(9465810), edgeTypesToTraverse, null, null, null, edges, null, null, null, edgePropertyTypes);
        var path3i = someUser.Traverse(4, false, Direction.Out, userType.GetVertex(9465810), edgeTypesToTraverse, null, null, null, edges, null, null, null, null, p => { object pv = p.GetProperty(countryProperty); return pv != null && pv.Equals("Sweden"); });
        var path3j = someUser.Traverse(4, false, Direction.Out, userType.GetVertex(9465810), edgeTypesToTraverse, null, null, null, edges, null, null, null, null, null, p => { DateTime? pv = (DateTime?)p.GetProperty(friendshipStartProperty); return pv != null && pv.Value.CompareTo(DateTime.Now) > 0; });
        var path4 = someUser.Traverse(4, false, Direction.Out, userType.GetVertex(2798), edgeTypesToTraverse);
        var path5 = someUser.Traverse(4, false, Direction.Out, userType.GetVertex(175), edgeTypesToTraverse);
        var path6 = someUser.Traverse(4, false, Direction.Out, userType.GetVertex(1531), edgeTypesToTraverse);
        var path7 = someUser.Traverse(4, false, Direction.Out, userType.GetVertex(1537), edgeTypesToTraverse);
        Console.WriteLine();

        // Get all the paths between two user ids
        var path8 = someUser.Traverse(4, true, Direction.Out, someUser2, edgeTypesToTraverse);
        path = someUser.Traverse(4, true, Direction.Out, userType.GetVertex(41), edgeTypesToTraverse);

        // Get the number of unique 2nd level friends a given user has (friends of my friends)

        var someUsers2ndLevelFriends = (from v in someUserFriends
                                        from Edge e in v.GetEdges(friendEdgeType, Direction.Out)
                                        select e.Head).Distinct();

        Console.WriteLine("unique 2nd level friends a given user has");
        int ct = 0;
        foreach (Vertex v in someUsers2ndLevelFriends)
        {
          if (++ct % 100 == 0)
            Console.WriteLine("User id: " + v.VertexId);
        }
        Console.WriteLine("Some user has: " + ct + " 2nd level friends");
        Console.WriteLine();

        // Get the top 10 users with most friends
        var top10mostFriends = from vertex in userType.GetTopNumberOfEdges(friendEdgeType, 10, Direction.Out)
                               select vertex;
        Console.WriteLine("top 10 users with most friends");
        foreach (Vertex v in top10mostFriends)
        {
          long count = v.GetNumberOfEdges(friendEdgeType, Direction.Out);
          Console.WriteLine("User id: " + v.VertexId + "\t number of friends: " + count);
        }
        Console.WriteLine();
        session.Commit();
      }
    }
  }
}
