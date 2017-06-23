using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Collection.Comparer;
using VelocityDb.Collection.BTree;
using VelocityDb.Session;

namespace VelocityDbSchema.TextIndexer
{
  public class Lexicon<T> : OptimizedPersistable where T : IComparable
  {
    BTreeMap<T, UInt32> _valueToId;
    BTreeMap<UInt32, T> _IdToValue;
    BTreeMap<UInt32, UInt32> _idToGlobalCount;
    UInt32 _nextId;
    BTreeMap<UInt32, BTreeSet<Document>> _tokenMap;
    public Lexicon(ushort nodeSize, SessionBase session)
    {
      _valueToId = new BTreeMap<T, UInt32>(null, session);
      _IdToValue = new BTreeMap<UInt32, T>(null, session);
      _idToGlobalCount = new BTreeMap<uint, uint>(null, session);
      _nextId = 0;
      _tokenMap = new BTreeMap<UInt32, BTreeSet<Document>>(null, session);
    }

    public BTreeMap<T, UInt32> ValueToId => _valueToId;
    public BTreeMap<UInt32, T> IdToValue => _IdToValue;

    public BTreeMap<UInt32, BTreeSet<Document>> TokenMap => _tokenMap;

    public BTreeMap<UInt32, UInt32> IdToGlobalCount => _idToGlobalCount;

    public UInt32 PossiblyAddToken(T token, Document doc)
    {
      UInt32 id;
      BTreeSet<Document> docSet;
      if (!ValueToId.TryGetValue(token, out id))
      {
        Update();
        id = ++_nextId;
        _IdToValue.AddFast(id, token);
        _valueToId.Add(token, id);
        docSet = new BTreeSet<Document>();
        _tokenMap.AddFast(id, docSet);
      }
      else
        docSet = _tokenMap[id];
      UInt32 wordHit;
      if (!doc.WordHit.TryGetValue(id, out wordHit))
      {
        docSet.AddFast(doc);
        doc.WordHit.Add(id, 1);
      }
      else
        doc.WordHit[id] = ++wordHit;
      AddToGlobalCount(id, 1);
      return id;
    }

    public void RemoveToken(T token)
    {
      UInt32 id;
      if (ValueToId.TryGetValue(token, out id))
      {
        _IdToValue.Remove(id);
        _valueToId.Remove(token);
        var v = _tokenMap[id];
        v.Unpersist(Session);
        _tokenMap.Remove(id);
      }
    }

    public void RemoveToken(UInt32 id)
    {
      T token;
      if (_IdToValue.TryGetValue(id, out token))
      {
        _IdToValue.Remove(id);
        _valueToId.Remove(token);
        var v = _tokenMap[id];
        v.Unpersist(Session);
        _tokenMap.Remove(id);
      }
    }

    public void ReduceGlobalCount(UInt32 id, UInt32 countReduce)
    {
      UInt32 count;
      if (!_idToGlobalCount.TryGetValue(id, out count))
        _idToGlobalCount[id] = countReduce;
      else
        _idToGlobalCount[id] = count - countReduce;
    }

    public void AddToGlobalCount(UInt32 id, UInt32 countAdd)
    {
      UInt32 count;
      if (!_idToGlobalCount.TryGetValue(id, out count))
        _idToGlobalCount[id] = countAdd;
      else
        _idToGlobalCount[id] = count + countAdd;
    }

    public override CacheEnum Cache => CacheEnum.Yes;
  }
}
