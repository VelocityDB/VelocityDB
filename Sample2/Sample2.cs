using VelocityDb.Session;
using VelocityDbSchema.Samples.Sample2;

namespace Sample2
{
  class Sample2
  {
    static readonly string systemDir = "Sample2"; // appended to SessionBase.BaseDatabasePath

    static void Main(string[] args)
    {
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        Person robinHood = new Person("Robin", "Hood", 30);
        Person billGates = new Person("Bill", "Gates", 56, robinHood);
        Person steveJobs = new Person("Steve", "Jobs", 56, billGates);
        robinHood.BestFriend = billGates;
        session.Persist(steveJobs); // the other persons will be persisted implicetly by reachability from "Steve Jobs" person object
        session.Commit();
      }
    }
  }
}
