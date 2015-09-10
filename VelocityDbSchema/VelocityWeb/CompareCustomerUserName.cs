using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb.Session;
using VelocityDb.Collection.Comparer;

namespace VelocityDbSchema.VelocityWeb
{
  class CompareCustomerUserName : VelocityDbComparer<CustomerContact>
  {
    public CompareCustomerUserName() { }
    public override int Compare(CustomerContact aCustomer, CustomerContact bCustomer)
    {
      return aCustomer.userName.CompareTo(bCustomer.userName);
    }
  }
}
