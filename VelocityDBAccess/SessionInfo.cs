using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using VelocityDb.Session;

namespace VelocityDBAccess
{
  public class SessionInfo
  {
    SessionBase _session;
    public enum SessionTypeEnum { NoServerSession, NoServerSharedSession, ServerClientSession };

    public string DBFolder { get; set; }
    public string Host { get; set; }
    public bool PessimisticLocking { get; set; }
    public SessionTypeEnum SessionType { get; set; }
    public bool WindowsAuth { get; set; }
    public SessionBase Session => _session;
    public void SetSession()
    {
      _session = null;
      bool lPessimistic = PessimisticLocking;
      bool lAuth = WindowsAuth;
      switch (SessionType)
      {
        case SessionTypeEnum.NoServerSession:
          _session = new SessionNoServer(DBFolder, 5000, !lPessimistic);
          break;
        case SessionTypeEnum.NoServerSharedSession:
          _session = new SessionNoServerShared(DBFolder, 5000, !lPessimistic);
          break;
        case SessionTypeEnum.ServerClientSession:
          IPHostEntry lHostEntry = Dns.GetHostEntry(Host);
          _session = new ServerClientSession(DBFolder, lHostEntry.HostName, 2000, !lPessimistic);
          SessionBase.DoWindowsAuthentication = lAuth;
          break;
        default:
          throw new InvalidDataException("Invalid Session Type");
      }
      Session.BeginRead();
    }
  }
}
