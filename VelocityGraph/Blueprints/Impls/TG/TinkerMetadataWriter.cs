using System;
using System.IO;

namespace Frontenac.Blueprints.Impls.TG
{
    /// <summary>
    ///     Writes TinkerGrapĥ metadata to an OutputStream.
    /// </summary>
    internal class TinkerMetadataWriter
    {
        private readonly TinkerGrapĥ _tinkerGrapĥ;

        /// <summary>
        ///     the TinkerGrapĥ to pull the data from
        /// </summary>
        /// <param name="tinkerGrapĥ"></param>
        public TinkerMetadataWriter(TinkerGrapĥ tinkerGrapĥ)
        {
            if (tinkerGrapĥ == null)
                throw new ArgumentNullException(nameof(tinkerGrapĥ));

            _tinkerGrapĥ = tinkerGrapĥ;
        }

        /// <summary>
        ///     Write TinkerGrapĥ metadata to a file.
        /// </summary>
        /// <param name="filename">the name of the file to write the TinkerGrapĥ metadata to</param>
        public void Save(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
                throw new ArgumentNullException(nameof(filename));

            using (var fos = File.Create(filename))
            {
                Save(fos);
            }
        }

        /// <summary>
        ///     Write TinkerGrapĥ metadata to an OutputStream.
        /// </summary>
        /// <param name="outputStream">the OutputStream to write the TinkerGrapĥ metadata to</param>
        public void Save(Stream outputStream)
        {
            if (outputStream == null)
                throw new ArgumentNullException(nameof(outputStream));

            using (var writer = new BinaryWriter(outputStream))
            {
                writer.Write(_tinkerGrapĥ.CurrentId);
                WriteIndices(writer, _tinkerGrapĥ);
                WriteVertexKeyIndices(writer, _tinkerGrapĥ);
                WriteEdgeKeyIndices(writer, _tinkerGrapĥ);
            }
        }

        /// <summary>
        ///     Write TinkerGrapĥ metadata to an OutputStream.
        /// </summary>
        /// <param name="tinkerGrapĥ">the TinkerGrapĥ to pull the metadata from</param>
        /// <param name="outputStream">the OutputStream to write the TinkerGrapĥ metadata to</param>
        public static void Save(TinkerGrapĥ tinkerGrapĥ, Stream outputStream)
        {
            if (tinkerGrapĥ == null)
                throw new ArgumentNullException(nameof(tinkerGrapĥ));
            if (outputStream == null)
                throw new ArgumentNullException(nameof(outputStream));

            var writer = new TinkerMetadataWriter(tinkerGrapĥ);
            writer.Save(outputStream);
        }

        /// <summary>
        ///     Write TinkerGrapĥ metadata to a file.
        /// </summary>
        /// <param name="tinkerGrapĥ">the TinkerGrapĥ to pull the data from</param>
        /// <param name="filename">the name of the file to write the TinkerGrapĥ metadata to</param>
        public static void Save(TinkerGrapĥ tinkerGrapĥ, string filename)
        {
            if (tinkerGrapĥ == null)
                throw new ArgumentNullException(nameof(tinkerGrapĥ));
            if (string.IsNullOrWhiteSpace(filename))
                throw new ArgumentNullException(nameof(filename));

            var writer = new TinkerMetadataWriter(tinkerGrapĥ);
            writer.Save(filename);
        }

