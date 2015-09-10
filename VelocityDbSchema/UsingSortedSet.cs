using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Collection;
using VelocityDb.Session;

namespace VelocityDbSchema
{
  public class UsingSortedSet : OptimizedPersistable 
  {
    //public SortedSet<int> intSortedSet;
    public SortedSetAny<int> intSortedSet;
    public List<int> intList;
    public static Random randGen = new Random(5);
    public UsingSortedSet(int addHowMany) 
    {
      intSortedSet = new SortedSetAny<int>(1);
      intList = new List<int>();
      for (int i = 0; i < addHowMany; i++)
      {
        int n = randGen.Next();
        intSortedSet.Add(n);
        intList.Add(n);
      }
    }
  }
}
