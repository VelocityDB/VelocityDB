using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb.Session;
using VelocityDb.Collection.Comparer;

namespace VelocityDbSchema.Tracker
{
  class CompareByVersion : VelocityDbComparer<Issue>
  {
    public CompareByVersion() { }
    public override int Compare(Issue aIssue, Issue bIssue)
    {
      int compareValue = aIssue.Version.CompareTo(bIssue.Version);
      if (compareValue != 0)
        return compareValue;
      return base.Compare(aIssue, bIssue);
    }
  }
}