        private static void WriteIndices(BinaryWriter writer, TinkerGrapĥ tinkerGrapĥ)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));
            if (tinkerGrapĥ == null)
                throw new ArgumentNullException(nameof(tinkerGrapĥ));

            // Write the number of indices
            writer.Write(tinkerGrapĥ.Indices.Count);

            foreach (var index in tinkerGrapĥ.Indices)
            {
                // Write the index name
                writer.Write(index.Key);

                var tinkerIndex = index.Value;
                var indexClass = tinkerIndex.Type;

                // Write the index type
                writer.Write((byte) (indexClass == typeof (IVertex) ? 1 : 2));

                // Write the number of items associated with this index name
                writer.Write(tinkerIndex.Index.Count);
                foreach (var tinkerIndexItem in tinkerIndex.Index)
                {
                    // Write the item key
                    writer.Write(tinkerIndexItem.Key);

                    var tinkerIndexItemSet = tinkerIndexItem.Value;

                    // Write the number of sub-items associated with this item
                    writer.Write(tinkerIndexItemSet.Count);
                    foreach (var items in tinkerIndexItemSet)
                    {
                        if (indexClass == typeof (IVertex))
                        {
                            var vertices = items.Value;

                            // Write the number of vertices in this sub-item
                            writer.Write(vertices.Count);
                            foreach (var v in vertices)
                            {
                                // Write the vertex identifier
                                WriteTypedData(writer, v.Value.Id);
                            }
                        }
                        else if (indexClass == typeof (IEdge))
                        {
                            var edges = items.Value;

                            // Write the number of edges in this sub-item
                            writer.Write(edges.Count);
                            foreach (var e in edges)
                            {
                                // Write the edge identifier
                                WriteTypedData(writer, e.Value.Id);
                            }
                        }
                    }
                }
            }
        }

        private static void WriteVertexKeyIndices(BinaryWriter writer, TinkerGrapĥ tinkerGrapĥ)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));
            if (tinkerGrapĥ == null)
                throw new ArgumentNullException(nameof(tinkerGrapĥ));

            // Write the number of vertex key indices
            writer.Write(tinkerGrapĥ.VertexKeyIndex.Index.Count);

            foreach (var index in tinkerGrapĥ.VertexKeyIndex.Index)
            {
                // Write the key index name
                writer.Write(index.Key);

                // Write the number of items associated with this key index name
                writer.Write(index.Value.Count);
                foreach (var item in index.Value)
                {
                    // Write the item key
                    WriteTypedData(writer, item.Key);

                    // Write the number of vertices in this item
                    writer.Write(item.Value.Count);
                    foreach (var v in item.Value)
                    {
                        // Write the vertex identifier
                        WriteTypedData(writer, v.Value.Id);
                    }
                }
            }
        }

        private static void WriteEdgeKeyIndices(BinaryWriter writer, TinkerGrapĥ tinkerGrapĥ)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));
            if (tinkerGrapĥ == null)
                throw new ArgumentNullException(nameof(tinkerGrapĥ));

            // Write the number of edge key indices
            writer.Write(tinkerGrapĥ.EdgeKeyIndex.Index.Count);

            foreach (var index in tinkerGrapĥ.EdgeKeyIndex.Index)
            {
                // Write the key index name
                writer.Write(index.Key);

                // Write the number of items associated with this key index name
                writer.Write(index.Value.Count);
                foreach (var item in index.Value)
                {
                    // Write the item key
                    WriteTypedData(writer, item.Key);

                    // Write the number of edges in this item
                    writer.Write(item.Value.Count);
                    foreach (var e in item.Value)
                    {
                        // Write the edge identifier
                        WriteTypedData(writer, e.Value.Id);
                    }
                }
            }
        }

        private static void WriteTypedData(BinaryWriter writer, object data)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            var s = data as string;
            if (s != null)
            {
                writer.Write((byte) 1);
                writer.Write(s);
            }
            else if (data is int)
            {
                writer.Write((byte) 2);
                writer.Write((int) data);
            }
            else if (data is long)
            {
                writer.Write((byte) 3);
                writer.Write((long) data);
            }
            else if (data is short)
            {
                writer.Write((byte) 4);
                writer.Write((short) data);
            }
            else if (data is float)
            {
                writer.Write((byte) 5);
                writer.Write((float) data);
            }
            else if (data is double)
            {
                writer.Write((byte) 6);
                writer.Write((double) data);
            }
            else
                throw new IOException("unknown data type: use .NET serialization");
        }
    }
}