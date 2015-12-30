using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.TypeInfo;
using VelocityDb.Session;

namespace VelocityDbSchema.Samples.AllSupportedSample
{
  public class PersistenceByInterfaceSnake : Pet, IOptimizedPersistable
  {
    [NonSerialized]
    internal byte opFlags;
    [NonSerialized]
    internal Page page;
    [NonSerialized]
    internal UInt64 id;
    [NonSerialized]
    TypeVersion shape;
   
    UInt16 lengthCm;
    bool poisonous;

    public PersistenceByInterfaceSnake(string aName, short anAge, bool poisonous, UInt16 lengthCm)
      : base(aName, anAge)
    {
      this.lengthCm = lengthCm;
      this.poisonous = poisonous;
    }

    public virtual bool AllowOtherTypesOnSamePage
    {
      get
      {
        return true;
      }
    }
    
    public virtual CacheEnum Cache
    {
      get
      {
        return CacheEnum.No;
      }
    }

    public virtual int CompareTo(object obj)
    {
      IOptimizedPersistable otherPersistentObject = obj as IOptimizedPersistable;
      if (otherPersistentObject != null)
      {
        if (otherPersistentObject.Id == 0 || id == 0)
          throw new PersistedObjectExcpectedException("When comparing IOptimizedPersistable objects, both objects must first be persisted (have a non 0 Id)");
        return id.CompareTo(otherPersistentObject.Id);
      }
      else
        throw new ArgumentException("Object is not a IOptimizedPersistable");
    }

    /// <summary>
    /// By default we flush (write) any updated page we find when looking for an object placement page and the page is considered full (depends on how many objects we permit/page)
    /// </summary>
    public virtual bool FlushIfPageFull
    {
      get
      {
        return true;
      }
    }

    public virtual void FlushTransients()
    {
    }

    public UInt64 Id
    {
      get
      {
        return id;
      }
      set
      {
        id = value;
      }
    }

    /// <summary>
    /// This function is called when an object has been read from disk and all data members (fields) have been loaded. Override this to provide your own initializtions of transient data.
    /// </summary>
    /// <param name="session">The active session managing this object</param>
    public virtual void InitializeAfterRead(SessionBase session)
    {
    }

    /// <summary>
    /// This function is called when an object has been read from disk before all data members (fields) have been fully loaded. Override this to provide your own initializtions of transient data.
    /// </summary>
    /// <param name="session">The active session managing this object</param>
    public virtual void InitializeAfterRecreate(SessionBase session)
    {
    }

    public bool IsPersistent
    {
      get
      {
        return page != null;
      }
    }
    
    /// <summary>
    /// Gets the updated state of the object
    /// </summary>
    /// <value>true if updated; otherwise false</value>
    public bool IsUpdated
    {
      get
      {
        return (opFlags & (byte)OpFlags.isUpdated) > 0;
      }
      set
      {
        if (value)
          opFlags |= (byte)OpFlags.isUpdated;
        else
          opFlags &= (byte)~OpFlags.isUpdated;
      }
    }

    public virtual bool LazyLoadFields
    {
      get
      {
        return false;
      }
    }

    /// <summary>
    /// Loads all fields of an object if they are not already loaded.
    /// </summary>
    /// <param name="depth">Set this if you want to limit the depth of the graph loaded by this open.
    public virtual void LoadFields(int depth = Int32.MaxValue)
    {
      if (FieldsLoaded == false)
      {
        FieldsLoaded = true;
        Schema schema = Session.OpenSchema(false);
        List<IOptimizedPersistable> toLoadMembers = new List<IOptimizedPersistable>(shape.DataMemberArray.Length);
        toLoadMembers.Add(this);
        while (toLoadMembers.Count > 0)
        {
          IOptimizedPersistable toLoad = toLoadMembers[toLoadMembers.Count - 1];
          toLoadMembers.RemoveAt(toLoadMembers.Count - 1);
          toLoad.Shape.LoadMembers(toLoad, schema, toLoadMembers, 0, depth, Session);
        }
        InitializeAfterRead(Session);
      }
    }

    /// <summary>
    /// Possibly restrict instances of to a single Database. By default this property is UInt32.MaxValue but classes like BTreeSetOidShort, BTreeMapShortOid ... override this property to return 1 since short references are restricted to a single Database.
    /// </summary>
    public virtual UInt32 MaxNumberOfDatabases
    {
      get
      {
        return UInt32.MaxValue;
      }
    }
    
    public bool FieldsLoaded
    {
      get
      {
        return (opFlags & (byte)OpFlags.membersLoaded) > 0;
      }
      set
      {
        if (value)
          opFlags |= (byte)OpFlags.membersLoaded;
        else
          opFlags &= (byte)~OpFlags.membersLoaded;
      }
    }

    public virtual UInt16 ObjectsPerPage
    {
      get
      {
        return 40000;
      }
    }
    
