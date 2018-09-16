// This sample is inspired by http://www.vertica.com/2011/09/21/counting-triangles/ where the Vertica Database is comparing itself with Hadoop and PIG
// We were not able to run the Vertica, Hadoop or PIG solution yet due to build issues (missing build.xml).
// Note that VelocityDB only needs about 387MB to store the data for this while Hadoop is claimed to need 160GB !!
// This same test was also performed using Oracle on a very expensive system, see http://structureddata.org/2011/10/17/counting-triangles-faster/
// After the Oracle publication came Kognitio which claims to beat the other Databases, see http://insurancepressrelease.com/2011/10/28/kognitio-beats-vertica-oracle-hadoop-and-pig-on-query-performance/

// We ran VelocityDB on a Gateway SX2800 C2Q Desktop PC purchased refurbished for $329 in 2009.
// Using this low end hardware with 32 bit Windows 7 and 4GB memory we most likly outperform all the of the Databases.
// Some of them probably would even fail if attempting to run on this low end PC.
// Using the Gateway PC, VelocityDB counts the triangles between the 4846609 nodes in test data in about 125 seconds plus 30 seconds for loading the database using 4 threads.
// We will test on a big server ASAP, we are confident we'll beat everyone else when utilizing 96GB memory and 12 cores (cpu's).

// The other databases are: Kognitio, Oracle, Vertica, Hadoop and PIG.

// Download the raw data (~1.3 GB) for this test from http://www.vertica.com/benchmark/TriangleCounting/edges.txt.gz
// Put the edges.txt in c:\edges.txt or edit the code to match your path.

using System;
using System.Runtime;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VelocityDb;
using VelocityDb.Collection.BTree;
using VelocityDb.Session;
using System.Runtime.InteropServices;

namespace TriangleCounter
{
  class TriangleCounter
  {
    static readonly string systemDir = "TriangleCounterCore"; // appended to SessionBase.BaseDatabasePath for a full path
    static readonly string edgesInputFile = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "c:/SampleData/edges.txt" : "/SampleData/edges.txt";

    static int discoverTrianglesSingleCore(BTreeMap<int, int[]> edges)
    {
      int triangles = 0;
      foreach (KeyValuePair<int, int[]> pair in edges)
      {
        int nodeId = pair.Key;
        int[] edge = pair.Value;
        int stop = edge.Length - 1;
        int i = stop;
        int edgeToStart, edgeTo;
        int pos;
        while (i >= 0)
        {
          int[] edgeInfo2;
          edgeToStart = edge[i--];
          if (nodeId < edgeToStart)
          {
            if (edges.TryGetValue(edgeToStart, out edgeInfo2))
            {
              for (int j = stop; j >= i; j--)
              {
                edgeTo = edge[j];
                if (edgeToStart < edgeTo)
                {
                  pos = Array.BinarySearch<int>(edgeInfo2, edgeTo);
                  if (pos >= 0)
                  { // we know this one is connected to edgeInfo.From because it is part of edgeInfo.To
                    triangles++;
                  }
                }
                else
                  break;
              }
            }
          }
          else
            break;
        }
      }
      return triangles;
    }

    // Please help, I have not figured out how to properly do the triangle query using LINQ
    static int queryUsingLINQ(BTreeMap<int, int[]> edges)
    {
      int[] edge2values = null;

      int r = (from KeyValuePair<int, int[]> edge in edges
               from int edgeTo1 in edge.Value 
               where edge.Key < edgeTo1
               from int edgeTo2 in edge.Value
               where edge.Key < edgeTo2 && edgeTo2 > edgeTo1 && edges.TryGetValue(edgeTo1, out edge2values) && Array.BinarySearch(edge2values, edgeTo2) >= 0
               select edge).Count();
      return r;
    }

