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

namespace VelocityDbSchema.TextIndexer
{
  public class Document : OptimizedPersistable
  {
    public const UInt32 PlaceInDatabase = 40;
    const short indexedFlag = 1; 
    public string url;
    BTreeSetOidShort<Word> wordSet;
    BTreeMapOidShort<Word, WordHit> m_wordHit;
    OidShort documentTextShortId;
    Int16 flags = 0;

    public Document(UInt64 id): base(id) {} // for lookups

    public Document(string url, IndexRoot indexRoot, SessionBase session)
    {
      this.url = url;
      HashCodeComparer<Word> hashCodeComparer = new HashCodeComparer<Word>();
      m_wordHit = new BTreeMapOidShort<Word, WordHit>(null, session);
      m_wordHit.TransientBatchSize = 10000;
      wordSet = new BTreeSetOidShort<Word>(hashCodeComparer, session, 1500, sizeof(int));
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
      return url + " " + Oid.AsString(Id);
    }

    public DocumentText Content
    {
      get
      {
        return (DocumentText)Open(documentTextShortId.Id);
      }
      set
      {
        Update();
        documentTextShortId = value.OidShort;
      }
    }

    public bool Indexed
    {
      get
      {
        return (flags & indexedFlag) > 0;
      }
      set
      {
        Update();
        if (value)
          flags |= indexedFlag;
        else
          flags &= ~indexedFlag;
      }
    }

    public string Name
    {
      get
      {
        return url;
      }
    }

    public override UInt64 Persist(Placement place, SessionBase session, bool persistRefs = true, bool disableFlush = false, Queue<IOptimizedPersistable> toPersist = null)
    {
        base.Persist(place, session, false, disableFlush, toPersist);
        Placement otherPlace = new Placement(DatabaseNumber, (ushort) (PageNumber + 1), 1, UInt16.MaxValue - 1, UInt16.MaxValue, true, false, 1, false);
        wordSet.Persist(otherPlace, session, true, disableFlush, toPersist);
        Debug.Assert(DatabaseNumber == wordSet.DatabaseNumber);
        otherPlace.TrySlotNumber = 1;
        ++otherPlace.TryPageNumber;
        m_wordHit.Persist(otherPlace, session, true, disableFlush, toPersist);
        return Id;
    }

    public int Remove(IndexRoot indexRoot, SessionBase session)
    {
      if (Id == 0)
        return -1;
      foreach (KeyValuePair<Word, WordHit> pair in m_wordHit)
      {
        if (pair.Key.DocumentHit.Count > 0) // somehow empty wordHit maps may appaer (need to fix)
        {
          uint occurances = (uint)pair.Value.Count;
          pair.Key.GlobalCount = pair.Key.GlobalCount - occurances;
          if (pair.Key.GlobalCount == 0)
          {
            indexRoot.lexicon.WordSet.Remove(pair.Key);
            if (pair.Key.DocumentHit.Count > 1)
              throw new UnexpectedException("When globalCount is 0, then only this single doc should remain for the word");
            pair.Key.DocumentHit.Unpersist(session);
            pair.Key.Unpersist(session);
          }
          else
            pair.Key.DocumentHit.Remove(this);
       }
       }
      int index = 0;
      var itr = indexRoot.repository.documentSet.Iterator();
      itr.GoTo(this);
      while (itr.MovePrevious())
        ++index;
      indexRoot.repository.documentSet.Remove(this);
      m_wordHit.Clear();
      m_wordHit.Unpersist(session);
      base.Unpersist(session);
      return index;
    }

    public BTreeMapOidShort<Word, WordHit> WordHit
    {
      get
      {
        return m_wordHit;
      }
    }

    public BTreeSetOidShort<Word> WordSet
    {
      get
      {
        return wordSet;
      }
    }
  }
}
