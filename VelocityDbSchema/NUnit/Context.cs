using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VelocityDb;
using VelocityDb.Collection.BTree;
using VelocityDb.Collection.BTree.Extensions;
using VelocityDb.Collection.Comparer;
using VelocityDb.Session;

namespace RelSandbox
{
  public interface IContext : IDisposable
  {
    //
    // Generellt
    //
    void EnableTracing(bool b);
    void Checkpoint();

    //
    // Objekt
    //
    IDevice AddDevice(ulong ownerUid);
    IDevice GetDevice(ulong? uid = null, ulong? ownerUid = null);
    IEnumerable<IDevice> GetDevices();
    IArtifactCommunication AddCommunication(string commType, ulong fromDevice, ulong toDevice, DateTime startTime, DateTime endTime);
    IArtifactCommunication GetCommunication(ulong? uid = null);
    IEnumerable<IArtifactCommunication> GetCommunications();
    IArtifactPerson AddPerson(string firstName, string secondName, string surName, string gender, DateTime birthDate);
    IArtifactPerson GetPerson(ulong uid);
    List<IArtifactPerson> GetPersons(string firstName = null, string secondName = null, string surName = null, string gender = null, DateTime? birthDate = null);

    //
    // Noder och relationer
    //
    Node AddNode(string what, ulong artifactUid);
    Node GetNode(ulong? id = null, string what = null, ulong? artifactUid = null);
    List<Node> GetNodes(string what = null, IEnumerable<ulong> artifactUids = null, int? startIndex = null, int? count = null);
    int CountNodes(string what = null, IEnumerable<ulong> artifactUids = null);
    void DeleteNode(Node node);
    void DeleteNodes();
    Relation AddRelation(string how, ulong startNodeId, ulong endNodeId);
    List<Relation> GetRelations(string how = null, ulong? startNodeId = null, ulong? endNodeId = null);
    void DeleteRelation(Relation relation);
    void DeleteRelations();
    List<Node> GetEndNodes(string how, ulong startNodeId);
    List<Node> GetStartNodes(string how, ulong endNodeId);
    List<Node> GetNodesByStartCardinality(string how, int minCardinality, int maxCardinality);
    List<Node> GetNodesByEndCardinality(string how, int minCardinality, int maxCardinality);

    void VerifyRelationEndIndex(ulong endNodeId);
  }

  public class Context : IContext
  {
    SessionBase _session;
    Investigation _investigation;

