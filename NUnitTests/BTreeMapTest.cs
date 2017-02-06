using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VelocityDb;
using VelocityDb.Collection.BTree;
using VelocityDb.Session;

namespace NUnitTests
{
  [TestFixture]
  public class BTreeMapTest
  {
    UInt64 _id;
      const int numberOfZipCodes = 10000;
    [Test]
    public void BTreeMap1Populate()
    {
      int sessionId = 0;
      SessionBase session = null;
      try
      {
        session = SessionManager.SessionPool.GetSession(out sessionId);
        session.BeginUpdate();
        var bTreeMap = new BTreeMapOwner();
        _id = session.Persist(bTreeMap);
        for (int i = 0; i < numberOfZipCodes; i++)
        {
          string str = i.ToString();
          var bTree = new BTreeSet<LocationWithinUSA>();
          for (int j = 0; j < Math.Min(i, 1000); j++)
          {
            var loc = new LocationWithinUSA();
            session.Persist(loc);
            bTree.AddFast(loc);
          }
          bTreeMap.LocationByZipCode.AddFast(str, bTree);
        }
        session.Commit();
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
        throw;
      }
      finally
      {
        SessionManager.SessionPool.FreeSession(sessionId, session);
      }
    }

    [Test]
    public void BTreeMap2Iterate()
    {
      int sessionId = 0;
      SessionBase session = null;
      try
      {
        session = SessionManager.SessionPool.GetSession(out sessionId);
        session.BeginRead();
        var bTreeMap = session.Open<BTreeMapOwner>(_id);
        int zipCodeCt = 0;
        foreach (var p in bTreeMap.LocationByZipCode)
        {
          session.Commit();
          session.BeginUpdate();
          var v = p.Value;
          foreach (var l in v)
            l.Address1 = "2034 Cordoba PL";
          var k = p.Key;
          zipCodeCt++;
          session.Commit();
          session.BeginRead();
        }
        Assert.AreEqual(numberOfZipCodes, zipCodeCt);
        session.Commit();
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
        throw;
      }
      finally
      {
        SessionManager.SessionPool.FreeSession(sessionId, session);
      }
    }

    public static string RandomString(int length)
    {
      Random random = new Random();
      const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
      return new string(Enumerable.Repeat(chars, length)
        .Select(s => s[random.Next(s.Length)]).ToArray());
    }
  }

    public static class SessionManager
    {
      static readonly string s_dataDir = "c:/NUnitTestDbs";
      static SessionPool s_pool = new SessionPool(1, () => new SessionNoServer(s_dataDir, 5000, true, true, CacheEnum.No));

      static SessionManager()
      {
        //SessionBase.SetMinMaxStringIntern(1, 1);
        SessionBase.BTreeAddFastTransientBatchSize = 2000;
        SessionBase.DefaultCompressPages = VelocityDb.PageInfo.compressionKind.LZ4;
      }

      public static SessionPool SessionPool
      {
        get
        {
          return s_pool;
        }
      }
    }

    public class BTreeMapOwner : OptimizedPersistable
    {
      BTreeMap<string, BTreeSet<LocationWithinUSA>> _locationByZipCode;
      public BTreeMapOwner()
      {
        _locationByZipCode = new BTreeMap<string, BTreeSet<LocationWithinUSA>>(null, null);
      }

      public BTreeMap<string, BTreeSet<LocationWithinUSA>> LocationByZipCode
      {
        get
        {
          return _locationByZipCode;
        }
      }
    }

    public abstract class Location : OptimizedPersistable
    {
      string _addrees1;
      string _addrees2;
      string _addrees3;
      string _city;
      string _email;
      string _faxNumber;
      string _phoneNumber;
      double _longitude;
      double _latitude;
      string _websiteUrl;

      public Location()
      {
        _longitude = double.NaN;
        _latitude = double.NaN;
      }
      enum FlagBitEnum : UInt32 { IsMainPracticeLocation = 1, IsHome = 2, IsHospital = 4, IsMailingAddress = 8 }

      public string Address1
      {
        get
        {
          return _addrees1;
        }
        set
        {
          Update();
          _addrees1 = value;
        }
      }

      public string Address2
      {
        get
        {
          return _addrees2;
        }
        set
        {
          Update();
          _addrees2 = value;
        }
      }

      public string Address3
      {
        get
        {
          return _addrees3;
        }
        set
        {
          Update();
          _addrees3 = value;
        }
      }

      public string City
      {
        get
        {
          return _city;
        }
        set
        {
          Update();
          _city = value;
        }
      }

      public string FaxNumber
      {
        get
        {
          return _faxNumber;
        }
        set
        {
          Update();
          _faxNumber = value;
        }
      }

      public double Latitude
      {
        get
        {
          if (_latitude != double.NaN)
            return _latitude;
          return double.NaN;
        }
        set
        {
          Update();
          _latitude = value;
        }
      }

      public double Longitude
      {
        get
        {
          if (_latitude != double.NaN)
            return _latitude;
          return double.NaN;
        }
        set
        {
          Update();
          _longitude = value;
        }
      }
      public string PhoneNumber
      {
        get
        {
          return _phoneNumber;
        }
        set
        {
          Update();
          _phoneNumber = value;
        }
      }
      public string WebsiteUrl
      {
        get
        {
          return _websiteUrl;
        }
        set
        {
          Update();
          _websiteUrl = value;
        }
      }
    }

    public class LocationWithinUSA : Location
    {
      string _state; // not only naming states within USA, also includes "PR", "San Juan", "AE" ...
      string _zip5FirstDigits;
      string _zipAddional4Digits;
      public LocationWithinUSA()
      {

      }

      public string State
      {
        get
        {
          return _state;
        }
        set
        {
          Update();
          _state = value;
        }
      }

      public string Zip5FirstDigits
      {
        get
        {
          return _zip5FirstDigits;
        }
        set
        {
          Update();
          _zip5FirstDigits = value;
        }
      }
      public string ZipAddional4Digits
      {
        get
        {
          return _zipAddional4Digits;
        }
        set
        {
          Update();
          _zipAddional4Digits = value;
        }
      }
    }
  }
