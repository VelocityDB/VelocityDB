using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb.Collection.BTree;
using VelocityDb.Session;

namespace VelocityDbSchema.TextIndexer
{
    public class WordGlobal : Word
    {
        BTreeSet<Document> documentHit;
        UInt32 m_globalCount;

        public WordGlobal(string word, SessionBase session, UInt32 count):base(word)
        {
            m_globalCount = count;
            documentHit = new BTreeSet<Document>(null, session, 10000);
        }

        public override BTreeSet<Document> DocumentHit
        {
            get
            {
                return documentHit;
            }
        }

        public override bool AllowOtherTypesOnSamePage
        {
          get
          {
            return false;
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
