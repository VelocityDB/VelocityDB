using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb.Session;

namespace VelocityDbSchema.OnlineStoreFinder
{
  public class ShareasaleStore : StoreBase
  {
    public ShareasaleStore(string storeName, SessionBase session)
      : base(storeName, session)
    {
    }
  }
}
