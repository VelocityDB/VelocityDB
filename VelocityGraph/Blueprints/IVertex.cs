using System.Collections.Generic;

namespace Frontenac.Blueprints
{
    /// <summary>
    ///     A vertex maintains pointers to both a set of incoming and outgoing edges.
    ///     The outgoing edges are those edges for which the vertex is the tail.
    ///     The incoming edges are those edges for which the vertex is the head.
    ///     Diagrammatically, ---inEdges---> vertex ---outEdges--->.
    /// </summary>
    public interface IVertex : IElement
    {
        /// <summary>
        ///     Return the edges incident to the vertex according to the provided direction and edge labels.
        /// </summary>
        /// <param name="direction">the direction of the edges to retrieve</param>
        /// <param name="labels">the labels of the edges to retrieve</param>
        /// <returns>an IEnumerable of incident edges</returns>
        IEnumerable<IEdge> GetEdges(Direction direction, params string[] labels);

        /// <summary>
        ///     Return the vertices adjacent to the vertex according to the provided direction and edge labels.
        ///     This method does not remove duplicate vertices (i.e. those vertices that are connected by more than one edge).
        /// </summary>
        /// <param name="direction">the direction of the edges of the adjacent vertices</param>
        /// <param name="labels">the labels of the edges of the adjacent vertices</param>
        /// <returns>an IEnumerable of adjacent vertices</returns>
        IEnumerable<IVertex> GetVertices(Direction direction, params string[] labels);

        /// <summary>
        ///     Generate a query object that can be used to fine tune which edges/vertices are retrieved that are incident/adjacent to this vertex.
        /// </summary>
        /// <returns>a vertex query object with methods for constraining which data is pulled from the underlying graph</returns>
        IVertexQuery Query();

        /// <summary>
        ///     Add a new outgoing edge from this vertex to the parameter vertex with provided edge label.
        /// </summary>
        /// <param name="id">the id of the edge</param>
        /// <param name="label">the label of the edge</param>
        /// <param name="inVertex"></param>
        /// <returns>the newly created edge</returns>
        IEdge AddEdge(object id, string label, IVertex inVertex);
    }
}