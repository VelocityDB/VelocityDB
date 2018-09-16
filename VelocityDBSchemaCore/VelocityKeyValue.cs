using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;

namespace VelocityDbSchema
{
  public class VelocityKeyValue : OptimizedPersistable
  {
    private byte[] key;
    private byte[] record;

    public VelocityKeyValue(byte[] key, byte[] record)
    {
      this.key = key;
      this.record = record;
    }

    public byte[] Key
    {
      get { return key; }
    }

    public byte[] Record
    {
      get { return record; }
    }

    public override bool AllowOtherTypesOnSamePage
    {
      get
      {
        return false;
      }
    }
  }
}
