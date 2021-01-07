using System;
using System.Collections.Concurrent;
using System.IO;

namespace Frontenac.Blueprints.Impls.TG
{
    /// <summary>
    ///     Reads TinkerGrapĥ metadata from a Stream.
    /// </summary>
    public class TinkerMetadataReader
    {
        private readonly TinkerGrapĥ _tinkerGrapĥ;

        public TinkerMetadataReader(TinkerGrapĥ tinkerGrapĥ)
        {
            if (tinkerGrapĥ == null)
                throw new ArgumentNullException(nameof(tinkerGrapĥ));

            _tinkerGrapĥ = tinkerGrapĥ;
        }

        /// <summary>
        ///     Read TinkerGrapĥ metadata from a file.
        /// </summary>
        /// <param name="filename">the name of the file to read the TinkerGrapĥ metadata from</param>
        public void Load(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
                throw new ArgumentNullException(nameof(filename));

            using (var fos = File.OpenRead(filename))
            {
                Load(fos);
            }
        }

        /// <summary>
        ///     Read TinkerGrapĥ metadata from a Stream.
        /// </summary>
        /// <param name="inputStream">the Stream to read the TinkerGrapĥ metadata from</param>
        public void Load(Stream inputStream)
        {
            if (inputStream == null)
                throw new ArgumentNullException(nameof(inputStream));

            using (var reader = new BinaryReader(inputStream))
            {
                _tinkerGrapĥ.CurrentId = reader.ReadInt64();
                ReadIndices(reader, _tinkerGrapĥ);
                ReadVertexKeyIndices(reader, _tinkerGrapĥ);
                ReadEdgeKeyIndices(reader, _tinkerGrapĥ);
            }
        }

        /// <summary>
        ///     Read TinkerGrapĥ metadata from a Stream.
        /// </summary>
        /// <param name="tinkerGrapĥ">the IGraph to push the metadata to</param>
        /// <param name="inputStream">the Stream to read the TinkerGrapĥ metadata from</param>
        public static void Load(TinkerGrapĥ tinkerGrapĥ, Stream inputStream)
        {
            if (tinkerGrapĥ == null)
                throw new ArgumentNullException(nameof(tinkerGrapĥ));
            if (inputStream == null)
                throw new ArgumentNullException(nameof(inputStream));

            var reader = new TinkerMetadataReader(tinkerGrapĥ);
            reader.Load(inputStream);
        }

        /// <summary>
        ///     Read TinkerGrapĥ metadata from a file.
        /// </summary>
        /// <param name="tinkerGrapĥ">the TinkerGrapĥ to push the data to</param>
        /// <param name="filename">the name of the file to read the TinkerGrapĥ metadata from</param>
        public static void Load(TinkerGrapĥ tinkerGrapĥ, string filename)
        {
            if (tinkerGrapĥ == null)
                throw new ArgumentNullException(nameof(tinkerGrapĥ));
            if (string.IsNullOrWhiteSpace(filename))
                throw new ArgumentNullException(nameof(filename));

            var reader = new TinkerMetadataReader(tinkerGrapĥ);
            reader.Load(filename);
        }

        private static void ReadIndices(BinaryReader reader, TinkerGrapĥ tinkerGrapĥ)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            if (tinkerGrapĥ == null)
                throw new ArgumentNullException(nameof(tinkerGrapĥ));

            // Read the number of indices
            var indexCount = reader.ReadInt32();

