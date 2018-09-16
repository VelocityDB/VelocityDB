using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Session;

namespace VelocityDbSchema.Samples.Wikipedia
{
  public abstract class XmlLinkedNode : XmlNode
  {
    public XmlLinkedNode(XmlDocument ownerDocument)
      : base(ownerDocument)
    {
    }
  }
}
