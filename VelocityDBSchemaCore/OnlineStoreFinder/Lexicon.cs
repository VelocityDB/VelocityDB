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
  public class Lexicon<T> : OptimizedPersistable where T:IComparable
  {
    BTreeMap<T, UInt32> _valueToId;
    BTreeMap<UInt32, T> _IdToValue;
    UInt32 _nextId;

    BTreeMap<UInt32, BTreeSet<StoreBase>> _tokenMap;

    public Lexicon(SessionBase session, HashCodeComparer<TokenType<T>> hashCodeComparer)
    {
      _valueToId = new BTreeMap<T, UInt32>(null, session);
      _IdToValue = new BTreeMap<UInt32, T>(null, session);
      _nextId = 0;
      _tokenMap = new BTreeMap<UInt32, BTreeSet<StoreBase>>(null, session);
    }

    public override CacheEnum Cache => CacheEnum.Yes;
    public UInt32 PossiblyAddToken(T token)
    {
      UInt32 id;
      if (!ValueToId.TryGetValue(token, out id))
      {
        Update();
        id = ++_nextId;
        _IdToValue[id] = token;
        _valueToId[token] = id;
      }
      return id;
    }
    public BTreeMap<T, UInt32> ValueToId => _valueToId;
    BTreeMap<UInt32, T> IdToValue => _IdToValue;

    public BTreeMap<UInt32, BTreeSet<StoreBase>> TokenMap => _tokenMap;
  }
}
