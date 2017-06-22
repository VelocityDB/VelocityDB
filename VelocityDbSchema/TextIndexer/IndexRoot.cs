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
    Lexicon<string> _lexicon;
    Repository _repository;
    //public BTreeMap<Word, UInt32> globalWordCount;

    public IndexRoot(ushort nodeSize, SessionBase session)
    {
      _lexicon = new Lexicon<string>(nodeSize, session);
      session.Persist(_lexicon);
      _repository = new Repository(nodeSize, session);
      _repository.Persist(session, _repository, true);
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

    public Lexicon<string> Lexicon => _lexicon;

    public Repository Repository => _repository;
  }
}
