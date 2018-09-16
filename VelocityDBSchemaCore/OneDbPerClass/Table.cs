using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;

namespace VelocityDbSchema.OneDbPerClass
{
  public class Table : OptimizedPersistable
  {
    float height;

    public float Height
    {
      get
      {
        return height;
      }
      set
      {
        Update();
        height = value;
      }
    }
  }
}