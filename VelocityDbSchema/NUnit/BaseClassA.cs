using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;

namespace VelocityDbSchema.NUnit
{
  public class BaseClassA : OptimizedPersistable
  {
    static readonly Random random = new Random(5);
    string m_baseClassA = "BaseClassA";
    double m_randOrder = random.NextDouble();   

    public double RandomOrder
    {
      get
      {
        return m_randOrder;
      }
    }
  }

  public class ClassB : BaseClassA
  {
    string m_classB = "ClassB";
  }

  public class ClassC : BaseClassA
  {
    string m_classC = "ClassC";
  }
}
