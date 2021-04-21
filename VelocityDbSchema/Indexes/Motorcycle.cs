using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Collection;
using VelocityDb.Indexing;

namespace VelocityDbSchema.Indexes
{
  [Index]
  public class Motorcycle : Vehicle
  {
    static Random rand = new Random(5);
    [Index]
    double cc = rand.NextDouble();

    public Motorcycle() : base(DateTime.MaxValue)
    { }

    WeakIOptimizedPersistableReference<VelocityDbList<DataBaseFileEntry>> childrenWeakReference;
    public double CC
    {
      get
      {
        return cc;
      }
      set
      {
        Update();
        cc = value;
      }
    }
    public void InitChildrenIfNecessary()
    {
      if (childrenWeakReference == null)
      {
        var childrenList = new VelocityDbList<DataBaseFileEntry>();
        childrenList.Persist(Session, this);
        childrenWeakReference = new WeakIOptimizedPersistableReference<VelocityDbList<DataBaseFileEntry>>(childrenList);
        Update();
      }
    }

    public void AddChild(DataBaseFileEntry dataBaseFileEntry)
    {
      InitChildrenIfNecessary();
      var list = childrenWeakReference.GetTarget(true, Session);
      list.Add(dataBaseFileEntry);
      list.Update();
    }

    public void RemoveChild(DataBaseFileEntry dataBaseFileEntry)
    {
      InitChildrenIfNecessary();
      var list = childrenWeakReference.GetTarget(true, Session);
      list.Remove(dataBaseFileEntry);
      list.Update();
    }

    public VelocityDbList<DataBaseFileEntry> Children
    {
      get
      {
        if (childrenWeakReference != null)
          return childrenWeakReference.GetTarget(true, Session);
        return null;
      }
    }
  }

  public class MotorcycleList : OptimizedPersistable
  {
    VelocityDbList<Motorcycle> _motorcycles;

    public MotorcycleList()
    {
      _motorcycles = new VelocityDbList<Motorcycle>();
    }
    public VelocityDbList<Motorcycle> Motorcycles
    {
      get { return _motorcycles; }
    }
  }
}
