using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VelocityDbSchema.Indexes
{
  public class InsuranceCompanySpecial : InsuranceCompany
  {
    string m_mySpecialField;
    public string MySpecialField
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
    public InsuranceCompanySpecial(string name, string phoneNumber):base(name, phoneNumber)
    {
    }

    public override bool AllowOtherTypesOnSamePage
    {
      get
      {
        return false;
      }
    }
  }
}
