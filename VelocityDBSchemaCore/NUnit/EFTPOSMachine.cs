using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NodaTime;
using VelocityDb;

namespace VelocityDbSchema.NUnit
{
  public class EFTPOSMachine : CommonBaseClass, ITransactionFacilitator
  {
    public DateTimeZone MyTimeZone => DateTimeZoneProviders.Tzdb["Australia/Sydney"];

    //   public override string BasicDescription() => $"{MyTimeZone}";
  }

  public class EFTPOSMachineParent : OptimizedPersistable
  {
    public EFTPOSMachineParent()
    {
      _itransactionFacilitator = new EFTPOSMachine();
    }
    ITransactionFacilitator _itransactionFacilitator;

    public ITransactionFacilitator TransactionFacilitator
    {
      get
      {
        return _itransactionFacilitator;
      }
    }
  }

  public interface ITransactionFacilitator
  {
    DateTimeZone MyTimeZone { get; }
  }
  public abstract class CommonBaseClass : OptimizedPersistable
  {
    public override bool AllowOtherTypesOnSamePage
    {
      get
      {
        return false;
      }
    }

  }
}
