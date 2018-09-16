using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Collection;
using VelocityDb.Session;

namespace VelocityDbSchema
{
	public class StoreStruct : OptimizedPersistable 
  {
    public MyStruct myStruct;
    public Int16 in16;
    public StoreStruct()
    {
      myStruct = new MyStruct();
      myStruct.anInt = 9999;
      myStruct.aString = "string in a struct";
      myStruct.aBool = true;
      myStruct.insideStruct = new MyStructInside();
      myStruct.insideStruct.anInt = 88888;
      myStruct.insideStruct.aString = "string in a struct in a struct";
      myStruct.insideStruct.boolList = new VelocityDbList<bool>(2);
      myStruct.intList = new List<int>();
      in16 = 16;
    }
   [Serializable]
    public struct MyStruct
    {
      public int anInt;
      public string aString;
      public bool aBool;
      public List<int> intList;
      public MyStructInside insideStruct;
    }

    public struct MyStructInside
    {
      public int anInt;
      public string aString;
      public bool aBool;
      public VelocityDbList<bool> boolList;
    }
  }
}
