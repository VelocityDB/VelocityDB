using System;
using System.Collections;
using System.Linq;
using System.Text;
using VelocityDb.Session;

namespace VelocityDbSchema.Samples.AllSupportedSample
{
  [Serializable]
  public class Cat : Pet
  {
    public string color;
    public Cat() { } // used by Activator.CreateInstance for non OptimizedPersistable subclasses and value types
    public Cat(string aName, short anAge, string color = "black") : base(aName, anAge)
    {
      this.color = color;
    }
  }
}