    #region Construction
    public Context(string path)
    {
      var p = Path.Combine(@"C:\Databases\", path);
      var s = Path.Combine(Path.GetDirectoryName(p), "4.odb");
      var d = Path.Combine(p, "4.odb");

      SessionBase.DefaultStringComparer = StringComparer.Ordinal;

      //            _session = GetSessionPool(p).GetSession(out _);
      _session = new SessionNoServerShared(p);
      _session.BeginUpdate(true);
      _session.RegisterClass(typeof(Investigation));
      _session.RegisterClass(typeof(Device));
      _session.RegisterClass(typeof(ArtifactCommunication));
      _session.RegisterClass(typeof(ArtifactPerson));
      _session.RegisterClass(typeof(Node));
      _session.RegisterClass(typeof(Relation));

      if (File.Exists(s) && File.Exists(d) == false)        // Om licensen "4.odb" finns under roten, kopiera den till db-katalogen.
        File.Copy(s, d);

      _investigation = _session.AllObjects<Investigation>().FirstOrDefault() ?? SessionPersist(new Investigation());

      Trace.Listeners.Add(new ConsoleTraceListener());
    }
    #endregion

    public void Dispose()
    {
      if (_session.IsDisposed == false)
      {
        while (_session.InCommit)
        {
          Task.Delay(100);
        }

        _session.WaitForIndexUpdates();
        _session.Dispose();
      }
    }

    #region IContext implementation

    public void EnableTracing(bool b)
    {
      _session.TraceIndexUsage = b;
    }

    public void Checkpoint()
    {
      lock (_session)
      {
        _session.Checkpoint();
      }
    }

    public IDevice AddDevice(ulong ownerUid)
    {
      return SessionPersist(new Device
      {
        Uid = ++_investigation.LastUid,
        OwnerUid = ownerUid
      });
    }

    public IDevice GetDevice(ulong? uid = null, ulong? ownerUid = null)
    {
      IEnumerable<Device> e;
      Predicate<Device> p = x =>
          (uid == null || x.Uid == uid) &&
          (ownerUid == null || x.OwnerUid == ownerUid);

      if (uid != null)
      {
        e = _session.Index<Device>("_uid").Where(x => x.Uid == uid);
      }
      else if (ownerUid != null)
      {
        e = _session.Index<Device>("_ownerUid").Where(x => x.OwnerUid == ownerUid);
      }
      else
      {
        e = _session.AllObjects<Device>();
      }

      lock (_session)
      {
        return e.Where(x => p(x)).SingleOrDefault();
      }
    }

    public IEnumerable<IDevice> GetDevices()
    {
      lock (_session)
      {
        return _session.AllObjects<Device>();
      }
    }

    public IArtifactCommunication AddCommunication(string commType, ulong fromDevice, ulong toDevice, DateTime startTime, DateTime endTime)
    {
      return SessionPersist(new ArtifactCommunication
      {
        Uid = ++_investigation.LastUid,
        CommType = commType,
        FromDevice = fromDevice,
        ToDevice = toDevice,
        StartTime = startTime,
        EndTime = endTime
      });
    }

    public IArtifactCommunication GetCommunication(ulong? uid = null)
    {
      IEnumerable<ArtifactCommunication> e;
      Predicate<ArtifactCommunication> p = x =>
          (uid == null || x.Uid == uid);

      if (uid != null)
      {
        e = _session.Index<ArtifactCommunication>("_uid").Where(x => x.Uid == uid);
      }
      else
      {
        e = _session.AllObjects<ArtifactCommunication>();
      }

      lock (_session)
      {
        return e.Where(x => p(x)).SingleOrDefault();
      }
    }

    public IEnumerable<IArtifactCommunication> GetCommunications()
    {
      lock (_session)
      {
        return _session.AllObjects<ArtifactCommunication>();
      }
    }

    public IArtifactPerson AddPerson(string firstName, string secondName, string surName, string gender, DateTime birthDate)
    {
      return SessionPersist(new ArtifactPerson
      {
        Uid = ++_investigation.LastUid,
        FirstName = firstName,
        SecondName = secondName,
        SurName = surName,
        Gender = gender,
        BirthDate = birthDate
      });
    }

    public IArtifactPerson GetPerson(ulong uid)
    {
      var b = _session.Index<ArtifactPerson>("_uid");

      lock (_session)
      {
        return b.Where(x => x.Uid == uid).SingleOrDefault();
      }
    }

    public List<IArtifactPerson> GetPersons(string firstName = null, string secondName = null, string surName = null, string gender = null, DateTime? birthDate = null)
    {
      lock (_session)
      {
        return EnumeratePersons(firstName, secondName, surName, gender, birthDate).Cast<IArtifactPerson>().ToList();
      }
    }

    IEnumerable<ArtifactPerson> EnumeratePersons(string firstName = null, string secondName = null, string surName = null, string gender = null, DateTime? birthDate = null)
    {
      Predicate<ArtifactPerson> pred1 = x => firstName == null || x.FirstName == firstName;
      Predicate<ArtifactPerson> pred2 = x => secondName == null || x.SecondName == secondName;
      Predicate<ArtifactPerson> pred3 = x => surName == null || x.SurName == surName;
      Predicate<ArtifactPerson> pred4 = x => gender == null || x.Gender == gender;
      Predicate<ArtifactPerson> pred5 = x => !birthDate.HasValue || x.BirthDate == birthDate.Value;
      BTreeSet<ArtifactPerson> b;

      if (firstName != null)
      {
        b = _session.Index<ArtifactPerson>("_firstName");
        return b.Where(x => x.FirstName == firstName).Where(x => pred2(x) && pred3(x) && pred4(x) && pred5(x));
      }

      if (secondName != null)
      {
        b = _session.Index<ArtifactPerson>("_secondName");
        return b.Where(x => x.SecondName == secondName).Where(x => pred1(x) && pred3(x) && pred4(x) && pred5(x));
      }

      if (surName != null)
      {
        b = _session.Index<ArtifactPerson>("_surName");
        // Fel antal resultatposter, men index används.
        return b.Where(x => x.SurName == surName).Where(x => pred1(x) && pred2(x) && pred4(x) && pred5(x));

        // Rätt antal resultatposter, men index används inte.
        //return b.Where(x => SessionBase.DefaultStringComparer.Equals(x.SurName, surName)).Where(x => pred1(x) && pred2(x) && pred4(x) && pred5(x));
      }

      return _session.AllObjects<ArtifactPerson>().Where(x => pred1(x) && pred2(x) && pred3(x) && pred4(x) && pred5(x));
    }

    public Node AddNode(string what, ulong artifactUid)
    {
      return SessionPersist(new Node { Id = ++_investigation.LastUid, What = what, ArtifactUid = artifactUid });
    }

    public Node GetNode(ulong? id = null, string what = null, ulong? artifactUid = null)
    {
      lock (_session)
      {
        IEnumerable<Node> e;
        Predicate<Node> p = x =>
            (what == null || x.What == what) &&
            (artifactUid == null || x.ArtifactUid == artifactUid);

        if (id != null)
        {
          e = SessionOpen<Node>(id.Value);
        }
        else if (artifactUid != null)
        {
          e = SessionIndex<Node>("_artifactUid").Where(x => x.ArtifactUid == artifactUid);
        }
        else if (what != null)
        {
          e = SessionIndex<Node>("_what").Where(x => x.What == what);
        }
        else
        {
          e = _session.AllObjects<Node>();
        }

        return e.Where(x => p(x)).SingleOrDefault();
      }
    }

    public List<Node> GetNodes(string what = null, IEnumerable<ulong> artifactUids = null, int? startIndex = null, int? count = null)
    {
      lock (_session)
      {
       // _session.FlushUpdates();
        IEnumerable<Node> e;
        List<ulong> ac = artifactUids?.ToList();
        Predicate<Node> p = x =>
            (what == null || x.What == what) &&
            (ac == null || ac.Contains(x.ArtifactUid));

        if (ac != null)
        {
          var b = SessionIndex<Node>("_artifactUid");
          e = from a in ac select b.Where(x => x.ArtifactUid == a).SingleOrDefault();
        }
        else if (what != null)
        {
          e = SessionIndex<Node>("_what").Where(x => x.What == what);
        }
        else
        {
          e = _session.AllObjects<Node>();
        }

        return e.Where(x => p(x)).Skip(startIndex ?? 0).Take(count ?? int.MaxValue).ToList();
      }
    }

    public int CountNodes(string what = null, IEnumerable<ulong> artifactUids = null)
    {
      lock (_session)
      {
        IEnumerable<Node> e;
        List<ulong> ac = artifactUids?.ToList();
        Predicate<Node> p = x =>
            (what == null || x.What == what) &&
            (ac == null || ac.Contains(x.ArtifactUid));

        if (ac != null)
        {
          var b = SessionIndex<Node>("_artifactUid");
          e = from a in ac select b.Where(x => x.ArtifactUid == a).SingleOrDefault();
        }
        else if (what != null)
        {
          e = SessionIndex<Node>("_what");
        }
        else
        {
          e = _session.AllObjects<Node>();
        }

        return e.Where(x => p(x)).Count();
      }
    }

    public void DeleteNode(Node node)
    {
      _session.Unpersist(node);
    }

    public void DeleteNodes()
    {
      foreach (var x in _session.AllObjects<Node>())
        _session.Unpersist(x);
    }

    public List<Node> GetEndNodes(string how, ulong startNodeId)
    {
      lock (_session)
      {
        return (from r in GetRelations(how: how, startNodeId: startNodeId) select SessionOpen<Node>(r.EndNodeId).Single()).ToList();
      }
    }

    public List<Node> GetStartNodes(string how, ulong endNodeId)
    {
      lock (_session)
      {
        return (from r in GetRelations(how: how, endNodeId: endNodeId) select SessionOpen<Node>(r.StartNodeId).Single()).ToList();
      }
    }

    public List<Node> GetNodesByStartCardinality(string how, int minCardinality, int maxCardinality)
    {
      var nc = new List<Node>();
      //var b = _session.Index<Relation>("_ix1");
      //var r = new Relation { _ix1 = how };
      //var i = b.Iterator();
      //var c = 1;
      //ulong id;

      //i.GoTo(r);
      //r = i.IndexInTree < b.Count ? i.Current() : null;
      //id = r?.StartNodeId ?? 0;

      //while (r != null && r.How == how)
      //{
      //    r = i.Next();

      //    if (r != null && r.How == how && r.StartNodeId == id)
      //    {
      //        c += 1;
      //        continue;
      //    }

      //    if (minCardinality <= c && c <= maxCardinality)
      //        nc.Add(GetNode(id));

      //    c = 1;
      //}

      return nc;
    }

    public List<Node> GetNodesByEndCardinality(string how, int minCardinality, int maxCardinality)
    {
      var nc = new List<Node>();
      //var b = _session.Index<Relation>("_ix2");
      //var r = new Relation { _ix2 = how };
      //var i = b.Iterator();
      //var c = 1;
      //ulong id;

      //i.GoTo(r);
      //r = i.IndexInTree < b.Count ? i.Current() : null;
      //id = r?.EndNodeId ?? 0;

      //while (r != null && r.How == how)
      //{
      //    r = i.Next();

      //    if (r != null && r.How == how && r.EndNodeId == id)
      //    {
      //        c += 1;
      //        continue;
      //    }

      //    if (minCardinality <= c && c <= maxCardinality)
      //        nc.Add(GetNode(id));

      //    r = i.Next();
      //    id = r?.EndNodeId ?? 0;
      //    c = 1;
      //}

      return nc;
    }

    #region Relation
    public Relation AddRelation(string how, ulong startNodeId, ulong endNodeId)
    {
      if (endNodeId == 261993207322)
        Console.WriteLine(261993207322);
      return SessionPersist(new Relation { Id = ++_investigation.LastUid, How = how, StartNodeId = startNodeId, EndNodeId = endNodeId });
    }

    public List<Relation> GetRelations(string how = null, ulong? startNodeId = null, ulong? endNodeId = null)
    {
      IEnumerable<Relation> e;
      Predicate<Relation> p = x =>
          (how == null || x.How == how) &&
          (startNodeId == null || x.StartNodeId == startNodeId) &&
          (endNodeId == null || x.EndNodeId == endNodeId);
      var bTree = _session.Index<Relation>("_startNodeId");
      var bTree2 = _session.Index<Relation>("_endNodeId");
      if (startNodeId != null)
      {
        e = bTree.Where(x => x.StartNodeId == startNodeId);
        //if (bTree != null && (e == null || e.Count() == 0))
         // Console.WriteLine("Unexpected");
      }
      else if (endNodeId != null)
      {
        e = bTree2.Where(x => x.EndNodeId == endNodeId);
        if (bTree2 != null && ( e == null || e.Count() == 0))
          Console.WriteLine("Unexpected");
      }
      else if (how != null)
      {
        e = _session.Index<Relation>("_how").Where(x => x.How == how);
      }
      else
      {
        e = _session.AllObjects<Relation>();
      }

      lock (_session)
      {
        return e.Where(x => p(x)).ToList();
      }
    }

    public void DeleteRelation(Relation relation)
    {
      lock (_session)
      {
        _session.Unpersist(relation);
      }
    }

    public void DeleteRelations()
    {
      lock (_session)
      {
        foreach (var x in _session.AllObjects<Relation>().ToArray())
          _session.Unpersist(x);
      }
    }

    public void VerifyRelationEndIndex(ulong endNodeId)
    {
      var b = _session.Index<Relation>("_endNodeId");
      var sb = new StringBuilder();
      var i = b.Iterator();
      var owner = false;
      Relation prev = null;

      while (i.MoveNext())
      {
        if (i.Current().EndNodeId == endNodeId && (prev == null || prev.EndNodeId != endNodeId))
          sb.Append($" {i.IndexInTree}-");

        if (i.Current().EndNodeId != endNodeId && prev != null && prev.EndNodeId == endNodeId)
          sb.Append($"{i.IndexInTree}");

        if (i.Current().EndNodeId == endNodeId && i.Current().How == "Owner")
          owner = true;

        prev = i.Current();
      }

      var ranges = sb.ToString();   // För vilka index hittade vi endNodeId? Ska bara vara ett område.

      if (owner == false)
      {
        throw new Exception($"Invalid BTreeSet. No Relation found with How = 'Owns' and EndNodeId = {endNodeId}.");
      }
    }
    #endregion

    #endregion

    IEnumerable<T> SessionOpen<T>(params ulong[] oids)
    {
      return SessionOpen<T>((IEnumerable<ulong>)oids);
    }

    IEnumerable<T> SessionOpen<T>(IEnumerable<ulong> oids)
    {
      foreach (var i in oids)
        yield return _session.Open<T>(i);
    }

    IEnumerable<Node> GetNodesFromArtifactsUids(BTreeSet<Node> source, IEnumerable<ulong> uids)
    {
      foreach (var uid in uids)
        yield return source.Where(x => x.ArtifactUid == uid).SingleOrDefault();
    }

    T SessionPersist<T>(T obj)
    {
      lock (_session)
      {
        _session.Persist(obj);
      }

      return obj;
    }

    BTreeSet<T> SessionIndex<T>(string indexByFieldName)
    {
      return _session.Index<T>(indexByFieldName) ?? new BTreeSet<T>(new CompareByField<T>(indexByFieldName, _session), _session);
    }

    IEnumerable<T> GetSubset<T>(BTreeSet<T> source, T startKey, Predicate<T> pred)
    {
      var i = source.Iterator();

      i.GoTo(startKey);

      while (pred(i.Current()))
      {
        yield return i.Current();

        if (i.MoveNext() == false)
          break;
      }
    }
  }
}
