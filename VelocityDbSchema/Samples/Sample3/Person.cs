using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Collection;
using VelocityDb.Session;

namespace VelocityDbSchema.Samples.Sample3
{
  public class Person : OptimizedPersistable
  {
    string firstName;
    string lastName;
    UInt16 age;
    Person bestFriend;
    List<Person> friends;
    [AutoIncrement]
#pragma warning disable 0169
    UInt64 autoIncrement;
#pragma warning restore 0169 
    public Person(string firstName, string lastName, UInt16 age, Person bestFriend = null)
    {
      this.firstName = firstName;
      this.lastName = lastName;
      this.age = age;
      this.bestFriend = bestFriend;
      friends = new List<Person>();
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

    public List<Person> Friends
    {
      get
      {
        return friends;
      }
    }
  }
}
