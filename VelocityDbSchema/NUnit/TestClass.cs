using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Collection;

namespace VelocityDbSchema.NUnit
{
  public class TestClass : OptimizedPersistable
  {
    public TestClass()
    {
    }

    private int someIntVar;

    public int SomeIntVar
    {
      get { return someIntVar; }
      set { someIntVar = value; }
    }

    private string someStringVar;

    public string SomeStringVar
    {
      get { return someStringVar; }
      set { someStringVar = value; }
    }

  }
}
