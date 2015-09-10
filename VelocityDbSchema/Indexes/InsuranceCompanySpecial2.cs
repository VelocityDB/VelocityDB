using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VelocityDbSchema.Indexes
{
  public class InsuranceCompanySpecial2 : InsuranceCompany
  {
    Int64 m_mySpecialField;
    public Int64 MySpecialField
    {
      get
      {
        return m_mySpecialField;
      }
      set
      {
        Update();
        m_mySpecialField = value;
      }
    }
    public InsuranceCompanySpecial2(string name, string phoneNumber):base(name, phoneNumber)
    {
    }
  }
}
