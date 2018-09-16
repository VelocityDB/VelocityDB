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
  /// <summary>
  /// Contains a list of possible license Propertys for licensing VelocityDb class library and server
  /// </summary>
  [Serializable]
  public class LicenseRequest : OptimizedPersistable
  {
    /// <summary>
    /// Hint about a Database location for this type of object.
    /// </summary>
    public const UInt32 PlaceInDatabase = 12;
    string hostName;
    string domainName;
    string userName;
#pragma warning disable 0169
    string computerId; // reserved for future possible use (ethernet address)
#pragma warning restore 0169
    Int32 versionMajor;
    Int32 versionMinor;
    Int32 processorCount;
    UInt32 maxNumberOfClients;
#pragma warning disable 0414
    UInt32 maxNumberOfServers;
#pragma warning restore 0414 
    DateTime dateTimeCreated;
    DateTime expireDate;
    UInt32 price;
    License license;
    /// <summary>
    /// Creates a new unrestricted license
    /// </summary>
    public LicenseRequest()
    {
      hostName = "";
      domainName = "";
      userName = "";
      maxNumberOfClients = 1;
      maxNumberOfServers = 0;
      dateTimeCreated = DateTime.Now;
      expireDate = dateTimeCreated + new TimeSpan(365, 0, 0, 0);
      versionMajor = 1;
      versionMinor = 9;
      processorCount = 1;
      SetPrice();
    }

    /// <summary>
    /// Creates a new restricted license
    /// </summary>
    /// <param storeName="forHostName">Usage valid on this host only</param>
    /// <param storeName="forMaxNumberOfClients">Maximum number of server clients for which license is valid</param>
    /// <param storeName="validForHowManyDays">Restricts usgage to this number of days from now</param>
    public LicenseRequest(string forHostName, UInt32 forMaxNumberOfClients, int validForHowManyDays)
    {
      hostName = forHostName;
      maxNumberOfClients = forMaxNumberOfClients;
      dateTimeCreated = DateTime.Now;
      versionMajor = 1;
      versionMinor = 9;
      expireDate = dateTimeCreated + new TimeSpan(validForHowManyDays, 0, 0, 0);
      SetPrice();
    }
    /// <summary>
    /// Creates a new restricted license
    /// </summary>
    /// <param storeName="forHostName">Usage valid on this host only</param>
    /// <param storeName="forDomainName">Usage valid on this domain only</param>
    /// <param storeName="forMaxNumberOfClients">Maximum number of server clients for which license is valid</param>
    /// <param storeName="expireDate">License expiration time</param>
    public LicenseRequest(Int32 processorCount, string forUserName, string forHostName, string forDomainName, UInt32 forMaxNumberOfClients, DateTime expireDate)
    {
      userName = forUserName;
      hostName = forHostName;
      domainName = forDomainName;
      maxNumberOfClients = forMaxNumberOfClients;
      dateTimeCreated = DateTime.Now;
      this.expireDate = expireDate;
      versionMajor = 99;
      versionMinor = 9;
      this.processorCount = processorCount;
      SetPrice();
    }

    public void SetPrice()
    {
      Update();
      TimeSpan ts = expireDate - dateTimeCreated;
      int days = Math.Min(ts.Days, 1000);
      double calculatePrice;
      days -= 10;
      if (days <= 0)
        price = 0;
      else
      {
        if (maxNumberOfClients <= 0)
          maxNumberOfClients = 1;
        bool userNameSet = userName != null && userName.Length > 0;
        bool hostNameSet = hostName != null && hostName.Length > 0;
        bool domainNameSet = domainName != null && domainName.Length > 0;
        if (hostNameSet && domainNameSet && userNameSet)
          calculatePrice = 0.3 * days + maxNumberOfClients * days * 0.3;
        else if (userNameSet && hostNameSet)
          calculatePrice = 0.5 * days + maxNumberOfClients * days * 0.5;
        else if (userNameSet && domainNameSet)
          calculatePrice = 0.7 * days + maxNumberOfClients * days * 0.7;
        else if (hostNameSet && domainNameSet)
          calculatePrice = 0.8 * days + maxNumberOfClients * days * 0.8;
        else if (userNameSet)
          calculatePrice = 1.0 * days + maxNumberOfClients * days * 1.0;
        else if (hostNameSet)
          calculatePrice = 1.2 * days + maxNumberOfClients * days * 1.2;
        else if (domainNameSet)
          calculatePrice = 1.4 * days + maxNumberOfClients * days * 1.4;
        else
          calculatePrice = 4 * days + maxNumberOfClients * days * 3.0;
        price = (uint)Math.Round((calculatePrice + processorCount * days * 0.1));
      }
      
    }

    /// <summary>
    /// Gets the restricted domain storeName
    /// </summary>
    public string DomainName
    {
      get
      {
        return domainName;
      }
      set
      {
        Update();
        domainName = value;
      }
    }
    /// <summary>
    /// Gets the restricted host storeName
    /// </summary>
    public string HostName
    {
      get
      {
        return hostName;
      }
      set
      {
        Update();
        hostName = value;
      }
    }
    /// <summary>
    /// Gets the time of license creation.
    /// </summary>
    public DateTime DateTimeCreated
    {
      get
      {
        return dateTimeCreated;
      }
    }
    /// <summary>
    /// Gets expire time.
    /// </summary>
    public DateTime ExpireDate
    {
      get
      {
        return expireDate;
      }
      set
      {
        Update();
        expireDate = value;
      }
    }

    public License License
    {
      get
      {
        return license;
      }
      set
      {
        Update();
        license = value;
      }
    }

    /// <summary>
    /// Gets the maximum number of Server clients permitted by this license
    /// </summary>
    public UInt32 MaxNumberOfClients
    {
      get
      {
        return maxNumberOfClients;
      }
      set
      {
        Update();
        maxNumberOfClients = value;
      }
    }

    /// <summary>
    /// Gets/sets the price of the requested license
    /// </summary>
    public UInt32 Price
    {
      get
      {
        return price;
      }
      set
      {
        Update();
        price = value;
      }
    }

    public Int32 ProcessorCount
    {
      get
      {
        return processorCount;
      }
      set
      {
        Update();
        processorCount = value;
      }
    }

    /// <summary>
    /// Gets the user storeName restriction
    /// </summary>
    public string UserName
    {
      get
      {
        return userName;
      }
      set
      {
        Update();
        userName = value;
      }
    }
    /// <summary>
    /// Gets the preffered Database number for licenses
    /// </summary>
    public override UInt32 PlacementDatabaseNumber
    {
      get
      {
        return PlaceInDatabase;
      }
    }

    public int MajorVersion
    {
      get
      {
        return versionMajor;
      }
      set
      {
        Update();
        versionMajor = value;
      }
    }

    public int MinorVersion
    {
      get
      {
        return versionMinor;
      }
      set
      {
        Update();
        versionMinor = value;
      }
    }

    public override string ToString()
    {
      return Oid.ToString();
    }
  }
}
