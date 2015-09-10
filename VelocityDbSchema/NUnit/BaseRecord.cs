using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Collection;

namespace VelocityDbSchema.NUnit
{
  public class BaseRecord : OptimizedPersistable
  {
    private readonly SortedMap<string, object> myFields = new SortedMap<string, object>();
    private readonly WeakIOptimizedPersistableReference<RecordDefinition> myRecordDefinitionRef;
    private readonly object recordDefLock = new object();
    [NonSerialized]
    private RecordDefinition m_myRecordDefinition;
    public RecordDefinition MyRecordDefinition
    {
      get
      {
        return m_myRecordDefinition;
      }
      set
      {
        m_myRecordDefinition = value;
      }
    }

    /// <summary>
    ///   Creates an instance and makes sure properties are defaulted correctly.
    /// </summary>
    /// <param name = "recordType">Type of the record.</param>
    /// <param name = "isRecordDefinition">if set to <c>true</c> [is record definition].</param>
    /// <param name = "recordDefinition">The record definition.</param>

    public BaseRecord(string recordType, bool isRecordDefinition, WeakIOptimizedPersistableReference<RecordDefinition> recordDefinition)
    {
      Behaviors = new System.Collections.Generic.List<OptimizedPersistable>();
      RecordType = recordType;
      IsRecordDefinition = isRecordDefinition;
      myRecordDefinitionRef = recordDefinition;
      When = DateTime.UtcNow;
    }

    /// <summary>
    ///   Holds the set of <see cref = "IBehavior" /> items associated with this record.
    /// </summary>
    public System.Collections.Generic.List<OptimizedPersistable> Behaviors { get; private set; }

    /// <summary>
    ///   Holds when this item was persisted.
    /// </summary>
    public DateTime When { get; set; }

    /// <summary>
    ///   Gets or sets the name.
    /// </summary>
    /// <value>
    ///   This item's name.
    /// </value>
    public string RecordType { get; private set; }

    /// <summary>
    ///   Indicates whether this instance is a record definition object or normal record.
    /// </summary>
    public bool IsRecordDefinition { get; private set; }
  }
}
