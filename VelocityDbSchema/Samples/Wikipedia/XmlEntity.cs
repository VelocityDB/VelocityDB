using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VelocityDbSchema.Samples.Wikipedia
{
  public class XmlEntity : XmlNode
  {
    string localName;
    public XmlEntity(string localName, XmlDocument doc)
      : base(doc)
    {
      this.localName = localName;
    }

    public override string Name
    {
      get
      {
        return localName;
      }
    }
  }
}
