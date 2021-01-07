using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Exceptions;
using VelocityDb.Indexing;
using VelocityDb.Session;
using VelocityDb.TypeInfo;
using Index = VelocityDb.Indexing.Index;

namespace VelocityDbSchema.NUnit
{
  public class DynamicDictionary : DynamicObject
  {
    #region Fields

    // The inner dictionary.
    protected readonly Dictionary<string, object> Dictionary = new Dictionary<string, object>();

    #endregion Fields

    #region Properties

    // This property stores the dynamic typename
    public virtual string TypeName { get; set; } = nameof(DynamicDictionary);

    // Dictionary-like propertyValue by propertyName Indexer
    public dynamic this[string propertyName]
    {
      get { return Dictionary[propertyName.ToLower()]; }
      set { Dictionary[propertyName.ToLower()] = value; }
    }

    // This property returns the number of elements
    // in the inner dictionary.
    public int Count
    {
      get { return Dictionary.Count; }
    }

    #endregion Properties

    #region Methods

    // Used to iterate over all the properties
    public IEnumerable<string> GetPropertyNames()
    {
      foreach (var kvp in Dictionary)
        yield return kvp.Key;
    }

    // Check if a property exists or not
    public bool ContainsProperty(string propertyName)
    {
      return Dictionary.ContainsKey(propertyName.ToLower());
    }

    #endregion Methods

    #region DynamicObject Overrides

    // If you try to get a value of a property 
    // not defined in the class, this method is called.
    public override bool TryGetMember(GetMemberBinder binder, out object result)
    {
      // Converting the property name to lowercase
      // so that property names become case-insensitive.
      string name = binder.Name.ToLower();

      // If the property name is found in a dictionary,
      // set the result parameter to the property value and return true.
      // Otherwise, return false.
      return Dictionary.TryGetValue(name, out result);
    }

    // If you try to set a value of a property that is
    // not defined in the class, this method is called.
    public override bool TrySetMember(SetMemberBinder binder, object value)
    {
      // Converting the property name to lowercase
      // so that property names become case-insensitive.
      Dictionary[binder.Name.ToLower()] = value;

      // You can always add a value to a dictionary,
      // so this method always returns true.
      return true;
    }

    #endregion DynamicObject Overrides
  }

  [Index("_typeName")]
  public class PersistableDynamicDictionary : DynamicDictionary, IOptimizedPersistable
  {
    #region Fields

    [NonSerialized]
    private byte _opFlags;
    [NonSerialized]
    private Page _page;
    [NonSerialized]
    private UInt64 _id;
    [NonSerialized]
    private TypeVersion _shape;

    // Can't put Field Indexes when not Inheriting from OptimizedPersistable
    private string _typeName = nameof(PersistableDynamicDictionary);

    [Index]
    DateTime m_creationTime = DateTime.UtcNow;

    #endregion Fields

    #region Properties

    /// <summary>
    /// Gets the session of this object or null if this object isn't yet persisted.
    /// </summary>
    public virtual SessionBase Session => _page?.Database.GetSession();

    [FieldAccessor("m_creationTime")]
    public DateTime CreationTime
    {
      get { return m_creationTime; }
      set
      {
        Update();
        m_creationTime = value;
      }
    }   

    // This property stores the dynamic typename
    [FieldAccessor("_typeName")]
    public override string TypeName
    {
      get { return _typeName; }
      set
      {
        Update();

        _typeName = value;
      }
    }

    #endregion Properties

    #region IOptimizedPersistable Implementation

    public bool AllowOtherTypesOnSamePage => true;

    public bool FieldsLoaded
    {
      get { return (_opFlags & (byte)OpFlags.MembersLoaded) > 0; }
      set
      {
        if (value)
          _opFlags |= (byte)OpFlags.MembersLoaded;
        else
          _opFlags &= (byte)~OpFlags.MembersLoaded;
      }
    }

    /// <summary>
    /// By default we flush (write) any updated page we find when looking for an object placement page and the page is considered full (depends on how many objects we permit/page)
    /// </summary>
    public bool FlushIfPageFull => true;

