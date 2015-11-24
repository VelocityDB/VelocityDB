// This sample shows one way to solve problem descibed in http://stackoverflow.com/questions/28060104/recursive-query-with-sub-graph-aggregation-arbitrary-depth 

using Frontenac.Blueprints;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VelocityDb;
using VelocityDb.Session;
using VelocityGraph;
using Frontenac.Blueprints.Util.IO.GraphJson;

namespace SupplierTracking
{
  class SupplierTracking
  {
    static readonly string s_systemDir = "SupplierTracking"; // appended to SessionBase.BaseDatabasePath
    static readonly string s_licenseDbFile = "c:/4.odb";

    static int Main(string[] args)
    {
      DataCache.MaximumMemoryUse = 10000000000; // 10 GB, set this to what fits your case
      SupplierTracking supplierTracking = new SupplierTracking();

      using (SessionNoServer session = new SessionNoServer(s_systemDir, 5000, false, true))
      {             
        if (Directory.Exists(session.SystemDirectory))
          Directory.Delete(session.SystemDirectory, true); // remove systemDir from prior runs and all its databases.
        Directory.CreateDirectory(session.SystemDirectory);
        File.Copy(s_licenseDbFile, Path.Combine(session.SystemDirectory, "4.odb"));
        session.BeginUpdate();
        Graph g = new Graph(session);
        session.Persist(g);

        // SCHEMA
        VertexType warehouseVertexType = g.NewVertexType("Warehouse");
        VertexType supplierVertexType = g.NewVertexType("Supplier");
        EdgeType supplierWarehouseEdgeType = g.NewEdgeType("Warehouse", true, supplierVertexType, warehouseVertexType);
        EdgeType moveToS1EdgeType = g.NewEdgeType("MoveS1To", true, warehouseVertexType, warehouseVertexType);
        EdgeType moveToS2EdgeType = g.NewEdgeType("MoveS2To", true, warehouseVertexType, warehouseVertexType);
        EdgeType moveToS3EdgeType = g.NewEdgeType("MoveS3To", true, warehouseVertexType, warehouseVertexType);
        PropertyType supplierNameProperty = supplierVertexType.NewProperty("name", DataType.String, PropertyKind.NotIndexed);
        PropertyType wareHouseNameProperty = warehouseVertexType.NewProperty("name", DataType.String, PropertyKind.NotIndexed);
        PropertyType howManyS1Property = moveToS1EdgeType.NewProperty("howMany", DataType.Integer, PropertyKind.NotIndexed);
        PropertyType howManyS2Property = moveToS2EdgeType.NewProperty("howMany", DataType.Integer, PropertyKind.NotIndexed);
        PropertyType howManyS3Property = moveToS3EdgeType.NewProperty("howMany", DataType.Integer, PropertyKind.NotIndexed);

        Vertex supplier1 = supplierVertexType.NewVertex();
        supplier1.SetProperty(supplierNameProperty, "S1");

        Vertex supplier2 = supplierVertexType.NewVertex();
        supplier2.SetProperty(supplierNameProperty, "S2");

        Vertex supplier3 = supplierVertexType.NewVertex();
        supplier3.SetProperty(supplierNameProperty, "S3");

        Vertex wareHouse1Supplier1 = warehouseVertexType.NewVertex();
        supplier1.AddEdge(supplierWarehouseEdgeType, wareHouse1Supplier1);
        wareHouse1Supplier1.SetProperty(wareHouseNameProperty, "A01");
        Vertex wareHouse2Supplier1 = warehouseVertexType.NewVertex();
        supplier1.AddEdge(supplierWarehouseEdgeType, wareHouse2Supplier1);
        wareHouse2Supplier1.SetProperty(wareHouseNameProperty, "A02");
        Vertex wareHouse3Supplier1 = warehouseVertexType.NewVertex();
        supplier1.AddEdge(supplierWarehouseEdgeType, wareHouse3Supplier1);
        wareHouse3Supplier1.SetProperty(wareHouseNameProperty, "A05");
        Vertex wareHouse4Supplier1 = warehouseVertexType.NewVertex();
        supplier1.AddEdge(supplierWarehouseEdgeType, wareHouse4Supplier1);
        wareHouse4Supplier1.SetProperty(wareHouseNameProperty, "A06");
        Vertex wareHouse1Supplier2 = warehouseVertexType.NewVertex();
        supplier2.AddEdge(supplierWarehouseEdgeType, wareHouse1Supplier2);
        wareHouse1Supplier2.SetProperty(wareHouseNameProperty, "A03");
        Vertex wareHouse2Supplier2 = warehouseVertexType.NewVertex();
        supplier2.AddEdge(supplierWarehouseEdgeType, wareHouse2Supplier2);
        wareHouse2Supplier2.SetProperty(wareHouseNameProperty, "A07");
        Vertex wareHouse1Supplier3 = warehouseVertexType.NewVertex();
        supplier3.AddEdge(supplierWarehouseEdgeType, wareHouse1Supplier3);
        wareHouse1Supplier3.SetProperty(wareHouseNameProperty, "A04");
        Vertex wareHouse2Supplier3 = warehouseVertexType.NewVertex();
        supplier3.AddEdge(supplierWarehouseEdgeType, wareHouse2Supplier3);
        wareHouse2Supplier3.SetProperty(wareHouseNameProperty, "A08");

        Vertex wareHouseB1 = warehouseVertexType.NewVertex();
        wareHouseB1.SetProperty(wareHouseNameProperty, "B01");
        Vertex wareHouseB2 = warehouseVertexType.NewVertex();
        wareHouseB2.SetProperty(wareHouseNameProperty, "B02");
        Vertex wareHouseB3 = warehouseVertexType.NewVertex();
        wareHouseB3.SetProperty(wareHouseNameProperty, "B03");
        Vertex wareHouseB4 = warehouseVertexType.NewVertex();
        wareHouseB4.SetProperty(wareHouseNameProperty, "B04");

        Vertex wareHouseC1 = warehouseVertexType.NewVertex();
        wareHouseC1.SetProperty(wareHouseNameProperty, "C01");
        Vertex wareHouseC2 = warehouseVertexType.NewVertex();
        wareHouseC2.SetProperty(wareHouseNameProperty, "C02");

        Vertex wareHouseD1 = warehouseVertexType.NewVertex();
        wareHouseD1.SetProperty(wareHouseNameProperty, "D01");

        Edge moveA1toB1 = wareHouse1Supplier1.AddEdge(moveToS1EdgeType, wareHouseB1);
        moveA1toB1.SetProperty(howManyS1Property, 750);

        Edge moveA2toB1 = wareHouse2Supplier1.AddEdge(moveToS1EdgeType, wareHouseB1);
        moveA2toB1.SetProperty(howManyS1Property, 500);

        Edge moveA3toB2 = wareHouse1Supplier2.AddEdge(moveToS2EdgeType, wareHouseB2);
        moveA3toB2.SetProperty(howManyS2Property, 750);

        Edge moveA4toB2 = wareHouse1Supplier3.AddEdge(moveToS3EdgeType, wareHouseB2);
        moveA4toB2.SetProperty(howManyS3Property, 500);

        Edge moveA5toB3 = wareHouse3Supplier1.AddEdge(moveToS1EdgeType, wareHouseB3);
        moveA5toB3.SetProperty(howManyS1Property, 100);

        Edge moveA6toB3 = wareHouse4Supplier1.AddEdge(moveToS1EdgeType, wareHouseB3);
        moveA6toB3.SetProperty(howManyS1Property, 200);

        Edge moveA7toB4 = wareHouse2Supplier2.AddEdge(moveToS2EdgeType, wareHouseB4);
        moveA7toB4.SetProperty(howManyS2Property, 50);

        Edge moveA8toB4 = wareHouse2Supplier3.AddEdge(moveToS3EdgeType, wareHouseB4);
        moveA8toB4.SetProperty(howManyS3Property, 450);

        Edge moveB1toC1 = wareHouseB1.AddEdge(moveToS1EdgeType, wareHouseC1);
        moveB1toC1.SetProperty(howManyS1Property, 400);

        Edge moveS2B2toC1 = wareHouseB2.AddEdge(moveToS2EdgeType, wareHouseC1);
        moveS2B2toC1.SetProperty(howManyS2Property, 360);

        Edge moveS3B2toC1 = wareHouseB2.AddEdge(moveToS3EdgeType, wareHouseC1);
        moveS3B2toC1.SetProperty(howManyS3Property, 240);

        Edge moveS1B3toC2 = wareHouseB3.AddEdge(moveToS1EdgeType, wareHouseC2);
        moveS1B3toC2.SetProperty(howManyS1Property, 100);

        Edge moveS2B4toC2 = wareHouseB4.AddEdge(moveToS2EdgeType, wareHouseC2);
        moveS2B4toC2.SetProperty(howManyS2Property, 20);

        Edge moveS3B4toC2 = wareHouseB4.AddEdge(moveToS3EdgeType, wareHouseC2);
        moveS3B4toC2.SetProperty(howManyS3Property, 180);

        Edge moveS1C1toD1 = wareHouseC1.AddEdge(moveToS1EdgeType, wareHouseD1);
        moveS1C1toD1.SetProperty(howManyS1Property, 200);

        Edge moveS2C1toD1 = wareHouseC1.AddEdge(moveToS2EdgeType, wareHouseD1);
        moveS2C1toD1.SetProperty(howManyS2Property, 180);

        Edge moveS3C1toD1 = wareHouseC1.AddEdge(moveToS3EdgeType, wareHouseD1);
        moveS3C1toD1.SetProperty(howManyS3Property, 120);

        Edge moveS1C2toD1 = wareHouseC2.AddEdge(moveToS1EdgeType, wareHouseD1);
        moveS1C2toD1.SetProperty(howManyS1Property, 67);

        Edge moveS2C2toD1 = wareHouseC2.AddEdge(moveToS2EdgeType, wareHouseD1);
        moveS2C2toD1.SetProperty(howManyS2Property, 13);

        Edge moveS3C2toD1 = wareHouseC2.AddEdge(moveToS3EdgeType, wareHouseD1);
        moveS3C2toD1.SetProperty(howManyS3Property, 120);

        Console.WriteLine("Supplier 1 to Warehouse D total: " + supplierTracking.CalculateTotalTo(supplier1, supplierWarehouseEdgeType, moveToS1EdgeType, howManyS1Property, wareHouseD1));
        Console.WriteLine("Supplier 2 to Warehouse D total: " + supplierTracking.CalculateTotalTo(supplier2, supplierWarehouseEdgeType, moveToS2EdgeType, howManyS2Property, wareHouseD1));
        Console.WriteLine("Supplier 3 to Warehouse D total: " + supplierTracking.CalculateTotalTo(supplier3, supplierWarehouseEdgeType, moveToS3EdgeType, howManyS3Property, wareHouseD1));
        VelocityGraph.GraphJsonSettings gs = new VelocityGraph.GraphJsonSettings();
        gs.ClusterFuncProp = v =>
        {
          string s = (string)v.GetProperty("name");
          if (s == null)
            return 0;
          return Convert.ToInt32(s.First());
        };
        gs.VertexTypeFuncProp = v =>
        {
          Vertex vertex = v as Vertex;
          string s = (string)vertex.VertexType.TypeName;
          return s;
        }; 
        g.ExportToGraphJson("c:/SupplierTrackingExportToGraphJson.json", gs);
        Graph g2 = new Graph(session);
        session.Persist(g2);
        g2.ImportGraphJson("c:/SupplierTrackingExportToGraphJson.json");
        session.Commit();
        return 0;
      }
    }

    int CalculateTotalTo(Vertex supplier, EdgeType supplierWarehouseEdgeType, EdgeType moveToEdgeType, PropertyType howManyProperty, Vertex toVertex)
    {
      int total = 0;
      HashSet<Vertex> excludeSet = new HashSet<Vertex>();
      HashSet<EdgeType> edgeTypesToTraverse = new HashSet<EdgeType>();
      edgeTypesToTraverse.Add(moveToEdgeType);
      foreach (IEdge wareHouseEdge in supplier.GetEdges(supplierWarehouseEdgeType, Direction.Out))
      {
        Vertex supplierWareHouse = (Vertex)wareHouseEdge.GetVertex(Direction.In);
        var allPaths = supplierWareHouse.Traverse(10, true, Direction.Out, toVertex, edgeTypesToTraverse, null, excludeSet);
        foreach (List<Edge> path in allPaths)
        {
          if (path.Count > 0)
            total += (int)path.Last().GetProperty(howManyProperty); // last because that is all we care about in this simplified sample
          foreach (Edge edge in path)
          {
            excludeSet.Add(edge.Tail);
          }
        }
      }
      return total;
    }
  }
}
