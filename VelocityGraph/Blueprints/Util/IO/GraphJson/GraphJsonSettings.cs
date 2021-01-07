namespace Frontenac.Blueprints.Util.IO.GraphJson
{
    public class GraphJsonSettings
    {
        public string IdProp { get; set; }
        public string NodeCaptionProp { get; set; }
        public string EdgeCaptionProp { get; set; }
        public string SourceProp { get; set; }
        public string TargetProp { get; set; }

        public static GraphJsonSettings Default { get; private set; }

        static GraphJsonSettings()
        {
            Default = new GraphJsonSettings
                {
                    IdProp = "id",
                    NodeCaptionProp = "caption",
                    EdgeCaptionProp = "caption",
                    SourceProp = "source",
                    TargetProp = "target"
                };
        }
    }
}