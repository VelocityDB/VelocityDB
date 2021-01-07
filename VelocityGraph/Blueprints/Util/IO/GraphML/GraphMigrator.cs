using System;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;

namespace Frontenac.Blueprints.Util.IO.GraphML
{
    /// <summary>
    ///     GraphMigrator takes the data in one graph and pipes it to another graph.
    /// </summary>
    public static class GraphMigrator
    {
        /// <summary>
        ///     Pipe the data from one graph to another graph.
        /// </summary>
        /// <param name="fromGraph">the graph to take data from</param>
        /// <param name="toGraph">the graph to take data to</param>
        public static void MigrateGraph(IGraph fromGraph, IGraph toGraph)
        {
            if (fromGraph == null)
                throw new ArgumentNullException(nameof(fromGraph));
            if (toGraph == null)
                throw new ArgumentNullException(nameof(toGraph));

            const int pipeSize = 1024;
            var outPipe = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.Inheritable, pipeSize);
            {
                using (var inPipe = new AnonymousPipeClientStream(PipeDirection.In, outPipe.ClientSafePipeHandle))
                {
                    Task.Factory.StartNew(() =>
                        {
                            GraphMlWriter.OutputGraph(fromGraph, outPipe);
                            outPipe.Flush();
                            outPipe.Close();
                        });

                    GraphMlReader.InputGraph(toGraph, inPipe);
                }
            }
        }
    }
}