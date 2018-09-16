using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VelocityDbSchema.Samples.Wikipedia
{
  public class XmlComment : XmlCharacterData
  {
    public XmlComment(string comment, XmlDocument doc)
      : base(comment, doc)
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
