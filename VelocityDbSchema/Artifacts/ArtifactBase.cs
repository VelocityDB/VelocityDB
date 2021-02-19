using VelocityDb.Indexing;
using VelocityDb.TypeInfo;

namespace RelSandbox
{
    public interface IArtifactBase : IDbBase
    {
        ulong Uid { get; set; }
    }

    class ArtifactBase : DbBase, IArtifactBase
    {
        [Index]
        [UniqueConstraint]
        ulong _uid;

        [FieldAccessor("_uid")]
        public ulong Uid
        {
            get
            {
                return _uid;
            }
            set
            {
                Update(); _uid = value;
            }
        }
    }
}
