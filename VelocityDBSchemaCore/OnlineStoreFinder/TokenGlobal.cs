using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb.Collection.BTree;
using VelocityDb.Session;

namespace VelocityDbSchema.OnlineStoreFinder
{
    public class TokenGlobal<T> : TokenType<T> where T : IComparable
    {
        public BTreeSet<StoreBase> m_tokenCjStoreHit;
        UInt32 m_globalCount;

        public TokenGlobal(T token, SessionBase session)
            : base(token)
        {
            m_tokenCjStoreHit = new BTreeSet<StoreBase>(null, session, 10000);
        }
        public override BTreeSet<StoreBase> TokenCjStoreHit
        {
            get
            {
                return m_tokenCjStoreHit;
            }
        }

        public override UInt32 GlobalCount
        {
          get
          {
            return m_globalCount;
          }
          set
          {
            Update();
            m_globalCount = value;
          }
        }
    }
}
