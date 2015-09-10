using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using VelocityDb.Collection.Comparer;
using System.Reflection;

namespace VelocityDbSchema.Imdb
{
  [Obfuscation(Feature = "renaming", Exclude = true)]
  [Serializable]
  public class ActingByNameComparer : VelocityDbComparer<ActingPerson>
  {
    public override int Compare(ActingPerson a, ActingPerson b)
    {
      return a.Name.CompareTo(b.Name);
    }
  }
}
