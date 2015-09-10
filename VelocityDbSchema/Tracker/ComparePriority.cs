using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb.Session;
using VelocityDb.Collection.Comparer;

namespace VelocityDbSchema.Tracker
{
  class ComparePriority : VelocityDbComparer<Issue>
  {
    public ComparePriority() { }
    public override int Compare(Issue aIssue, Issue bIssue)
    {
      int compareValue = aIssue.Priority.CompareTo(bIssue.Priority);
      if (compareValue != 0)
        return compareValue;
      return base.Compare(aIssue, bIssue);
    }
  }
}

