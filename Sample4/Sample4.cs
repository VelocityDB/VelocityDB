using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Session;
using VelocityDbSchema.Samples.Sample4;
using VelocityDb.Collection;
using System.IO;
using FuzzyString;

namespace Sample4
{
  class Sample4
  {
    static readonly string s_systemDir = "Sample4"; // appended to SessionBase.BaseDatabasePath

    static void Main(string[] args)
    {
      try
      {
        SessionBase.DefaultCompressPages = PageInfo.compressionKind.LZ4;
        using (SessionNoServer session = new SessionNoServer(s_systemDir))
        {
          Console.WriteLine("Running with databases in directory: " + session.SystemDirectory);
          session.BeginUpdate();
          // delete (unpersist) all Person objects created in prior run
          foreach (Person p in session.AllObjects<Person>())
            p.Unpersist(session);
          // delete (unpersist) all VelocityDbList<Person> objects created in prior run
          foreach (VelocityDbList<Person> l in session.AllObjects<VelocityDbList<Person>>())
            l.Unpersist(session);
          Person robinHood = new Person("Robin", "Hood", 30);
          Person billGates = new Person("Bill", "Gates", 56, robinHood);
          Person steveJobs = new Person("Steve", "Jobs", 56, billGates);
          robinHood.BestFriend = billGates;
          session.Persist(steveJobs);
          steveJobs.Friends.Add(billGates);
          steveJobs.Friends.Add(robinHood);
          billGates.Friends.Add(billGates);
          robinHood.Friends.Add(steveJobs);
          session.Commit();
        }
        using (SessionNoServer session = new SessionNoServer(s_systemDir))
        {
          List<FuzzyStringComparisonOptions> options = new List<FuzzyStringComparisonOptions>();

          // Choose which algorithms should weigh in for the comparison
          options.Add(FuzzyStringComparisonOptions.UseOverlapCoefficient);
          options.Add(FuzzyStringComparisonOptions.UseLongestCommonSubsequence);
          options.Add(FuzzyStringComparisonOptions.UseLongestCommonSubstring);
          options.Add(FuzzyStringComparisonOptions.UseHammingDistance);
          options.Add(FuzzyStringComparisonOptions.UseJaccardDistance);
          options.Add(FuzzyStringComparisonOptions.UseJaroDistance);
          options.Add(FuzzyStringComparisonOptions.UseJaroWinklerDistance);
          options.Add(FuzzyStringComparisonOptions.UseLevenshteinDistance);
          options.Add(FuzzyStringComparisonOptions.UseRatcliffObershelpSimilarity);
          options.Add(FuzzyStringComparisonOptions.UseSorensenDiceDistance);
          options.Add(FuzzyStringComparisonOptions.UseTanimotoCoefficient);


          // Choose the relative strength of the comparison - is it almost exactly equal? or is it just close?
          FuzzyStringComparisonTolerance tolerance = FuzzyStringComparisonTolerance.Normal;

          session.BeginRead();
          foreach (Person p in session.AllObjects<Person>())
          {
            // Get a boolean determination of approximate equality
            foreach (string firstNameFuzzy in new string[] { "Rob", "Billy", "Mats", "Stevo", "stevo" })
            {
              bool result = firstNameFuzzy.ApproximatelyEquals(p.FirstName, options, tolerance);
              if (result)
                Console.WriteLine(firstNameFuzzy + " approximatly equals " + p.FirstName);
            }
          }
          session.Commit();
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.ToString());
      }
    }
  }
}
