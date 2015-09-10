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
    UInt64[] wordPositionArray;

    public WordHit(Document doc, UInt64 position, SessionBase session)
    {
      wordPositionArray = new UInt64[1];
      wordPositionArray[0] = position;
    }

    public void Add(UInt64 position)
    {
      Update();
      Array.Resize(ref wordPositionArray, wordPositionArray.Length + 1);
      wordPositionArray[wordPositionArray.Length - 1] = position;
    }
    
    public int Count
    {
      get
      {
        return wordPositionArray.Length;
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
