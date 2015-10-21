using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VelocityDb;

namespace DatabaseManager.Model
{
  public class FederationCopyInfo : OptimizedPersistable
  {
    DateTime m_creationTime;
    string m_hostName;
    string m_directoryPath;

    public FederationCopyInfo(string hostName, string directoryPath)
    {
      m_creationTime = DateTime.Now;
      m_hostName = hostName;
      m_directoryPath = directoryPath;
    }
  }
}
