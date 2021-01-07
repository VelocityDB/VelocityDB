namespace Frontenac.Blueprints.Util.IO.GML
{
    /// <summary>
    ///     A collection of tokens used for GML related data.
    ///     <p />
    ///     Tokens defined from GML Tags
    ///     (http://www.fim.uni-passau.de/fileadmin/files/lehrstuhl/brandenburg/projekte/gml/gml-documentation.tar.gz)
    /// </summary>
    public static class GmlTokens
    {
        public const string Gml = "gml";
        public const string Id = "id";
        public const string Name = "name";
        public const string Label = "label";
        public const string Comment = "comment";
        public const string Creator = "Creator";
        public const string Version = "Version";
        public const string Graph = "graph";
        public const string Node = "node";
        public const string Edge = "edge";
        public const string Source = "source";
        public const string Target = "target";
        public const string Directed = "directed"; // directed (0) undirected (1) default is undirected
        public const string Graphics = "graphics";
        public const string LabelGraphics = "LabelGraphics";
        public const char CommentChar = '#';

        /// <summary>
        ///     Special token used to store Blueprint ids as they may not be integers
        /// </summary>
        public const string BlueprintsId = "blueprintsId";
    }
}