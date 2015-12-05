using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;

namespace VelocityDbSchema.NUnit
{
  public class BaseClassA : OptimizedPersistable
  {
    string m_baseClassA = "BaseClassA";
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
