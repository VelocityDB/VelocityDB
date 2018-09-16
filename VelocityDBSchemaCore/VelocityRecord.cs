using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;

namespace VelocityDbSchema
{
  public class VelocityRecord : OptimizedPersistable
  {
      private byte[] record;

      public VelocityRecord(byte[] record)
      {
        this.record = record;
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