    static void Main(string[] args)
    {
      long triangles = 0;
      try
      {
        using (SessionNoServer session = new SessionNoServer(systemDir))
        {
          int numberOfWorkerThreads = -1;
          if (args.Length > 0)
          {
            if (!int.TryParse(args[0], out numberOfWorkerThreads))
              Console.WriteLine("First parameter is numberOfWorkerThreads which must be an Int32");
          }
          bool useLinq = args.Length > 1;
          session.BeginUpdate();
          BTreeMapIterator<int, int[]> edgesItr;
          int[] edge = null;
          BTreeMap<int, int[]> edges = session.AllObjects<BTreeMap<int, int[]>>(false).FirstOrDefault();
          if (edges != null)
          {
            session.Commit();
            session.BeginRead();
          }
          else
          {
            DatabaseLocation location = session.DatabaseLocations.Default();
            edges = new BTreeMap<int, int[]>(null, session, 6000);
            session.Persist(edges);
            edgesItr = edges.Iterator();
            using (StreamReader stream = new StreamReader(edgesInputFile, true))
            {
              int a;
              int b;
              string line;
              string[] fields;
              while ((line = stream.ReadLine()) != null)
              {
                fields = line.Split(' ');
                if (!int.TryParse(fields[0], out a))
                  break;
                b = int.Parse(fields[1]);
                if (a != b)
                {
                  if (edgesItr.CurrentKey() == a || edgesItr.GoTo(a))
                  {
                    edge = edgesItr.CurrentValue();
                    Array.Resize(ref edge, edge.Length + 1);
                    edge[edge.Length - 1] = b;
                    edgesItr.ReplaceValue(ref edge); // we need to update the value in the BTreeMap
                  }
                  else
                  {
                    edge = new int[1];
                    edge[0] = b;
                    edges.Add(a, edge);
                  }
                }
              }
            }
            edgesItr = edges.Iterator();
            while (edgesItr.MoveNext())
            {
              edge = edgesItr.CurrentValue();
              Array.Sort(edge);
              edgesItr.ReplaceValue(ref edge);
            }
            session.Commit();
            session.BeginRead();
          }
          Console.WriteLine("Number of Nodes found: " + edges.Count);
          if (useLinq)
            Console.WriteLine("Query using LINQ");
          if (numberOfWorkerThreads > 0)           
            Console.WriteLine("Start of triangle discovery using " + numberOfWorkerThreads + " threads, time is " + DateTime.Now);
          else if (numberOfWorkerThreads < 0)
            Console.WriteLine("Start of triangle discovery using system automatically selected number of threads, time is " + DateTime.Now);
          else
            Console.WriteLine("Start of triangle discovery using main thread, time is " + DateTime.Now);

          // Start counting triangles !
          if (numberOfWorkerThreads != 0)
          {
            if (useLinq)
            { // Please help, I have not figured out how to properly do the triangle query using LINQ
              int[] edge2values = null;
              if (numberOfWorkerThreads > 0)
                triangles = (from KeyValuePair<int, int[]> edgeFrom in edges
                           from int edgeTo1 in edgeFrom.Value
                           where edgeFrom.Key < edgeTo1
                           from int edgeTo2 in edgeFrom.Value
                           where edgeFrom.Key < edgeTo2 && edgeTo2 > edgeTo1 && edges.TryGetValue(edgeTo1, out edge2values) && Array.BinarySearch(edge2values, edgeTo2) >= 0
                           select edge).AsParallel().WithDegreeOfParallelism(numberOfWorkerThreads).Count();
              else
                triangles =  (from KeyValuePair<int, int[]> edgeFrom in edges
                              from int edgeTo1 in edgeFrom.Value
                             where edgeFrom.Key < edgeTo1
                             from int edgeTo2 in edgeFrom.Value
                             where edgeFrom.Key < edgeTo2 && edgeTo2 > edgeTo1 && edges.TryGetValue(edgeTo1, out edge2values) && Array.BinarySearch(edge2values, edgeTo2) >= 0
                             select edge).AsParallel().Count();
            }
            else
            {
              edgesItr = edges.Iterator();
              ParallelOptions pOptions = new ParallelOptions();
              pOptions.MaxDegreeOfParallelism = numberOfWorkerThreads;
              // First type parameter is the type of the source elements
              // Second type parameter is the type of the local data (subtotal)
              Parallel.ForEach<KeyValuePair<int, int[]>, long>(edges, // source collection
                pOptions,
                () => 0, // method to initialize the local variable
                (pair, loop, subtotal) => // method invoked by the loop on each iteration
                {
                  int nodeId = pair.Key;
                  int[] nodeTo = pair.Value;
                  int stop = nodeTo.Length - 1;
                  int i = stop;
                  int edgeToStart, edgeTo;
                  int pos;
                  while (i >= 0)
                  {
                    int[] edgeInfo2;
                    edgeToStart = nodeTo[i--];
                    if (nodeId < edgeToStart)
                    {
                      if (edges.TryGetValue(edgeToStart, out edgeInfo2))
                      {
                        for (int j = stop; j >= i; j--)
                        {
                          edgeTo = nodeTo[j];
                          if (edgeToStart < edgeTo)
                          {
                            pos = Array.BinarySearch<int>(edgeInfo2, edgeTo);
                            if (pos >= 0)
                            { // we know this one is connected to edgeInfo.From because it is part of edgeInfo.To
                              subtotal++;
                            }
                          }
                          else
                            break;
                        }
                      }
                    }
                    else
                      break;
                  }
                  return subtotal;
                },
                // Method to be executed when all loops have completed.
                // finalResult is the final value of subtotal. supplied by the ForEach method.
                (finalResult) => Interlocked.Add(ref triangles, finalResult));
            }
          }
          else if (useLinq)
            triangles = queryUsingLINQ(edges);
          else
            triangles = discoverTrianglesSingleCore(edges);

          session.Commit();
        }
        Console.WriteLine("Number of Triangles found: " + triangles + ", time is " + DateTime.Now);
      }
      catch (Exception e)
      {
        System.Console.WriteLine(e);
      }
    }
  }
}
