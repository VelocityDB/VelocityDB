using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Session;

namespace VelocityDbSchema.Samples.Wikipedia
{
  public abstract class XmlNode : OptimizedPersistable
  {
    XmlDocument ownerDocument;
#pragma warning disable 0169
    XmlNode parentNode;
    XmlNode[] childNodes;
#pragma warning restore 0169 

    public XmlNode(XmlDocument ownerDocument)
    {
      this.ownerDocument = ownerDocument;
    }

    public abstract string Name { get; }

    public virtual XmlDocument OwnerDocument
    {
      get
      {
        return ownerDocument;
      }
    }
  }
}
