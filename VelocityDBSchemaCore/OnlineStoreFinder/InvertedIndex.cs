using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Session;
using VelocityDb.Collection.Comparer;

namespace VelocityDbSchema.OnlineStoreFinder
{
  public class InvertedIndex : OptimizedPersistable
  {
    Lexicon<string> m_stringLexicon;
    public InvertedIndex(SessionBase session, HashCodeComparer<TokenType<string>> hashCodeComparer) 
    {
      m_stringLexicon = new Lexicon<string>(session, hashCodeComparer);
      session.Persist(m_stringLexicon);
    }

    public override CacheEnum Cache => CacheEnum.Yes;

    public Lexicon<string> Lexicon => m_stringLexicon;
  }
}
