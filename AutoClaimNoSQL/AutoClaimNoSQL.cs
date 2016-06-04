using AutoClaimCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VelocityDb.Session;

namespace AutoClaimUsingNoSQL
{
  public class AutoClaimNoSQL : IManageAutoClaim<MitchellClaimType>
  {
    static readonly string s_systemDir = "MitchellClaims"; // appended to SessionBase.BaseDatabasePath

    public void DeleteClaim(string claimNumber)
    {
      using (SessionBase session = new SessionNoServer(s_systemDir))
      {
        session.BeginUpdate();
        var claim = ReadClaim(claimNumber, session);
        claim?.Unpersist(session);
        session.Commit();
      }
    }

    public void NewClaim(MitchellClaimType claim)
    {
      using (SessionBase session = new SessionNoServer(s_systemDir))
      {
        session.BeginUpdate();
        session.Persist(claim);
        session.Commit();
      }
    }

    MitchellClaimType ReadClaim(string claimNumber, SessionBase session)
    {
      MitchellClaimType claim = (from c in session.AllObjects<MitchellClaimType>() where c.ClaimNumber == claimNumber select c).FirstOrDefault();
      if (claim == null)
        Console.WriteLine("Invalid claim read, claim with claim number: " + claimNumber + " does not exist in database");
      return claim;
    }

    public MitchellClaimType ReadClaim(string claimNumber)
    {
      using (SessionBase session = new SessionNoServer(s_systemDir))
      {
        session.BeginRead();
        var claim = ReadClaim(claimNumber, session);
        session.Commit();
        return claim;
      }    
    }

    public IList<MitchellClaimType> FindClaims(DateTime lossDateFrom, DateTime lossDateTo)
    {
      using (SessionBase session = new SessionNoServer(s_systemDir))
      {
        session.BeginRead();
        var e = (from c in session.AllObjects<MitchellClaimType>() where c.LossDate >= lossDateFrom && c.LossDate <= lossDateTo select c).ToList();
        session.Commit();
        return e;
      }
    }

    public void UpdateClaim(MitchellClaimType claimUpdates)
    {
      using (SessionBase session = new SessionNoServer(s_systemDir))
      {
        session.BeginUpdate();
        MitchellClaimType claim = ReadClaim(claimUpdates.ClaimNumber, session);
        UpdateValues(claim, claimUpdates, session);
        session.Commit();
      }
    }

    static void UpdateValues(object target, object source, SessionBase session)
    {
      Type t = target.GetType();
      var properties = t.GetProperties().Where(prop => prop.CanRead && prop.CanWrite);
      foreach (var prop in properties)
      {
        var newValue = prop.GetValue(source, null);
        var oldValue = prop.GetValue(target, null);
        bool skipProp = false;
        foreach (var a in prop.CustomAttributes)
          if (a.AttributeType == typeof(System.Xml.Serialization.XmlIgnoreAttribute))
          {
            skipProp = true;
            break;
          }
        if (newValue != null && !skipProp)
        {
          Type propType = newValue.GetType();
          TypeCode typeCode = Type.GetTypeCode(propType);
          if (typeCode == TypeCode.Object && oldValue != null)
          {
            if (propType.IsArray && propType.GetElementType().IsValueType == false) // array is not a good choice but that is what xsd.exe generated so try to work with it.
            {
              session.UpdateObject(target);
              Array newArray = newValue as Array;
              Array oldArray = oldValue as Array;
              int i = 0;
              if (newArray.Length > oldArray.Length)
              {
                Array array = (Array)Activator.CreateInstance(propType, newArray.Length);
                i = 0;
                foreach (var o in oldArray)
                  array.SetValue(o, i++);
                oldArray = array;
              }
              i = 0;
              foreach (object obj in newArray)
              {
                object oldObj = oldArray.GetValue(i); // assume same object is same element in both new and old
                if (oldObj == null)
                  oldArray.SetValue(obj, i);
                else
                  UpdateValues(oldObj, obj, session);
                i++;
              }
              prop.SetValue(target, oldValue, null);
            }
            else
            {
              session.UpdateObject(oldValue);
              UpdateValues(oldValue, newValue, session);
            }
          }
          else if (newValue.Equals(oldValue) == false)
          {
            session.UpdateObject(target);
            prop.SetValue(target, newValue, null);
          }
        }
      }
    }
  }
}
