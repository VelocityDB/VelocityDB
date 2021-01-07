namespace Frontenac.Blueprints.Util.IO.GraphSON
{
    /// <summary>
    ///     A factory responsible for creating graph elements.  Abstracts the way that graph elements are created. In
    ///     most cases a Graph is responsible for element creation, but there are cases where more control over
    ///     how vertices and edges are constructed.
    /// </summary>
    public interface IElementFactory
    {
        /// <summary>
        ///     Creates a new Edge instance.
        /// </summary>
        IEdge CreateEdge(object id, IVertex out_, IVertex in_, string label);

        /// <summary>
        ///     reates a new Vertex instance.
        /// </summary>
        IVertex CreateVertex(object id);
    }
}