using AutoClaimCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoClaimUsingSQL
{
  public class AutoClaimSQL : IManageAutoClaim<MitchellClaimType>
  {
    public void DeleteClaim(string claimNumber)
    {
      using (AutoClaimEntityFrameworkContext dbContext = new AutoClaimEntityFrameworkContext())
      {
        MitchellClaimType claim = (from c in dbContext.Claims.Include("LossInfo").Include("Vehicles") where c.ClaimNumber == claimNumber select c).FirstOrDefault();
        if (claim == null)
          Console.WriteLine("Invalid claim update, claim with claim number: " + claimNumber + " does not exist in database");
        dbContext.Claims.Remove(claim);
        dbContext.SaveChanges();
      }
    }

    public IList<MitchellClaimType> FindClaims(DateTime lossDateFrom, DateTime lossDateTo)
    {
      using (AutoClaimEntityFrameworkContext dbContext = new AutoClaimEntityFrameworkContext())
      {
        return (from c in dbContext.Claims where c.LossDate >= lossDateFrom && c.LossDate <= lossDateTo select c).ToList();
      }
    }

    public void NewClaim(MitchellClaimType claim)
    {
      using (AutoClaimEntityFrameworkContext dbContext = new AutoClaimEntityFrameworkContext())
      {
        dbContext.Claims.Add(claim);
        dbContext.SaveChanges();
      }
    }

    public MitchellClaimType ReadClaim(string claimNumber)
    {
      using (AutoClaimEntityFrameworkContext dbContext = new AutoClaimEntityFrameworkContext())
      {
        MitchellClaimType claim = (from c in dbContext.Claims.Include("LossInfo").Include("Vehicles") where c.ClaimNumber == claimNumber select c).FirstOrDefault();
        if (claim == null)
          Console.WriteLine("Invalid claim read, claim with claim number: " + claimNumber + " does not exist in database");
        return claim;
      }
    }

    public void UpdateClaim(MitchellClaimType claimUpdates)
    {
      // TO DO
    }
  }
}
