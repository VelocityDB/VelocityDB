using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Session;

namespace VelocityDbSchema.Samples.Wikipedia
{
  public abstract class XmlCharacterData : XmlLinkedNode
  {
    protected string data;

    public XmlCharacterData(string data, XmlDocument doc) : base(doc)
    {
      this.data = data;
    }

    public virtual string Data
    {
      get
      {
        return data;
      }
      set
      {
        Update();
        data = value;
      }
    }
  }
}
