using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Collection.BTree;
using VelocityDb.Session;
using VelocityDb.Collection.Comparer;

namespace VelocityDbSchema.OnlineStoreFinder
{
  public abstract class StoreBase : OptimizedPersistable
  {
    const short indexedFlag = 1;
    string m_storeName;
    Int16 m_flags = 0;
    WeakIOptimizedPersistableReference<BTreeSet<Product>> m_productSet;
    WeakIOptimizedPersistableReference<BTreeMap<UInt32, TokenStoreHit>> m_tokenStoreHit;

    public StoreBase(string storeName, SessionBase session)
    {
      this.m_storeName = storeName;
    }

    public override bool AllowOtherTypesOnSamePage
    {
      get
      {
        return false;
      }
    }

    public override CacheEnum Cache => CacheEnum.Yes;

    public bool Indexed
    {
      get
      {
        return (m_flags & indexedFlag) > 0;
      }
      set
      {
        Update();
        if (value)
          m_flags |= indexedFlag;
        else
          m_flags &= ~indexedFlag;
      }
    }

    public BTreeSet<Product> ProductSet
    {
      get
      {
        if (m_productSet == null)
        {
          Update();
          BTreeSet<Product> products = new BTreeSet<Product>(null, Session);
          Session.Persist(products);
          m_productSet = new WeakIOptimizedPersistableReference<BTreeSet<Product>>(products);
          return products;
        }
        return m_productSet.GetTarget(false, Session);
      }
    }

    public BTreeMap<UInt32, TokenStoreHit> TokenStoreHit
    {
      get
      {
        if (m_tokenStoreHit == null)
        {
          BTreeMap<UInt32, TokenStoreHit> tokenStoreHitMap = new BTreeMap<UInt32, TokenStoreHit>(null, Session);
          Session.Persist(tokenStoreHitMap);
          Update();
          m_tokenStoreHit = new WeakIOptimizedPersistableReference<BTreeMap<UInt32, TokenStoreHit>>(tokenStoreHitMap);
          return tokenStoreHitMap;
        }
        return m_tokenStoreHit.GetTarget(false, Session);
      }
    }

    //public BTreeMapOidShort<string, TokenStoreHit> TokenMap
    //{
    //  get
    //  {
    //    if (m_tokenMap == null)
    //    {
    //      Update();
    //      HashCodeComparer<TokenType<string>> hashCodeComparer = new HashCodeComparer<TokenType<string>>();
    //      BTreeMapOidShort<string, TokenStoreHit> tokens = new BTreeMapOidShort<string, TokenStoreHit>(null, Session);
    //      Placement otherPlace = new Placement(DatabaseNumber, PageNumber, 1, 10000, UInt16.MaxValue, true);
    //      otherPlace.TrySlotNumber = 1;
    //      otherPlace.TryPageNumber += 2;
    //      tokens.Persist(otherPlace, Session, true, true, null);
    //      m_tokenMap = new WeakIOptimizedPersistableReference<BTreeMapOidShort<string, TokenStoreHit>>(tokens);
    //      return tokens;
    //    }
    //    return m_tokenMap.GetTarget(false, Session);
    //  }
    //}

    public string StoreName
    {
      get
      {
        return m_storeName;
      }
    }
  }
}
