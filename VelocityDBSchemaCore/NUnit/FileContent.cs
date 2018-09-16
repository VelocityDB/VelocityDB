using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;

namespace VelocityDbSchema.NUnit
{
  public class FileContent : OptimizedPersistable
  {
    byte[] m_bytes;

    public FileContent(byte[] bytes)
    {
      m_bytes = bytes;
    }
  }
}
