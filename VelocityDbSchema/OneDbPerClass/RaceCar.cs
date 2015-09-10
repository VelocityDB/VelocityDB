using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;

namespace VelocityDbSchema.OneDbPerClass
{
  public class RaceCar : OptimizedPersistable
  {
    double speed;

    public double Speed
    {
      get
      {
        return speed;
      }
      set
      {
        Update();
        speed = value;
      }
    }
  }
}
