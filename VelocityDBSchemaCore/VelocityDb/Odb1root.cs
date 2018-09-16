using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using VelocityDb;
using VelocityDb.Collection;
using VelocityDb.Collection.BTree;
using VelocityDb.Session;

namespace VelocityDbSchema.VelocityDb
{
  public class VelocityDbroot : OptimizedPersistable
  {
    public const UInt32 PlaceInDatabase = 11;
    public UInt32 lastLicenseNumber;
    public UInt32 lastCustomerIdNumber;
    public string rsa2048privateKey;
    public string rsa2048publicKey;
    public BTreeSet<License> licenseSet;
    public BTreeSet<LicenseRequest> licenseRequestSet;
    public BTreeSet<License> licenseUnpaidSet;
    public BTreeSet<LicenseRequest> licenseUnpaidRequestSet;
    BTreeSet<LicensePayment> licensePaymentSet;
    public BTreeSet<CustomerContact> customersByEmail;
    public BTreeSet<CustomerContact> customersByUserName;

    public VelocityDbroot(SessionBase session, ushort maxEntriesPerNode, string keysDirectory)
    { 
      CompareCustomerEmail compareCustomerEmail = new CompareCustomerEmail();
      CompareCustomerUserName compareCustomerUserName = new CompareCustomerUserName();
      licenseSet = new BTreeSet<License>(null, session, 1000);
      licenseUnpaidSet = new BTreeSet<License>(null, session, 1000);
      licensePaymentSet = new BTreeSet<LicensePayment>(null, session, 1000);
      licenseRequestSet = new BTreeSet<LicenseRequest>(null, session, 1000);
      licenseUnpaidRequestSet = new BTreeSet<LicenseRequest>(null, session, 1000);
      customersByEmail = new BTreeSet<CustomerContact>(compareCustomerEmail, session, maxEntriesPerNode);
      customersByUserName = new BTreeSet<CustomerContact>(compareCustomerUserName, session, maxEntriesPerNode);
      lastLicenseNumber = 0;
      lastCustomerIdNumber = 0;     
      if (File.Exists(keysDirectory + @"\privateKey.txt") && File.Exists(keysDirectory + @"\publicKey.txt"))
      {
        using (StreamReader file = new StreamReader(keysDirectory + @"\privateKey.txt"))
        {
          rsa2048privateKey = file.ReadToEnd();     
        }
        using (StreamReader file = new StreamReader(keysDirectory + @"\publicKey.txt"))
        {
          rsa2048publicKey = file.ReadToEnd();     
        }
      }
      else
      {
        RSACryptoServiceProvider RSA = new RSACryptoServiceProvider(2048);
        rsa2048publicKey = RSA.ToXmlString(false);
        rsa2048privateKey = RSA.ToXmlString(true);
        using (StreamWriter outfile = new StreamWriter(keysDirectory + @"\publicKey.txt"))
        {
          outfile.Write(rsa2048publicKey);
        }
        using (StreamWriter outfile = new StreamWriter(keysDirectory + @"\privateKey.txt"))
        {
          outfile.Write(rsa2048privateKey);
        }
      }
    }

    public UInt32 NewLicenseNumber()
    {
      Update();
      return ++lastLicenseNumber;
    }    
    
    public UInt32 NewCustomerNumber()
    {
      Update();
      return ++lastCustomerIdNumber;
    }

    public BTreeSet<LicensePayment> LicensePaymentSet
    {
      get
      {
        return licensePaymentSet;
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
