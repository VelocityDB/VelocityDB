using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VelocityDbSchema.Samples.Wikipedia
{
  public class XmlWhitespace : XmlCharacterData
  {
    public XmlWhitespace(string strData, XmlDocument doc): base(strData, doc)
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
