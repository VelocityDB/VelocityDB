using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using VelocityDb.Collection.Comparer;

namespace VelocityDbSchema.OnlineStoreFinder
{
  [Serializable]
  public class TokenInProductComparer : VelocityDbComparer<TokenInProduct>
  {
    public override int Compare(TokenInProduct a, TokenInProduct b)
    {
      return a.ProductOid.CompareTo(b.ProductOid);
    }
  }
}

