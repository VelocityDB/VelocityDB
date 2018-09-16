using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VelocityDbSchema.Samples.Wikipedia
{
  public class XmlSchemaElement : XmlSchemaParticle
  {
    string name;
    public string Name
    {
      get
      {
        return name;
      }
      set
      {
        Update();
        name = value;
      }
    }
  }
}
