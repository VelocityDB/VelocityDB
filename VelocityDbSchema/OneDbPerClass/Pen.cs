using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;

namespace VelocityDbSchema.OneDbPerClass
{
  public class Pen : OptimizedPersistable
  {
    string color;

    public string Color
    {
      get
      {
        return color;
      }
      set
      {
        Update();
        color = value;
      }
    }
  }
}