    public ulong Id
    {
      get { return _id; }
      set { _id = value; }
    }

    public bool IsPersistent => _page != null;

    /// <summary>
    /// Gets the updated state of the object
    /// </summary>
    /// <value>true if updated; otherwise false</value>
    public bool IsUpdated
    {
      get { return (_opFlags & (byte)OpFlags.IsUpdated) > 0; }
      set
      {
        if (value)
          _opFlags |= (byte)OpFlags.IsUpdated;
        else
          _opFlags &= (byte)~OpFlags.IsUpdated;
      }
    }

    public bool LazyLoadFields => false;

    /// <summary>
    /// Possibly restrict instances of to a single Database. By default this property is UInt32.MaxValue but classes like BTreeSetOidShort, BTreeMapShortOid ... override this property to return 1 since short references are restricted to a single Database.
    /// </summary>
    public uint MaxNumberOfDatabases => UInt32.MaxValue;

    public ushort ObjectsPerPage => 40000;

    /// <summary>
    /// A default for number of objects per database page used when persiting objects without an explicit <see cref="Placement"/> object or if persistet using <see cref="OptimizedPersistable.Persist(SessionBase, OptimizedPersistable, bool, bool)"/>
    /// This happens when objects are persited by reachability from a persistent object.
    /// All objects reachable from a persistent object are automaticly made persistent.
    /// </summary> 
    /// <returns>The requested number of pages per database</returns>
    public ushort PagesPerDatabase => SessionBase.DefaultNumberOfObjectsPerPage;

    /// <summary>
    /// Gets the Database Id number to use when placing (persisting) an instance of this class when no other placement directive has been given.
    /// </summary>
    public uint PlacementDatabaseNumber => Placement.DefaultPlacementDatabaseNumber;

    /// <inheritdoc />
    public bool RemovedFromIndices
    {
      get { return (_opFlags & (byte)OpFlags.RemovedFromIndices) > 0; }
      set
      {
        if (value)
          _opFlags |= (byte)OpFlags.RemovedFromIndices;
        else
          _opFlags &= (byte)~OpFlags.RemovedFromIndices;
      }
    }

    protected TypeVersion Shape
    {
      get { return _shape; }
      set { _shape = value; }
    }

    [Obsolete("Ignore this one, will be removed!")]
    public object WrappedObject => this;

    CacheEnum IOptimizedPersistable.Cache => CacheEnum.No;

    public int CompareTo(object obj)
    {
      IOptimizedPersistable otherPersistentObject = obj as IOptimizedPersistable;

      if (otherPersistentObject != null)
      {
        if (otherPersistentObject.Id == 0 || _id == 0)
          throw new PersistedObjectExcpectedException(
              "When comparing IOptimizedPersistable objects, both objects must first be persisted (have a non 0 Id)");

        return _id.CompareTo(otherPersistentObject.Id);
      }
      else
        throw new ArgumentException("Object is not a IOptimizedPersistable");
    }
    /// <inheritdoc />
    public Page GetPage()
    {
      return _page;
    }

    /// <inheritdoc />
    public void SetPage(Page page)
    {
      _page = page;
    }

    /// <inheritdoc />
    public TypeVersion GetTypeVersion() => _shape;

    /// <inheritdoc />
    public void SetTypeVersion(TypeVersion typeVersion) => _shape = typeVersion;
    /// <inheritdoc />
    public object GetWrappedObject() => this;

    public void FlushTransients()
    {
    }

    /// <summary>
    /// This function is called when an object has been read from disk and all data members (fields) have been loaded. Override this to provide your own initializtions of transient data.
    /// </summary>
    /// <param name="session">The active session managing this object</param>
    public void InitializeAfterRead(SessionBase session)
    {
    }

    /// <summary>
    /// This function is called when an object has been read from disk before all data members (fields) have been fully loaded. Override this to provide your own initializtions of transient data.
    /// </summary>
    /// <param name="session">The active session managing this object</param>
    public void InitializeAfterRecreate(SessionBase session)
    {
    }

