using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;

namespace VelocityDbSchema.NUnit
{

  public interface IHasClassName
  {
    string ClassName { get;}
  }

  public class BaseClassA : OptimizedPersistable, IHasClassName
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

    public virtual string ClassName
    {
      get
      {
        return m_baseClassA;
      }
    }
  }

  public class ClassB : BaseClassA
  {
    string m_classB = "ClassB";
    public override string ClassName
    {
      get
      {
        return m_classB;
      }
    }
  }
  public class ClassFromB : ClassB
  {
    string m_classfromB = "ClassFromB";
    public override string ClassName
    {
      get
      {
        return m_classfromB;
      }
    }
  }

  public class ClassC : BaseClassA
  {
    string m_classC = "ClassC";
    public override string ClassName
    {
      get
      {
        return m_classC;
      }
    }
  }

  public class ClassD : IHasClassName
  {
    string m_className = "ClassD";
    public string ClassName
    {
      get
      {
        return m_className;
      }
    }
  }
}
