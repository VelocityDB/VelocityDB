using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Collection.BTree;
using VelocityDb.Session;

namespace VelocityDbSchema.TextIndexer 
{
  public class Repository : OptimizedPersistable
  {
    public const UInt32 PlaceInDatabase = 12; 
    public BTreeSet<Document> documentSet;

    public Repository(ushort nodeSize, SessionBase session)   
    {
      documentSet = new BTreeSet<Document>(null, session, nodeSize);
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
  }
}