    /// <summary>
    /// Persists this object.
    /// </summary>
    /// <param name="place">The placement rules to follow when persisting this object</param>
    /// <param name="session">The session managing this object</param>
    /// <param name="persistRefs">If true, objects referenced from this object will also be persisted</param>
    /// <param name="disableFlush">If true, disables possible flushing of updated pages while persisting this object; otherwise pasge flushing may occur</param>
    /// <returns>The object id of the persistent object</returns>
    public ulong Persist(SessionBase session, IOptimizedPersistable placeHint, bool persistRefs = true,
        bool disableFlush = false)
    {
      Placement place = new Placement(session, placeHint, this, persistRefs, UInt32.MaxValue,
          placeHint.FlushIfPageFull);

      return session.Persist(place, this, session.OpenSchema(false), UInt16.MaxValue - 1, disableFlush);
    }

    /// <summary>
    /// Persists this object.
    /// </summary>
    /// <param name="placeHint">Use placement as specified by this object type, see <see cref="OptimizedPersistable.PlacementDatabaseNumber"/>, <see cref="OptimizedPersistable.ObjectsPerPage()"/> and <see cref="OptimizedPersistable.PagesPerDatabase()"/></param>
    /// <param name="session">The session managing this object</param>
    /// <param name="persistRefs">Persist any referenced object now or delay until flush/commit</param>
    /// <param name="disableFlush">Controlls possible flushing of updated pages. Set to true if you want to prevent updated pages from being flushed to disk and setting such pages to a non updated state.</param>
    /// <returns>The object id of the persistent object</returns>
    public ulong Persist(Placement place, SessionBase session, bool persistRefs = true, bool disableFlush = false,
        Queue<IOptimizedPersistable> toPersist = null)
    {
      return session.Persist(place, this, session.OpenSchema(false), place.MaxObjectsPerPage, disableFlush,
          toPersist);
    }

    public void PersistMyReferences(SessionBase session, bool inFlush)
    {
      Shape.PersistRefences(GetWrappedObject(), _page.PageInfo, this, session, inFlush);
    }

    public void ReadMe(TypeVersion typeVersion, byte[] memberBytes, ref int offset, SessionBase session, Page page,
        bool useOidShort, Schema schema, bool openRefs, List<IOptimizedPersistable> toLoadMembers, int graphDepth,
        int graphDepthToLoad, bool primitivesOnly)
    {
      OptimizedPersistable.ReadMeUsingSchemaReflection(typeVersion, memberBytes, ref offset,
          (IOptimizedPersistable)this, session, page, useOidShort, schema, openRefs, toLoadMembers, graphDepth,
          graphDepthToLoad, primitivesOnly);
    }

    public IOptimizedPersistable ShallowCopyTo(Page page)
    {
      IOptimizedPersistable copy = (IOptimizedPersistable)this.MemberwiseClone();

      copy.SetPage(page);

      return copy;
    }

    /// <summary>
    /// Removes an object from the persistent store and makes the object a transient object. It does not automatically make referenced objects unpersisted. Best way to do so is to override this virtual function in your own classes.
    /// </summary>
    /// <param name="session">The managing session</param>
    public void Unpersist(SessionBase session)
    {
      if (!IsPersistent)
        return;

      if (session == null)
        throw new UnexpectedException("SessionBase parameter passed to Unpersist is null");

      this.Update();

      if (GetPage() != null)
      {
        GetPage().UnpersistObject(this);
        SetPage(null);
      }

      Id = 0;
    }

    public bool Update()
    {
      return !IsPersistent || GetPage().Database.GetSession().UpdateObject(this, false, true);
    }

    public byte[] WriteMe(TypeVersion typeVersion, bool addShapeNumber, PageInfo pageInfo,
        IOptimizedPersistable owner, SessionBase session, bool inFlush)
    {
      return OptimizedPersistable.WriteMeUsingSchemaReflection(typeVersion, this, addShapeNumber, pageInfo, owner,
          session, inFlush);
    }

    #endregion IOptimizedPersistable Implementation
  }

}
