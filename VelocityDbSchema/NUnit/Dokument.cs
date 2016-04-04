using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;

namespace VelocityDbSchema.NUnit
{
  public class Dokument : OptimizedPersistable
  {
    private string m_Name;

    public Dokument()
    {
    }

    public bool IsDataInUpdateTransaction
    {
      get
      {
        bool res = false;

        if (this.Session != null)
          res = this.Session.InUpdateTransaction;

        return res;
      }
    }
    public bool IsDataInTransaction
    {
      get
      {
        bool res = false;

        if (this.Session != null)
          res = this.Session.InTransaction;

        return res;
      }
    }

    public string Name
    {
      get
      {
        return m_Name;
      }
      set
      {
        UpdateField();
        m_Name = value;
      }
    }


    public bool UpdateField()
    {

      if (IsDataInUpdateTransaction)
        return Update();
      else
        return false;
    }

    public void Dispose()
    {
      try
      {
        GC.SuppressFinalize(this);
      }
      catch (Exception e)
      {
      }
    }


    public virtual void Init(object obj)
    {

    }
  }
}
