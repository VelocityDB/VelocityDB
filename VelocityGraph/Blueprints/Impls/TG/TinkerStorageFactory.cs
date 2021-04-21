using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using VelocityGraph.Frontenac.Blueprints.Util.IO.GML;
using VelocityGraph.Frontenac.Blueprints.Util.IO.GraphML;
using VelocityGraph.Frontenac.Blueprints.Util.IO.GraphSON;

namespace VelocityGraph.Frontenac.Blueprints.Impls.TG
{
    /// <summary>
    ///     Constructs TinkerFile instances to load and save TinkerGrapĥ instances.
    /// </summary>
    internal class TinkerStorageFactory
    {
        private static TinkerStorageFactory _factory;

        private TinkerStorageFactory()
        {
        }

        public static TinkerStorageFactory GetInstance()
        {
            return _factory ?? (_factory = new TinkerStorageFactory());
        }

        public ITinkerStorage GetTinkerStorage(TinkerGrapĥ.FileType fileType)
        {
            Contract.Ensures(Contract.Result<ITinkerStorage>() != null);

            switch (fileType)
            {
                case TinkerGrapĥ.FileType.Gml:
                    return new GmlTinkerStorage();
                case TinkerGrapĥ.FileType.Graphml:
                    return new GraphMlTinkerStorage();
                case TinkerGrapĥ.FileType.Graphson:
                    return new GraphSonTinkerStorage();
                case TinkerGrapĥ.FileType.DotNet:
                    return new DotNetTinkerStorage();
            }

            throw new Exception(string.Format("File Type {0} is not configurable by the factory", fileType));
        }

        /// <summary>
        ///     Base class for loading and saving a TinkerGrapĥ where the implementation separates the data from the
        ///     meta data stored in the TinkerGrapĥ.
        /// </summary>
        [ContractClass(typeof (AbstractSeparateTinkerStorageContract))]
        private abstract class AbstractSeparateTinkerStorage : AbstractTinkerStorage
        {
            private const string GraphFileMetadata = "/tinkergraph-metadata.dat";

            /// <summary>
            ///     Save the data of the TinkerGrapĥ with the specific file format of the implementation.
            /// </summary>
            public abstract void SaveGraphData(TinkerGrapĥ tinkerGrapĥ, string directory);

            /// <summary>
            ///     Load the data from the TinkerGrapĥ with the specific file format of the implementation.
            /// </summary>
            public abstract void LoadGraphData(TinkerGrapĥ tinkerGrapĥ, string directory);

            public override TinkerGrapĥ Load(string directory)
            {
                if (!Directory.Exists(directory))
                    throw new Exception(string.Concat("Directory ", directory, " does not exist"));

                var graph = new TinkerGrapĥ();
                LoadGraphData(graph, directory);

                var filePath = string.Concat(directory, GraphFileMetadata);
                if (File.Exists(filePath))
                    TinkerMetadataReader.Load(graph, filePath);

                return graph;
            }

            public override void Save(TinkerGrapĥ tinkerGrapĥ, string directory)
            {
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                SaveGraphData(tinkerGrapĥ, directory);
                var filePath = string.Concat(directory, GraphFileMetadata);
                DeleteFile(filePath);
                TinkerMetadataWriter.Save(tinkerGrapĥ, filePath);
            }
        }

        [ContractClassFor(typeof (AbstractSeparateTinkerStorage))]
        private abstract class AbstractSeparateTinkerStorageContract : AbstractSeparateTinkerStorage
        {
            public override void LoadGraphData(TinkerGrapĥ tinkerGrapĥ, string directory)
            {
                Contract.Requires(tinkerGrapĥ != null);
                Contract.Requires(!string.IsNullOrWhiteSpace(directory));
            }

            public override void SaveGraphData(TinkerGrapĥ tinkerGrapĥ, string directory)
            {
                Contract.Requires(tinkerGrapĥ != null);
                Contract.Requires(!string.IsNullOrWhiteSpace(directory));
            }
        }

