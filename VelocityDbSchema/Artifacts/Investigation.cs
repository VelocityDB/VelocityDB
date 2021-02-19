using VelocityDb.TypeInfo;

namespace RelSandbox
{
    interface IInvestigation
    {
    }

    class Investigation : DbBase, IInvestigation
    {
        ulong _lastUid;

        [FieldAccessor("_lastUid")]
        internal ulong LastUid
        {
            get
            {
                return _lastUid;
            }
            set
            {
                Update(); _lastUid = value;
            }
        }
    }
}
