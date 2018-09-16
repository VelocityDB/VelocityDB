using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Session;

namespace VelocityDbSchema.VelocityDb
{
  public class LicensePayment : OptimizedPersistable 
  {
    UInt32 dollars;
    DateTime dateTimeCreated;
    License[] licenses;
    LicenseRequest[] licenseRequests;

    public LicensePayment(UInt32 dollars)
    {
      this.dollars = dollars;
      dateTimeCreated = DateTime.Now;
    }

    public LicensePayment(UInt32 dollars, DateTime dateTimeCreated)
    {
      this.dollars = dollars;
      this.dateTimeCreated = dateTimeCreated;
    }

    public void AddLicense(License license)
    {
      Update();
      if (licenses == null)
      {
        licenses = new License[1];
        licenses[0] = license;
      }
      else
      {
        Array.Resize<License>(ref licenses, licenses.Length + 1);
        licenses[licenses.Length - 1] = license;
      }
    }

    public void AddLicenseRequest(LicenseRequest licenseRequest)
    {
      Update();
      if (licenseRequests == null)
      {
        licenseRequests = new LicenseRequest[1];
        licenseRequests[0] = licenseRequest;
      }
      else
      {
        Array.Resize<LicenseRequest>(ref licenseRequests, licenseRequests.Length + 1);
        licenseRequests[licenseRequests.Length - 1] = licenseRequest;
      }
    }

    public UInt32 Dollars
    {
      get
      {
        return dollars;
      }
      set
      {
        Update();
        dollars = value;
      }
    }

    public DateTime DateTimeCreated
    {
      get
      {
        return dateTimeCreated;
      }
    }

    public LicenseRequest[] LicenseRequests
    {
      get
      {
        return licenseRequests;
      }
      set
      {
        Update();
        licenseRequests = value;
      }
    }

    public License[] Licenses
    {
      get
      {
        return licenses;
      }
      set
      {
        Update();
        licenses = value;
      }
    }
  }
}
