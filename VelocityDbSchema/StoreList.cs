using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Collection;
using VelocityDb.Session;

namespace VelocityDbSchema
{
  public class StoreList : OptimizedPersistable
  {
    public List<int> intList;
    public Int16 in16;
    public StoreList()
    {
      intList = new List<int>(2);
      intList.Add(1);
      in16 = 16;
    }
  }
}
