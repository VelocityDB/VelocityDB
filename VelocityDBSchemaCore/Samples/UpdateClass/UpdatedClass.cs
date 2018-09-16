using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Session;

namespace VelocityDbSchema.Samples.UpdateClass
{
  // first version of class has:
  //                                    UInt32 aUint32; UInt64 aUint64;                 float aFloat; (1) then
  //                                    UInt32 aUint32; UInt64 aUint64; string aString; float aFloat; (2)
  //                                    UInt32 aUint32;                 string aString; float aFloat; (3)
  //                                    UInt32 aUint32;                                 float aFloat; (4)
  //                    UInt16 aUint16;                                                 float aFloat; (5) 
  //                    UInt16 aUint16; UInt32 aUint32;                                 float aFloat; (6) 
  //                    Int64 aUint16;  UInt32 aUint32;                                 float aFloat; (7) 
  // UpdatedClass aRef; Int64 aUint16;  UInt32 aUint32;                                 float aFloat; (8) 
  //                    Int64 aUint16;  UInt32 aUint32;                                 float aFloat; (9) 
  //                    Int64 aUint16;  UInt32 aUint32;                                 float aFloat; Int32[] intArray; DateTime aDateTime; (10)
  //                    Int64 aUint16;  UInt32 aUint32;                                 float aFloat;                   DateTime aDateTime; (11)
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
