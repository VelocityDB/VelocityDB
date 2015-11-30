using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using VelocityDb;
using VelocityDb.Collection;
using VelocityDb.Collection.BTree;
using VelocityDb.Session;

namespace VelocityDbSchema.VelocityWeb
{
  public class Root : OptimizedPersistable
  {
    public UInt32 lastLicenseNumber;
    public UInt32 lastCustomerIdNumber;
    public BTreeSet<CustomerContact> customersByEmail;
    public BTreeSet<CustomerContact> customersByUserName;

    public Root(SessionBase session, ushort maxEntriesPerNode)
    { 
      CompareCustomerEmail compareCustomerEmail = new CompareCustomerEmail();
      CompareCustomerUserName compareCustomerUserName = new CompareCustomerUserName();
      customersByEmail = new BTreeSet<CustomerContact>(compareCustomerEmail, session, maxEntriesPerNode);
      customersByUserName = new BTreeSet<CustomerContact>(compareCustomerUserName, session, maxEntriesPerNode);
      lastCustomerIdNumber = 0;     
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
  }
}
