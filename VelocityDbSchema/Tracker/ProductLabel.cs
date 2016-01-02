using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Collection;

namespace VelocityDbSchema.Tracker
{
  public class ProductLabel : OptimizedPersistable
  {   
#pragma warning disable 0169
    string m_label;
#pragma warning restore 0169
  }
}
