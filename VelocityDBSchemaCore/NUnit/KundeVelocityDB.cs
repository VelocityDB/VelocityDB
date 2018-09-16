using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;

namespace VelocityDbSchema.NUnit
{
  public class KundeVelocityDB : OptimizedPersistable
  {
    private string _kto;
    private string _ktoFoerderer;
    private int _periode;

    public string Kto
    {
      get { return _kto; }
      set
      {
        Update();
        _kto = value;
      }
    }
    public string KtoFoerderer
    {
      get { return _ktoFoerderer; }
      set
      {
        Update();
        _ktoFoerderer = value;
      }
    }

    public int Periode
    {
      get { return _periode; }
      set
      {
        Update();
        _periode = value;
      }
    }
  }
}
