using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Collection.BTree;
using VelocityDb.Session;

namespace VelocityDbSchema.OnlineStoreFinder
{
  public class CjStore : StoreBase
  {
      public CjStore(string storeName, SessionBase session)
          : base(storeName, session)
    {
    }
  }
}
