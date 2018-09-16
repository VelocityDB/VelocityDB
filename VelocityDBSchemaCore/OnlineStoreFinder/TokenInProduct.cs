using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Session;

namespace VelocityDbSchema.OnlineStoreFinder
{
  public class TokenInProduct : OptimizedPersistable
  {
    Oid m_product;
    List<UInt16> m_tokenLocations;

    public TokenInProduct(Product product, SessionBase session)
    {
      this.m_product = product.Oid;
      m_tokenLocations = new List<UInt16>();
    }

    public void Add(UInt16 fieldPosition)
    {
      Update();
      m_tokenLocations.Add(fieldPosition);
    }

    public override CacheEnum Cache => CacheEnum.Yes;

    public int Count
    {
      get
      {
        return m_tokenLocations.Count;
      }
    }

    public Oid ProductOid
    {
      get
      {
        return m_product;
      }
    }
  }
}
