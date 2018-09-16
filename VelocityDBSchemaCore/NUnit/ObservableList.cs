using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using VelocityDb.Collection;
using VelocityDb.Session;

namespace VelocityDbSchema.NUnit
{
  //public delegate void ProcessBookDelegate(Person p);
  public class ObservableList<T> : VelocityDbList<T>, INotifyCollectionChanged
  {
    [field: NonSerializedAttribute()]
    public event NotifyCollectionChangedEventHandler CollectionChanged;
    

    public override void Add(T item)
    {
      base.Add(item);
      CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
    }

    public override bool Remove(T item)
    {
      bool result = base.Remove(item);
      if (result)
      {
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
      }
      return result;
    }

    public override void InitializeAfterRead(SessionBase session)
    {
      base.InitializeAfterRead(session);

    }
  }

}
