using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;

namespace VelocityDbSchema.Samples.TriangleCounter
{
  public class EdgeInfo : OptimizedPersistable
  {
    int[] to;

    public EdgeInfo(int b)
    {
      to = new int[1];
      to[0] = b;
    }

    public void Add(int b)
    {
      Update();
      Array.Resize(ref to, to.Length + 1);
      to[to.Length - 1] = b;
    }

    public int[] To
    {
      get
      {
        return to;
      }
    }

    public void ToSort()
    {
      Update();
      Array.Sort(to);
    }

    public override CacheEnum Cache
    {
      get
      {
        return CacheEnum.Yes;
      }
    }
  }
}
