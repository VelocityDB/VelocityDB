using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Collection.BTree;

namespace VelocityDbSchema.Samples.Forex
{
  public class ForexBroker : OptimizedPersistable, IComparable<ForexBroker>
  {
    public enum BrokerName : ulong { Dukascopy, LMax, Alpari }; // fill in all possible (max 64 is OK?)

    BrokerName name;
    string city;
    string country;
    string state;
    string streetAddress1;
    string streetAddress2;
    string postalCode;
    string fullName;
    string email;
    string techEmail;
    string normalPhone;
    string accountingPhone;
    string emergencyPhone;
    string contactNameSales;
    string contactNameAccounts;
    string contactNameTech;
    string notes;

    public ForexBroker(BrokerName name, 
      string city,
      string country,
      string state,
      string streetAddress1,
      string streetAddress2,
      string postalCode,
      string fullName, 
      string email, 
      string techEmail, 
      string normalPhone, 
      string accountingPhone, 
      string emergencyPhone, 
      string contactNameSales, 
      string contactNameAccounts, 
      string contactNameTech,
      string notes = "")
    {
      this.name = name;
      this.city = city;
      this.country = country;
      this.state = state;
      this.streetAddress1 = streetAddress1;
      this.streetAddress2 = streetAddress2;
      this.postalCode = postalCode;
      this.fullName = fullName;
      this.email = email;
      this.techEmail = techEmail;
      this.normalPhone = normalPhone;
      this.accountingPhone = accountingPhone;
      this.emergencyPhone = emergencyPhone;
      this.contactNameSales = contactNameSales;
      this.contactNameAccounts = contactNameAccounts;
      this.contactNameTech = contactNameTech;
      this.notes = notes;
    }
    public int CompareTo(ForexBroker broker2)
    {
      return Name.CompareTo(broker2.Name);
    }

    public BrokerName Name
    {
      get
      {
        return name;
      }
    }

    public string FullName
    {
      get
      {
        return fullName;
      }
    }

    public string Email
    {
      get
      {
        return email;
      }
    }

    public string TechEmail
    {
      get
      {
        return techEmail;
      }
    }
  }
}
