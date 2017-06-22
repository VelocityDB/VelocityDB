using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using VelocityDb;
using VelocityDb.Collection.BTree;
using VelocityDb.Session;
using VelocityDb.Collection.Comparer;
using System.Diagnostics;
using VelocityDb.Exceptions;

namespace VelocityDbSchema.TextIndexer
{
  public class Document : OptimizedPersistable
  {
    const short indexedFlag = 1; 
    public string _url;
    BTreeMap<UInt32, UInt32> m_wordHit;
    UInt64 _documentTextId;
    Int16 _flags = 0;

    public Document(UInt64 id): base(id) {} // for lookups

    public Document(string url, IndexRoot indexRoot, SessionBase session)
    {
      _url = url;
      m_wordHit = new BTreeMap<UInt32, UInt32>(null, session, 50000);
    }

    public override bool AllowOtherTypesOnSamePage => false;

    public override string ToString()
    {
      return _url + " " + Oid.AsString(Id);
    }

    public DocumentText Content
    {
      get
      {
        return Session.Open<DocumentText>(_documentTextId);
      }
      set
      {
        Update();
        _documentTextId = value.Id;
      }
    }

    public bool Indexed
    {
      get
      {
        return (_flags & indexedFlag) > 0;
      }
      set
      {
        Update();
        if (value)
          _flags |= indexedFlag;
        else
          _flags &= ~indexedFlag;
      }
    }

    public string Name
    {
      get
      {
        return _url;
      }
    }

    public override UInt64 Persist(Placement place, SessionBase session, bool persistRefs = true, bool disableFlush = false, Queue<IOptimizedPersistable> toPersist = null)
    {
        base.Persist(place, session, false, disableFlush, toPersist);
        session.Persist(m_wordHit);
        return Id;
    }

    public int Remove(IndexRoot indexRoot, SessionBase session)
    {
      if (Id == 0)
        return -1;
      foreach (KeyValuePair<UInt32, UInt32> pair in m_wordHit)
      {
        if (pair.Value > 0) // somehow empty wordHit maps may appaer (need to fix)
        {
          uint occurances = (uint)pair.Value;
          indexRoot.Lexicon.UpdateGlobalCount(pair.Key, (int) -occurances);
          if (indexRoot.Lexicon.IdToGlobalCount[pair.Key] == 0)
          {
            indexRoot.Lexicon.RemoveToken(pair.Key);
            indexRoot.Lexicon.TokenMap.Remove(pair.Key);
            indexRoot.Lexicon.IdToGlobalCount.Remove(pair.Key);
          }
          else
            indexRoot.Lexicon.TokenMap[pair.Key].Remove(this);
       }
       }
      int index = 0;
      var itr = indexRoot.Repository.DocumentSet.Iterator();
      itr.GoTo(this);
      while (itr.MovePrevious())
        ++index;
      indexRoot.Repository.DocumentSet.Remove(this);
      m_wordHit?.Clear();
      m_wordHit?.Unpersist(session);
      base.Unpersist(session);
      return index;
    }

    public BTreeMap<UInt32, UInt32> WordHit => m_wordHit;

    //public override void InitializeAfterRead(SessionBase session)
    //{
    //  base.InitializeAfterRead(session);
    //}
  }
}
