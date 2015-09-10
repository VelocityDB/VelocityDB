using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Indexing;

namespace VelocityDbSchema.Indexes
{
  public class InsuranceCompany : OptimizedPersistable
  {
    [Index]
    [UniqueConstraint]
    [OnePerDatabase]
    string name;
    string phoneNumber;

    public InsuranceCompany(string name, string phoneNumber)
    {
      this.name = name;
      this.phoneNumber = phoneNumber;
    }

    [FieldAccessor("name")]
    public string Name
    {
      get
      {
        return name;
      }
    }
  }
}
