using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Session;
using VelocityDb.Collection.Comparer;

namespace VelocityDbSchema.VelocityWeb
{
  public class CompareLicenseHostName : VelocityDbComparer<License>
  {
    public CompareLicenseHostName() { }
    public override int Compare(License aLicense, License bLicense)
    {
      int compareValue = aLicense.HostName.CompareTo(bLicense.HostName);
      if (compareValue == 0)
        return base.Compare(aLicense, bLicense);
      return compareValue;
    }
  }
}