            for (var i = 0; i < indexCount; i++)
            {
                // Read the index name
                var indexName = reader.ReadString();

                // Read the index type
                var indexType = reader.ReadByte();

                if (indexType != 1 && indexType != 2)
                {
                    throw new InvalidDataException("Unknown index class type");
                }

                var tinkerIndex = new TinkerIndex(indexName, indexType == 1 ? typeof (IVertex) : typeof (IEdge));

                // Read the number of items associated with this index name
                var indexItemCount = reader.ReadInt32();
                for (var j = 0; j < indexItemCount; j++)
                {
                    // Read the item key
                    var indexItemKey = reader.ReadString();

                    // Read the number of sub-items associated with this item
                    var indexValueItemSetCount = reader.ReadInt32();
                    for (var k = 0; k < indexValueItemSetCount; k++)
                    {
                        // Read the number of vertices or edges in this sub-item
                        var setCount = reader.ReadInt32();
                        for (var l = 0; l < setCount; l++)
                        {
                            // Read the vertex or edge identifier
                            if (indexType == 1)
                            {
                                var v = tinkerGrapĥ.GetVertex(ReadTypedData(reader));
                                if (v != null)
                                    tinkerIndex.Put(indexItemKey, v.GetProperty(indexItemKey), v);
                            }
                            else if (indexType == 2)
                            {
                                var e = tinkerGrapĥ.GetEdge(ReadTypedData(reader));
                                if (e != null)
                                    tinkerIndex.Put(indexItemKey, e.GetProperty(indexItemKey), e);
                            }
                        }
                    }
                }

                tinkerGrapĥ.Indices.Put(indexName, tinkerIndex);
            }
        }

        private static void ReadVertexKeyIndices(BinaryReader reader, TinkerGrapĥ tinkerGrapĥ)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            if (tinkerGrapĥ == null)
                throw new ArgumentNullException(nameof(tinkerGrapĥ));

            // Read the number of vertex key indices
            var indexCount = reader.ReadInt32();

            for (var i = 0; i < indexCount; i++)
            {
                // Read the key index name
                var indexName = reader.ReadString();

                tinkerGrapĥ.VertexKeyIndex.CreateKeyIndex(indexName);

                var items = new ConcurrentDictionary<object, ConcurrentDictionary<string, IElement>>();

                // Read the number of items associated with this key index name
                var itemCount = reader.ReadInt32();
                for (var j = 0; j < itemCount; j++)
                {
                    // Read the item key
                    var key = ReadTypedData(reader);

                    var vertices = new ConcurrentDictionary<string, IElement>();

                    // Read the number of vertices in this item
                    var vertexCount = reader.ReadInt32();
                    for (var k = 0; k < vertexCount; k++)
                    {
                        // Read the vertex identifier
                        var v = tinkerGrapĥ.GetVertex(ReadTypedData(reader));
                        if (v != null)
                            vertices.TryAdd(v.Id.ToString(), v);
                    }

                    items.Put(key, vertices);
                }

                tinkerGrapĥ.VertexKeyIndex.Index.Put(indexName, items);
            }
        }

        private static void ReadEdgeKeyIndices(BinaryReader reader, TinkerGrapĥ tinkerGrapĥ)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            if (tinkerGrapĥ == null)
                throw new ArgumentNullException(nameof(tinkerGrapĥ));

            // Read the number of edge key indices
            var indexCount = reader.ReadInt32();

            for (var i = 0; i < indexCount; i++)
            {
                // Read the key index name
                var indexName = reader.ReadString();

                tinkerGrapĥ.EdgeKeyIndex.CreateKeyIndex(indexName);

                var items = new ConcurrentDictionary<object, ConcurrentDictionary<string, IElement>>();

                // Read the number of items associated with this key index name
                var itemCount = reader.ReadInt32();
                for (var j = 0; j < itemCount; j++)
                {
                    // Read the item key
                    var key = ReadTypedData(reader);

                    var edges = new ConcurrentDictionary<string, IElement>();

                    // Read the number of edges in this item
                    var edgeCount = reader.ReadInt32();
                    for (var k = 0; k < edgeCount; k++)
                    {
                        // Read the edge identifier
                        var e = tinkerGrapĥ.GetEdge(ReadTypedData(reader));
                        if (e != null)
                            edges.TryAdd(e.Id.ToString(), e);
                    }

                    items.Put(key, edges);
                }

                tinkerGrapĥ.EdgeKeyIndex.Index.Put(indexName, items);
            }
        }

        private static object ReadTypedData(BinaryReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            var type = reader.ReadByte();

            switch (type)
            {
                case 1:
                    return reader.ReadString();
                case 2:
                    return reader.ReadInt32();
                case 3:
                    return reader.ReadInt64();
                case 4:
                    return reader.ReadInt16();
                case 5:
                    return reader.ReadSingle();
                case 6:
                    return reader.ReadDouble();
                default:
                    throw new IOException("unknown data type: use .NET serialization");
            }
        }
    }
}