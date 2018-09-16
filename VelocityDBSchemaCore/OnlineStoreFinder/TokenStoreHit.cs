using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb.Session;
using VelocityDb;
using VelocityDb.Collection.BTree;
using VelocityDb.Collection.Comparer;
using VelocityDb.Collection;
using VelocityDb.TypeInfo;

namespace VelocityDbSchema.OnlineStoreFinder
{
  public class TokenStoreHit : OptimizedPersistable
  {
    WeakIOptimizedPersistableReference<BTreeSet<Oid>> m_tokenInProduct;

    public TokenStoreHit(SessionBase session)
    {
      var tree = new BTreeSet<Oid>();
      session.Persist(tree);
      session.Persist(this);
      m_tokenInProduct = new WeakIOptimizedPersistableReference<BTreeSet<Oid>>(tree);
    }
    public void Add(Oid productId)
    {
      m_tokenInProduct.GetTarget(false, Session).AddFast(productId);
    }

    /// <inheritdoc />
    public override bool AllowOtherTypesOnSamePage
    {
      get
      {
        return false;
      }
    }

    public override CacheEnum Cache => CacheEnum.Yes;

    public int Count
    {
      get
      {
        return m_tokenInProduct.GetTarget(false, Session).Count;
      }
    }

    public BTreeSet<Oid> TokenInProduct
    {
      get
      {
        return m_tokenInProduct.GetTarget(false, Session);
      }
    }
  }
}
