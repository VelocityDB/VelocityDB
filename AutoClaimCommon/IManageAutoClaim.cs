using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoClaimCommon
{
  public interface IManageAutoClaim<T>
  {
    void DeleteClaim(string claimNumber);
    IList<T> FindClaims(DateTime lossDateFrom, DateTime lossDateTo);
    void NewClaim(T claim);
    T ReadClaim(string claimNumber);
    void UpdateClaim(T claimUpdates);
  }
}
