using System;
using System.Collections;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Session;

namespace VelocityDbSchema.Samples.AllSupportedSample
{
  [Serializable]
  public class Bird : Pet
  {
    public Bird() { }
    public Bird(string aName, short anAge) : base(aName, anAge)
    {
    }
  }
}
