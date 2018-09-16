using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Collection.BTree;
using VelocityDb.Session;
using VelocityDb.Collection.Comparer;
using VelocityDb.Exceptions;

namespace VelocityDbSchema.OnlineStoreFinder
{
  public class TokenType<T> : TokenBase where T : IComparable
  {
    //TypeCode typeCode = Type.GetTypeCode(typeof(T));
    T m_token;

    public TokenType(T token)
    {
      m_token = token;
    }

    public override bool AllowOtherTypesOnSamePage
    {
        get
        {
            return false;
        }
    }

    public override string ToString()
    {
      return Oid + " " + m_token.ToString();
    }

    public virtual BTreeSet<StoreBase> TokenCjStoreHit
    {
      get
      {
          throw new UnexpectedException("Should not be used at this level");
      }
    }

    public override int GetHashCode()
    {
        string t = m_token as string;
        return t != null ? CompareByField<T>.GetHashCode32(t) : m_token.GetHashCode();
    }

    public override int CompareTo(object obj)
    {
      TokenType<T> otherToken = obj as TokenType<T>;
      return m_token.CompareTo(otherToken.m_token);
    }

    /// <inheritdoc />
    public override UInt64 Persist(Placement place, SessionBase session, bool persistRefs = true, bool disableFlush = false, Queue<IOptimizedPersistable> toPersist = null)
    {
      if (IsPersistent == false)
      {
        base.Persist(place, session, persistRefs, disableFlush, toPersist);
        if (place.StartDatabaseNumber != DatabaseNumber)
          throw new UnexpectedException();
      }
      return Id;
    }

    public T Token
    {
      get
      {
        return m_token;
      }
    }
  }
}
