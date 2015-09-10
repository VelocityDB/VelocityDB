using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using VelocityDb;
using VelocityDb.Collection.BTree;
using VelocityDb.Session;
using VelocityDb.Collection.Comparer;

namespace VelocityDbSchema.TextIndexer
{
  public class IndexRoot : OptimizedPersistable
  {
    public const UInt32 PlaceInDatabase = 11;
    public Lexicon lexicon;
    public Repository repository;
    public HashCodeComparer<Word> hashCodeComparer;
    //public BTreeMap<Word, UInt32> globalWordCount;

    public IndexRoot(ushort nodeSize, SessionBase session)
    {
      hashCodeComparer = new HashCodeComparer<Word>();
      lexicon = new Lexicon(nodeSize, hashCodeComparer, session);
      lexicon.Persist(session, lexicon);
      repository = new Repository(nodeSize, session);
      repository.Persist(session, repository, true);
      //globalWordCount = new BTreeMap<Word, uint>(null, session);
    }

    public override UInt32 PlacementDatabaseNumber
    {
      get
      {
        return PlaceInDatabase;
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
