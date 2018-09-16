using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Collection.BTree;
using VelocityDb.Session;

namespace VelocityDbSchema.TextIndexer
{
  public class WordHit : OptimizedPersistable
  {
    List<UInt64> _wordPositionArray;

    public WordHit()
    {
      _wordPositionArray = new List<UInt64>();
    }

    public void Add(UInt64 position)
    {
      Update();
      _wordPositionArray.Add(position);
    }
    
    public int Count
    {
      get
      {
        return _wordPositionArray.Count;
      }
    }

    public override bool AllowOtherTypesOnSamePage
    {
        get
        {
            return false;
        }
    }
  }
}
