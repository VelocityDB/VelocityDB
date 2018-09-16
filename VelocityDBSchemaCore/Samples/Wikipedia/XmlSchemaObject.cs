using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Collection.BTree;
using VelocityDb.Session;

namespace VelocityDbSchema.Samples.Wikipedia
{
  public abstract class XmlSchemaObject : OptimizedPersistable
  {
    BTreeSet<string> namespaces;
    public BTreeSet<string> Namespaces
    {
      get
      {
        return namespaces;
      }
      set
      {
        Update();
        namespaces = value;
      }
    }
  }
}
