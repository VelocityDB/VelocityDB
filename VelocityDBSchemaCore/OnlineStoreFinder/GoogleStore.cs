using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb.Session;

namespace VelocityDbSchema.OnlineStoreFinder
{
  public class GoogleStore : StoreBase
  {
    public GoogleStore(string storeName, SessionBase session) : base(storeName, session)
    {
    }
  }
}
