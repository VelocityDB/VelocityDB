using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Collection;
using VelocityDb.Session;
using VelocityDbSchema.Samples.AllSupportedSample;

namespace VelocityDbSchema
{
  public class StringTest : OptimizedPersistable 
  {
    public string theString;
    public Int32 stringDuplicateCt;

    public StringTest(string str, Int32 id) 
    {
      theString = str;
      stringDuplicateCt = id;
    }

    [FieldAccessor("stringDuplicateCt")]
    public Int32 StringDuplicateCt
    {
      get
      {
          return stringDuplicateCt;
      }
    }
    [FieldAccessor("theString")]
    public string TheString
    {
      get
      {
        return theString;
      }
    }

    public override string ToString()
    {
      return base.ToString() + " " + theString + " " + stringDuplicateCt;
    }    
  }
}
