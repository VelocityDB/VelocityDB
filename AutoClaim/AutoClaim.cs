using AutoClaimUsingNoSQL;
using AutoClaimUsingSQL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AutoClaim
{
  class AutoClaim
  {
    static void Main(string[] args)
    {
      bool useSqlServer = args.Length > 0;
      if (useSqlServer)
        ProcessClaimsUsingEntityFramework();
      else
        ProcessClaimsUsingNoSQL();
    }
    static void ProcessClaimsUsingEntityFramework()
    {
      try
      {
        object dataDir = AppDomain.CurrentDomain.GetData("DataDirectory");
        if (dataDir == null)
          AppDomain.CurrentDomain.SetData("DataDirectory", Directory.GetCurrentDirectory());
        AutoClaimSQL sql = new AutoClaimSQL();
        XmlSerializer xmlSerializer = new XmlSerializer(typeof(MitchellClaimType));
        MitchellClaimType claim = new MitchellClaimType();
        string[] files = Directory.GetFiles("../../NewClaims", "*.xml");
        foreach (string fileName in files)
        {
          using (StreamReader reader = new StreamReader(fileName))
          {
            claim = (MitchellClaimType)xmlSerializer.Deserialize(reader);
            sql.NewClaim(claim);
          }
        }
        claim = sql.ReadClaim(claim.ClaimNumber);
        files = Directory.GetFiles("../../ClaimUpdates", "*.xml");
        foreach (string fileName in files)
        {
          using (StreamReader reader = new StreamReader(fileName))
          {
            MitchellClaimType claimUpdates = (MitchellClaimType)xmlSerializer.Deserialize(reader);
            sql.UpdateClaim(claimUpdates);
          }
        }
        claim = sql.ReadClaim(claim.ClaimNumber);

        foreach (var c in sql.FindClaims(claim.LossDate, DateTime.UtcNow))
          Console.WriteLine(c);
        foreach (var c in sql.FindClaims(claim.LossDate + TimeSpan.FromDays(1), DateTime.UtcNow))
          Console.WriteLine(c); // should not get here
        sql.DeleteClaim(claim.ClaimNumber);
        claim = sql.ReadClaim(claim.ClaimNumber);
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
      }
    }

    static void ProcessClaimsUsingNoSQL()
    {
      try
      {
        AutoClaimNoSQL noSql = new AutoClaimNoSQL();
        XmlSerializer xmlSerializer = new XmlSerializer(typeof(AutoClaimUsingNoSQL.MitchellClaimType));
        AutoClaimUsingNoSQL.MitchellClaimType claim = new AutoClaimUsingNoSQL.MitchellClaimType();
        using (MemoryStream xmlStream = new MemoryStream())
        {
          xmlSerializer.Serialize(xmlStream, claim); // sanity check, make sure serializtion works
        }
        string[] files = Directory.GetFiles("../../NewClaims", "*.xml");
        foreach (string fileName in files)
        {
          claim = (AutoClaimUsingNoSQL.MitchellClaimType)xmlSerializer.Deserialize(new StreamReader(fileName));
          noSql.NewClaim(claim);
        }
        claim = noSql.ReadClaim(claim.ClaimNumber);
        files = Directory.GetFiles("../../ClaimUpdates", "*.xml");
        foreach (string fileName in files)
        {
          AutoClaimUsingNoSQL.MitchellClaimType claimUpdates = (AutoClaimUsingNoSQL.MitchellClaimType)xmlSerializer.Deserialize(new StreamReader(fileName));
          noSql.UpdateClaim(claimUpdates);
        }
        claim = noSql.ReadClaim(claim.ClaimNumber);
        foreach (var c in noSql.FindClaims(claim.LossDate.Value, DateTime.UtcNow))
          Console.WriteLine(c);
        foreach (var c in noSql.FindClaims(claim.LossDate.Value + TimeSpan.FromDays(1), DateTime.UtcNow))
          Console.WriteLine(c); // should not get here
        noSql.DeleteClaim(claim.ClaimNumber);
        claim = noSql.ReadClaim(claim.ClaimNumber);
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
      }
    }
  }
}
