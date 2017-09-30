using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VelocityDb;

namespace UpdateClass
{
  public class UpdatedClass : OptimizedPersistable
  {
#pragma warning disable 0414
    //UpdatedClass aRef;
    //Int64 aInt64; 
    //UInt16 aUint16; 
    UInt32 aUint32;
    UInt64 aUint64;
    //string aString;
    float aFloat;
#pragma warning restore 0414
    //Int32[] intArray;
    //DateTime aDateTime;

    public UpdatedClass()
    {
      //aRef = this;
      //aInt64 = Int64.MaxValue;
      //aUint16 = UInt16.MaxValue;
      aUint32 = UInt32.MaxValue;
      aUint64 = UInt64.MaxValue;
      //aString = DateTime.Now.ToString();
      aFloat = float.MaxValue;
      //intArray = new Int32[10];
      //aDateTime = DateTime.Now;
    }
  }
}