        /// <summary>
        ///     Base class for loading and saving a TinkerGrapĥ.
        /// </summary>
        private abstract class AbstractTinkerStorage : ITinkerStorage
        {
            public abstract TinkerGrapĥ Load(string directory);
            public abstract void Save(TinkerGrapĥ tinkerGrapĥ, string directory);

            /// <summary>
            ///     Clean up the directory that houses the TinkerGrapĥ.
            /// </summary>
            /// <param name="path"></param>
            protected static void DeleteFile(string path)
            {
                Contract.Requires(!string.IsNullOrWhiteSpace(path));

                if (File.Exists(path))
                    File.Delete(path);
            }
        }

        /// <summary>
        ///     Reads and writes a TinkerGrapĥ using .NET serialization.
        /// </summary>
        private class DotNetTinkerStorage : AbstractTinkerStorage
        {
            private const string GraphFileDotNet = "/tinkergraph.dat";

            public override TinkerGrapĥ Load(string directory)
            {
                using (var stream = File.OpenRead(string.Concat(directory, GraphFileDotNet)))
                {
                    var formatter = new BinaryFormatter();
                    return (TinkerGrapĥ) formatter.Deserialize(stream);
                }
            }

            public override void Save(TinkerGrapĥ tinkerGrapĥ, string directory)
            {
                var filePath = string.Concat(directory, GraphFileDotNet);
                DeleteFile(filePath);
                using (var stream = File.Create(string.Concat(directory, GraphFileDotNet)))
                {
                    var formatter = new BinaryFormatter();
                    formatter.Serialize(stream, tinkerGrapĥ);
                }
            }
        }

        /// <summary>
        ///     Reads and writes a TinkerGrapĥ to GML as the format for the data.
        /// </summary>
        private class GmlTinkerStorage : AbstractSeparateTinkerStorage
        {
            private const string GraphFileGml = "/tinkergraph.gml";

            public override void LoadGraphData(TinkerGrapĥ tinkerGrapĥ, string directory)
            {
                GmlReader.InputGraph(tinkerGrapĥ, string.Concat(directory, GraphFileGml));
            }

            public override void SaveGraphData(TinkerGrapĥ tinkerGrapĥ, string directory)
            {
                var filePath = string.Concat(directory, GraphFileGml);
                DeleteFile(filePath);
                GmlWriter.OutputGraph(tinkerGrapĥ, filePath);
            }
        }

        /// <summary>
        ///     Reads and writes a TinkerGrapĥ to GraphML as the format for the data.
        /// </summary>
        private class GraphMlTinkerStorage : AbstractSeparateTinkerStorage
        {
            private const string GraphFileGraphml = "/tinkergraph.xml";

            public override void LoadGraphData(TinkerGrapĥ tinkerGrapĥ, string directory)
            {
                GraphMlReader.InputGraph(tinkerGrapĥ, string.Concat(directory, GraphFileGraphml));
            }

            public override void SaveGraphData(TinkerGrapĥ tinkerGrapĥ, string directory)
            {
                var filePath = string.Concat(directory, GraphFileGraphml);
                DeleteFile(filePath);
                GraphMlWriter.OutputGraph(tinkerGrapĥ, filePath);
            }
        }

        /// <summary>
        ///     Reads and writes a TinkerGrapĥ to GraphSON as the format for the data.
        /// </summary>
        private class GraphSonTinkerStorage : AbstractSeparateTinkerStorage
        {
            private const string GraphFileGraphson = "/tinkergraph.json";

            public override void LoadGraphData(TinkerGrapĥ tinkerGrapĥ, string directory)
            {
                GraphSonReader.InputGraph(tinkerGrapĥ, string.Concat(directory, GraphFileGraphson));
            }

            public override void SaveGraphData(TinkerGrapĥ tinkerGrapĥ, string directory)
            {
                var filePath = string.Concat(directory, GraphFileGraphson);
                DeleteFile(filePath);
                GraphSonWriter.OutputGraph(tinkerGrapĥ, filePath, GraphSonMode.EXTENDED);
            }
        }
    }
}