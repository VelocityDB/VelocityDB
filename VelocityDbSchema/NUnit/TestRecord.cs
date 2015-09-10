using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;

namespace VelocityDbSchema.NUnit
{
  public class TestRecord : OptimizedPersistable
  {
    private readonly object recordDefLock = new object();
  }
}
