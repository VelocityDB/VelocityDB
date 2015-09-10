using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Session;

namespace VelocityDbSchema.Samples.Wikipedia
{
  public class XmlText : XmlCharacterData
  {
    public XmlText(string data, XmlDocument doc)
      : base(data, doc)
    {
    }
    public override string Name
    {
      get
      {
        return data;
      }
    }
  }
}
