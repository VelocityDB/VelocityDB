using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb.Session;
using VelocityDb.Collection.Comparer;

namespace VelocityDbSchema.Tracker
{
  class CompareCategory : VelocityDbComparer<Issue>
  {
    public CompareCategory() { }
    public override int Compare(Issue aIssue, Issue bIssue)
    {
      int compareValue = aIssue.Category.CompareTo(bIssue.Category);
      if (compareValue != 0)
        return compareValue;
      return base.Compare(aIssue, bIssue);
    }
  }
}
