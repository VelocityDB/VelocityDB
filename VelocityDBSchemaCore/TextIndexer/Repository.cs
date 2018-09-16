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
    BTreeSet<Document> _documentSet;

    public Repository(ushort nodeSize, SessionBase session)   
    {
      _documentSet = new BTreeSet<Document>(null, session, nodeSize);
    }

    public override bool AllowOtherTypesOnSamePage
    {
      get
      {
        return false;
      }
    }

    public BTreeSet<Document> DocumentSet => _documentSet;
  }
}
