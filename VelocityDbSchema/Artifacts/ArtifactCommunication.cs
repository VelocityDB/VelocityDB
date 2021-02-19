using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelSandbox
{
    public interface IArtifactCommunication : IArtifactBase
    {
        string CommType { get; set; }
        ulong FromDevice { get; set; }
        ulong ToDevice { get; set; }
        DateTime StartTime { get; set; }
        DateTime EndTime { get; set; }

    }

    class ArtifactCommunication : ArtifactBase, IArtifactCommunication
    {
        string _commType;
        public string CommType { get { return _commType; } set { Update(); _commType = value; } }

        ulong _fromDevice;
        public ulong FromDevice { get { return _fromDevice; } set { Update(); _fromDevice = value; } }

        ulong _toDevice;
        public ulong ToDevice { get { return _toDevice; } set { Update(); _toDevice = value; } }

        DateTime _startTime;
        public DateTime StartTime { get { return _startTime; } set { Update(); _startTime = value; } }

        DateTime _endTime;
        public DateTime EndTime { get { return _endTime; } set { Update(); _endTime = value; } }

    }
}
