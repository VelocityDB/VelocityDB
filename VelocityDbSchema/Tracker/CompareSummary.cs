using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb.Session;
using VelocityDb.Collection.Comparer;

namespace VelocityDbSchema.Tracker
{
  class CompareSummary : VelocityDbComparer<Issue>
  {
    public CompareSummary() { }
    public override int Compare(Issue aIssue, Issue bIssue)
    {
      int compareValue = aIssue.Summary.CompareTo(bIssue.Summary);
      if (compareValue != 0)
        return compareValue;
      return base.Compare(aIssue, bIssue);
    }
  }
}

