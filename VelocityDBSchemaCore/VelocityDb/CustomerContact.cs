using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Session;
using VelocityDb.Collection;
using VelocityDb.Collection.Comparer;
using VelocityDb.Collection.BTree;

namespace VelocityDbSchema.VelocityDb
{
  public class CustomerContact : OptimizedPersistable 
  {
    public UInt32 idNumber;
    public string company;
    public string firstName;
    public string lastName;
    public string email;
    public string address;
    public string addressLine2;
    public string city;
    public string zipCode;
    public string state;
    public string country;
    public string countryCode;
    public string phone;
    public string fax;
    public string mobile;
    public string skypeName;
    public string webSite;
    public string userName;
    public string password;
    public HowFound howFoundVelocityDb;
    public string howFoundOther;
    //public BTreeSet<VisitEvent> visitEvents;
    public BTreeSet<License> licenseSet;
    public BTreeSet<LicenseRequest> licenseRequestSet;
    public BTreeSet<License> licenseUnpaidSet;
    public BTreeSet<LicenseRequest> licenseUnpaidRequestSet;
    public BTreeSet<LicensePayment> licensePaymentSet;
    public SortedSetAny<string> priorVerifiedEmailSet;
    public enum HowFound : byte { Unknown, Google, Friend, Bing, Yahoo, Wiki, Email, VisualStudioGallery };

    public CustomerContact(string email, string userName)
    {
      this.email = email;
      this.userName = userName;
    }

    public CustomerContact(string company, string firstName, string lastName, string email, string address,
                           string addressLine2, string city, string zipCode, string state, string country, string countryCode, string phone, string fax,
                           string mobile, string skypeName, string webSite, string userName, string password, string howFoundOther, int howFoundChoice,
                           SessionBase session)
    {
      this.company = company;
      this.firstName = firstName;
      this.lastName = lastName;
      this.email = email;
      this.address = address;
      this.addressLine2 = addressLine2;
      this.city = city;
      this.zipCode = zipCode;
      this.state = state;
      this.country = country;
      this.countryCode = countryCode;
      this.phone = phone;
      this.fax = fax;
      this.mobile = mobile;
      this.skypeName = skypeName;
      this.webSite = webSite;
      this.userName = userName;
      this.password = password;
      this.howFoundOther = howFoundOther;
      this.howFoundVelocityDb = (HowFound) howFoundChoice;
      priorVerifiedEmailSet = new SortedSetAny<string>();
      licenseSet = new BTreeSet<License>(null, session);
      licenseRequestSet = new BTreeSet<LicenseRequest>(null, session);
      licenseUnpaidSet = new BTreeSet<License>(null, session);
      licenseUnpaidRequestSet = new BTreeSet<LicenseRequest>(null, session);
      licensePaymentSet = new BTreeSet<LicensePayment>(null, session);
      CompareByField<VisitEvent> comparator = new CompareByField<VisitEvent>("eventTime", session);
      //visitEvents = new BTreeSet<VisitEvent>(comparator, session, 10000, sizeof(long), true);
    }

    public override int CompareTo(object obj)
    {
      if (obj is CustomerContact)
      {
        CustomerContact otherCustomerContact = (CustomerContact)obj;
        return this.email.CompareTo(otherCustomerContact.email);
      }
      else
      {
        throw new ArgumentException("object is not a CustomerContact");
      }
    }

    public string Address
    {
      get
      {
        return address;
      }
    } 

    public string Address2
    {
      get
      {
        return addressLine2;
      }
    }

    public string City
    {
      get
      {
        return city;
      }
    } 

    public string Company
    {
      get
      {
        return company;
      }
    } 

    public string Country
    {
      get
      {
        return country;
      }
    } 

    public string Email
    {
      get
      {
        return email;
      }
    }    

    public string Fax
    {
      get
      {
        return fax;
      }
    } 
    
    public string FirstName
    {
      get
      {
        return firstName;
      }
    }

    public HowFound HowFoundUs
    {
      get
      {
        return howFoundVelocityDb;
      }
    }        
    
    public string HowFoundUsOther
    {
      get
      {
        return howFoundOther;
      }
    }   

    public string LastName
    {
      get
      {
        return lastName;
      }
    }    
    
    public string Mobile
    {
      get
      {
        return mobile;
      }
    }

    public string Password
    {
      get
      {
        return password;
      }
    }    
    
    public string Phone
    {
      get
      {
        return phone;
      }
    }

    public string Skype
    {
      get
      {
        return skypeName;
      }
    }       
    
    public string State
    {
      get
      {
        return state;
      }
    } 
 
    public string UserName
    {
      get
      {
        return userName;
      }
    }

    public string WebSite
    {
      get
      {
        return webSite;
      }
    }

    public string ZipCode
    {
      get
      {
        return zipCode;
      }
    }

    public BTreeSet<License> LicenseSet
    {
      get
      {
        return licenseSet;
      }
    }

    public BTreeSet<License> LicenseUnpaidSet
    {
      get
      {
        return licenseUnpaidSet;
      }
    }

    public BTreeSet<LicensePayment> LicensePaymentSet
    {
      get
      {
        return licensePaymentSet;
      }
    }

    public BTreeSet<LicenseRequest> LicenseRequestSet
    {
      get
      {
        return licenseRequestSet;
      }
    }

    public override UInt32 PlacementDatabaseNumber
    {
      get
      {
        return 12;
      }
    }

    public override string ToString()
    {
      return UserName + " " + Oid.ToString();
    }

    public override UInt64 Persist(Placement place, SessionBase session, bool persistRefs = true, bool disableFlush = false, Queue<IOptimizedPersistable> toPersist = null)
    {
      base.Persist(place, session, false, disableFlush, toPersist);
      priorVerifiedEmailSet.Persist(place, session, true, disableFlush, toPersist);
      licenseRequestSet.Persist(place, session, true, disableFlush, toPersist);
      licenseSet.Persist(place, session, true, disableFlush, toPersist);
      licenseUnpaidSet.Persist(place, session, true, disableFlush, toPersist);
      licenseUnpaidRequestSet.Persist(place, session, true, disableFlush, toPersist);
      licensePaymentSet.Persist(place, session, true, disableFlush, toPersist);
      licensePaymentSet.Persist(place, session, true, disableFlush, toPersist);
      return Id;
    }

    public override void Unpersist(SessionBase session)
    {
      if (IsPersistent == false)
        return;
      foreach (LicenseRequest r in LicenseRequestSet)
        r.Unpersist(session);
      LicenseRequestSet.Unpersist(session);
      foreach (LicensePayment r in LicensePaymentSet)
        r.Unpersist(session);
      LicensePaymentSet.Unpersist(session);
      //foreach (VisitEvent r in visitEvents)
      //  r.Unpersist(session, disableFlush);
      //visitEvents.Unpersist(session, disableFlush);
      LicenseUnpaidSet.Unpersist(session);
      LicenseSet.Unpersist(session);
      base.Unpersist(session);
    }
  }
}


