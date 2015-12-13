using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;

namespace AutoClaimUsingSQL
{
  public class AutoClaimEntityFrameworkContext : DbContext
  {
    public DbSet<MitchellClaimType> Claims { get; set; }
  }
}
