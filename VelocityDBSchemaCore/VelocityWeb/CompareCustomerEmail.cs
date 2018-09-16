using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb.Session;
using VelocityDb.Collection.Comparer;

namespace VelocityDbSchema.VelocityWeb
{
  class CompareCustomerEmail : VelocityDbComparer<CustomerContact>
  {
    public CompareCustomerEmail() { }
    public override int Compare(CustomerContact aCustomer, CustomerContact bCustomer)
    {
      return aCustomer.email.CompareTo(bCustomer.email);
    }
  }
}
