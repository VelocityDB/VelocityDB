using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VelocityDb.Indexing;
using VelocityDb.TypeInfo;

namespace RelSandbox
{
    public interface IDevice : IArtifactBase
    {
        ulong OwnerUid { get; set; }
    }

    class Device : ArtifactBase, IDevice
    {
        [Index]
        ulong _ownerUid;

        [FieldAccessor("_ownerUid")]
        public ulong OwnerUid { get { return _ownerUid; } set { Update(); _ownerUid = value; } }
    }
}
