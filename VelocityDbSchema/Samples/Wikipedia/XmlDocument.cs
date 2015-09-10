using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Session;

namespace VelocityDbSchema.Samples.Wikipedia
{
  public class XmlDocument : XmlNode
  {
    string localName;
    public XmlDocument(string localName)
      : base(null)
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