    public Page Page
    {
      get
      {
        return page;
      }
      set
      {
        page = value;
      }
    }

    /// <summary>
    /// A default for number of objects per database page used when persiting objects without an explicit <see cref="Placement"/> object or if persistet using <see cref="OptimizedPersistable.Persist(SessionBase, OptimizedPersistable, bool, bool)"/>
    /// This happens when objects are persited by reachability from a persistent object.
    /// All objects reachable from a persistent object are automaticly made persistent.
    /// </summary> 
    /// <returns>The requested number of pages per database</returns>
    public virtual UInt16 PagesPerDatabase
    {
      get
      {
        return SessionBase.DefaultNumberOfObjectsPerPage;
      }
    }

    /// <summary>
    /// Persists this object.
    /// </summary>
    /// <param name="place">The placement rules to follow when persisting this object</param>
    /// <param name="session">The session managing this object</param>
    /// <param name="persistRefs">If true, objects referenced from this object will also be persisted</param>
    /// <param name="disableFlush">If true, disables possible flushing of updated pages while persisting this object; otherwise pasge flushing may occur</param>
    /// <returns>The object id of the persistent object</returns>
    public virtual UInt64 Persist(Placement place, SessionBase session, bool persistRefs = true, bool disableFlush = false, Queue<IOptimizedPersistable> toPersist = null)
    {
      return session.Persist(place, this, session.OpenSchema(false), place.MaxObjectsPerPage, disableFlush, toPersist);
    }

    /// <summary>
    /// Persists this object.
    /// </summary>
    /// <param name="placeHint">Use placement as specified by this object type, see <see cref="OptimizedPersistable.PlacementDatabaseNumber"/>, <see cref="OptimizedPersistable.ObjectsPerPage()"/> and <see cref="OptimizedPersistable.PagesPerDatabase()"/></param>
    /// <param name="session">The session managing this object</param>
    /// <param name="persistRefs">Persist any referenced object now or delay until flush/commit</param>
    /// <param name="disableFlush">Controlls possible flushing of updated pages. Set to true if you want to prevent updated pages from being flushed to disk and setting such pages to a non updated state.</param>
    /// <returns>The object id of the persistent object</returns>
    public virtual UInt64 Persist(SessionBase session, IOptimizedPersistable placeHint, bool persistRefs = true, bool disableFlush = false)
    {
      Placement place = new Placement(session, placeHint, this, persistRefs, UInt32.MaxValue, placeHint.FlushIfPageFull);
      return session.Persist(place, this, session.OpenSchema(false), UInt16.MaxValue - 1, disableFlush);
    }
    public virtual void PersistMyReferences(SessionBase session, bool inFlush)
    {
      Shape.PersistRefences(WrappedObject, page.PageInfo, this, session, inFlush);
    }

    /// <summary>
    /// Gets the Database Id number to use when placing (persisting) an instance of this class when no other placement directive has been given.
    /// </summary>
    public virtual UInt32 PlacementDatabaseNumber
    {
      get
      {
        return Placement.DefaultPlacementDatabaseNumber;
      }
    }

    public void ReadMe(TypeVersion typeVersion, byte[] memberBytes, ref int offset, SessionBase session,
                           Page page, bool useOidShort, Schema schema, bool openRefs, List<IOptimizedPersistable> toLoadMembers,
                           int graphDepth, int graphDepthToLoad, bool primitivesOnly)
    {
      OptimizedPersistable.ReadMeUsingSchemaReflection(typeVersion, memberBytes, ref offset, this, session, page, useOidShort, schema, openRefs, toLoadMembers, graphDepth, graphDepthToLoad, primitivesOnly);
    }

    public virtual byte[] WriteMe(TypeVersion typeVersion, bool addShapeNumber, PageInfo pageInfo, IOptimizedPersistable owner, SessionBase session, bool inFlush)
    {
      return OptimizedPersistable.WriteMeUsingSchemaReflection(typeVersion, this, addShapeNumber, pageInfo, owner, session, inFlush);
    }

    /// <summary>
    /// Gets the session of this object or null if this object isn't yet persisted.
    /// </summary>
    public virtual SessionBase Session
    {
      get
      {
        if (this.page == null)
          return null;
        return this.page.Database.Session;
      }
    }

    public IOptimizedPersistable ShallowCopyTo(Page page)
    {
      IOptimizedPersistable copy = (IOptimizedPersistable)this.MemberwiseClone();
      copy.Page = page;
      return copy;
    }

    public TypeVersion Shape
    {
      get
      {
        return shape;
      }
      set
      {
        shape = value;
      }
    }

    public bool Update(bool disableFlush = false)
    {
      if (page != null && (!IsUpdated || !page.IsUpdated))
        return this.Page.Database.Session.UpdateObject(this, disableFlush);
      return true;
    }

    /// <summary>
    /// Ignore this one, will be removed
    /// </summary>
    public object WrappedObject
    {
      get
      {
        return this;
      }
    }
  }
}
