using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Session;

namespace VelocityDbSchema.Samples.Sample2
{
  public class Person : OptimizedPersistable
  {
    string firstName;
    string lastName;
    UInt16 age;
    Person bestFriend;

    public Person(string firstName, string lastName, UInt16 age, Person bestFriend = null)
    {
      this.firstName = firstName;
      this.lastName = lastName;
      this.age = age;
      this.bestFriend = bestFriend;
    }

    public Person BestFriend
    {
      get
      {
        return bestFriend;
      }
      set
      {
        Update();
        bestFriend = value;
      }
    }
  }
}
