using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb.Session;
using VelocityDb.Collection.Comparer;

namespace VelocityDbSchema.Tracker
{
  class CompareLastUpdatedBy : VelocityDbComparer<Issue>
  {
    public CompareLastUpdatedBy() { }
    public override int Compare(Issue aIssue, Issue bIssue)
    {
      int compareValue = aIssue.LastUpdatedBy.CompareTo(bIssue.LastUpdatedBy);
      if (compareValue != 0)
        return compareValue;
      return base.Compare(aIssue, bIssue);
    }
  }
}