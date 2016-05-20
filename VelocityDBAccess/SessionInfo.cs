using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VelocityDBAccess
{
    public class SessionInfo
    {
        public enum SessionTypeEnum { NoServerSession, NoServerSharedSession, ServerClientSession };

        public string DBFolder { get; set; }
        public string Host { get; set; }
        public bool PessimisticLocking { get; set; }
        public SessionTypeEnum SessionType { get; set; }
        public bool WindowsAuth { get; set; }
    }
}
