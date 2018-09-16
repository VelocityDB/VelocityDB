using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Session;

namespace VelocityDbSchema.Samples.Wikipedia
{
  public class XmlElement : XmlLinkedNode
  {
    string localName;
    public XmlElement(string prefix, string localName, string namespaceURI, XmlDocument doc)
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
