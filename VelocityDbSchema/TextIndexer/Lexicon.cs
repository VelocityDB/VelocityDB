using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Collection.Comparer;
using VelocityDb.Collection.BTree;
using VelocityDb.Session;

namespace VelocityDbSchema.TextIndexer
{
  public class Lexicon : OptimizedPersistable
  {
    public const UInt32 PlaceInDatabase = 13;

    BTreeSetOidShort<Word> wordSet;

    public Lexicon(ushort nodeSize, HashCodeComparer<Word> hashComparer, SessionBase session)
    {
      wordSet = new BTreeSetOidShort<Word>(hashComparer, session, nodeSize, sizeof(int));
    }

    public override bool AllowOtherTypesOnSamePage
    {
      get
      {
        return false;
      }
    }

    public override UInt32 PlacementDatabaseNumber
    {
      get
      {
        return PlaceInDatabase;
      }
    }

    public BTreeSetOidShort<Word> WordSet
    {
      get
      {
        return wordSet;
      }
    }
  }
}
