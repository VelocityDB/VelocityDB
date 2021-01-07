using System;

namespace Frontenac.Blueprints.Impls.TG
{
    public static class TinkerGraphFactory
    {
        public static TinkerGrapĥ CreateTinkerGraph()
        {
            var graph = new TinkerGrapĥ();
            CreateTinkerGraph(graph);
            return graph;
        }

        public static void CreateTinkerGraph(IGraph graph)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));

            var marko = graph.AddVertex(1);
            marko.SetProperty("name", "marko");
            marko.SetProperty("age", 29);

            var vadas = graph.AddVertex(2);
            vadas.SetProperty("name", "vadas");
            vadas.SetProperty("age", 27);

            var lop = graph.AddVertex(3);
            lop.SetProperty("name", "lop");
            lop.SetProperty("lang", "java");

            var josh = graph.AddVertex(4);
            josh.SetProperty("name", "josh");
            josh.SetProperty("age", 32);

            var ripple = graph.AddVertex(5);
            ripple.SetProperty("name", "ripple");
            ripple.SetProperty("lang", "java");

            var peter = graph.AddVertex(6);
            peter.SetProperty("name", "peter");
            peter.SetProperty("age", 35);

            var loupi = graph.AddVertex(7);
            loupi.SetProperty("name", "loupi");
            loupi.SetProperty("age", 33);
            loupi.SetProperty("lang", "c#");

            graph.AddEdge(7, marko, vadas, "knows").SetProperty("weight", 0.5);
            graph.AddEdge(8, marko, josh, "knows").SetProperty("weight", 1.0);
            graph.AddEdge(9, marko, lop, "created").SetProperty("weight", 0.4);

            graph.AddEdge(10, josh, ripple, "created").SetProperty("weight", 1.0);
            graph.AddEdge(11, josh, lop, "created").SetProperty("weight", 0.4);

            graph.AddEdge(12, peter, lop, "created").SetProperty("weight", 0.2);
        }
    }
}