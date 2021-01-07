namespace Frontenac.Blueprints.Util.IO.GraphSON
{
    /// <summary>
    ///     Modes of operation of the GraphSONUtility.
    /// </summary>
    public enum GraphSonMode
    {
// ReSharper disable InconsistentNaming
        /// <summary>
        ///     COMPACT constructs GraphSON on the assumption that all property keys
        ///     are fair game for exclusion including _type, _inV, _outV, _label and _id.
        ///     It is possible to write GraphSON that cannot be read back into Graph,
        ///     if some or all of these keys are excluded.
        /// </summary>
        COMPACT,

        /// <summary>
        ///     NORMAL includes the _type field and JSON data typing.
        /// </summary>
        NORMAL,

        /// <summary>
        ///     EXTENDED includes the _type field and explicit data typing.
        /// </summary>
        EXTENDED
// ReSharper restore InconsistentNaming
    }
}