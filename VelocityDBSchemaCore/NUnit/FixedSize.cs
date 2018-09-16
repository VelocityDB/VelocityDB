using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using VelocityDb;
using VelocityDb.Session;

namespace VelocityDbSchema.NUnit
{
  public class FixedSize : OptimizedPersistable
  {
    public const UInt32 PlaceInDatabase = 878;

    Int32 m_myInt32;
    public Int32 MyInt32
    {
      get
      {
        return m_myInt32;
      }
      set
      {
        Update();
        m_myInt32 = value;
      }
    }

    bool m_myBool;
    public bool MyBool
    {
      get
      {
        return m_myBool;
      }
      set
      {
        Update();
        m_myBool = value;
      }
    }

    public FixedSize()
    {
      m_myInt32 = 7878787;
      m_myBool = true;
    }

    public override bool AllowOtherTypesOnSamePage
    {
      get
      {
        return false;
      }
    }

    public override UInt32 PlacementDatabaseNumber
    {
      get
      {
        return PlaceInDatabase;
      }
    }
  }
}
