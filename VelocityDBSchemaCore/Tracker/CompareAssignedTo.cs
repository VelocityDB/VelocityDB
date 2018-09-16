using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb.Session;
using VelocityDb.Collection.Comparer;

namespace VelocityDbSchema.Tracker
{
  public class CompareAssignedTo : VelocityDbComparer<Issue>, IEqualityComparer<Issue>
  {
    public CompareAssignedTo() { }
    public override int Compare(Issue aIssue, Issue bIssue)
    {
      int compareValue = 0;
      if (aIssue.AssignedTo != null && bIssue.AssignedTo != null)
      {
        compareValue = aIssue.AssignedTo.CompareTo(bIssue.AssignedTo);
        if (compareValue != 0)
          return compareValue;
      }
      else
        if (aIssue.AssignedTo == null && bIssue.AssignedTo != null)
          return -1;
        else if (aIssue.AssignedTo != null && bIssue.AssignedTo == null)
          return 1;
      return base.Compare(aIssue, bIssue);
    }

    public int GetHashCode(Issue aIssue)
    {
      if (aIssue.AssignedTo != null)
        return aIssue.AssignedTo.GetHashCode();
      return 0;
    }

    public bool Equals(Issue a, Issue b)
    {
      return Compare(a, b) == 0;
    }
  }
}