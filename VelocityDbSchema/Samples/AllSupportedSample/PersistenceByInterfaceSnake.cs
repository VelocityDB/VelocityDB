using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.TypeInfo;
using VelocityDb.Session;
using VelocityDb.Exceptions;

namespace VelocityDbSchema.Samples.AllSupportedSample
{
  public class PersistenceByInterfaceSnake : Pet, IOptimizedPersistable
  {
    [NonSerialized]
    internal byte m_opFlags;
    [NonSerialized]
    internal Page m_page;
    [NonSerialized]
    internal UInt64 m_id;
    [NonSerialized]
    TypeVersion m_shape;
   
    UInt16 m_lengthCm;
    bool m_poisonous;

    public PersistenceByInterfaceSnake(string aName, short anAge, bool poisonous, UInt16 lengthCm)
      : base(aName, anAge)
    {
      m_lengthCm = lengthCm;
      m_poisonous = poisonous;
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
        if (otherPersistentObject.Id == 0 || m_id == 0)
          throw new PersistedObjectExcpectedException("When comparing IOptimizedPersistable objects, both objects must first be persisted (have a non 0 Id)");
        return m_id.CompareTo(otherPersistentObject.Id);
      }
      else
        throw new ArgumentException("Object is not a IOptimizedPersistable");
    }

    /// <inheritdoc />
    public Page GetPage()
    {
      return m_page;
    }

    /// <inheritdoc />
    public void SetPage(Page page)
    {
      m_page = page;
    }
    /// <inheritdoc />
    public object GetWrappedObject() => this;
    /// <inheritdoc />
    public TypeVersion GetTypeVersion() => m_shape;

    /// <inheritdoc />
    public void SetTypeVersion(TypeVersion typeVersion) => m_shape = typeVersion;

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
        return m_id;
      }
      set
      {
        m_id = value;
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
        return m_page != null;
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
        return (m_opFlags & (byte)OpFlags.IsUpdated) > 0;
      }
      set
      {
        if (value)
          m_opFlags |= (byte)OpFlags.IsUpdated;
        else
          m_opFlags &= (byte)~OpFlags.IsUpdated;
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
        return (m_opFlags & (byte)OpFlags.MembersLoaded) > 0;
      }
      set
      {
        if (value)
          m_opFlags |= (byte)OpFlags.MembersLoaded;
        else
          m_opFlags &= (byte)~OpFlags.MembersLoaded;
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
        return m_page;
      }
      set
      {
        m_page = value;
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
      Shape.PersistRefences(WrappedObject, m_page.PageInfo, this, session, inFlush);
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

    /// <inheritdoc />
    public bool RemovedFromIndices
    {
      get
      {
        return (m_opFlags & (byte)OpFlags.RemovedFromIndices) > 0;
      }
      set
      {
        if (value)
          m_opFlags |= (byte)OpFlags.RemovedFromIndices;
        else
          m_opFlags &= (byte)~OpFlags.RemovedFromIndices;
      }
    }

    public virtual byte[] WriteMe(TypeVersion typeVersion, bool addShapeNumber, PageInfo pageInfo, IOptimizedPersistable owner, SessionBase session, bool inFlush)
    {
      return OptimizedPersistable.WriteMeUsingSchemaReflection(typeVersion, this, addShapeNumber, pageInfo, owner, session, inFlush);
    }

    /// <summary>
    /// Gets the session of this object or null if this object isn't yet persisted.
    /// </summary>
    protected virtual SessionBase Session
    {
      get
      {
        if (this.m_page == null)
          return null;
        return this.m_page.Database.GetSession();
      }
    }

    public IOptimizedPersistable ShallowCopyTo(Page page)
    {
      IOptimizedPersistable copy = (IOptimizedPersistable)this.MemberwiseClone();
      copy.SetPage(page);
      return copy;
    }

    public TypeVersion Shape
    {
      get
      {
        return m_shape;
      }
      set
      {
        m_shape = value;
      }
    }

    /// <summary>
    /// Removes an object from the persistent store and makes the object a transient object. It does not automatically make referenced objects unpersisted. Best way to do so is to override this virtual function in your own classes.
    /// </summary>
    /// <param name="session">The managing session</param>
    public virtual void Unpersist(SessionBase session)
    {
      if (!IsPersistent)
        return;
      if (session == null)
        throw new UnexpectedException("SessionBase parameter passed to Unpersist is null");
      this.Update();
      if (Page != null)
      {
        Page.UnpersistObject(this);
        Page = null;
      }
      Id = 0;
    }

    public bool Update()
    {
      if (IsPersistent)
        return this.Page.Database.GetSession().UpdateObject(this, false, true);
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
