using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Collection;
using VelocityDb.Session;

namespace VelocityDbSchema.Samples.Sample4
{
  public class Person : OptimizedPersistable
  {
    string firstName;
    string lastName;
    UInt16 age;
    Person bestFriend;
    VelocityDbList<Person> friends;

    public Person(string firstName, string lastName, UInt16 age, Person bestFriend = null)
    {
      this.firstName = firstName;
      this.lastName = lastName;
      this.age = age;
      this.bestFriend = bestFriend;
      friends = new VelocityDbList<Person>();
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

    public string FirstName
    {
      get
      {
        return firstName;
      }
      set
      {
        Update();
        firstName = value;
      }
    }

    public VelocityDbList<Person> Friends
    {
      get
      {
        return friends;
      }
    }
  }
}
