using System;
using System.Collections.Generic;
using VelocityDb;
using VelocityDb.TypeInfo;

namespace VelocityDbSchema.Samples.Sample3
{
  public class Person : OptimizedPersistable
  {
    string m_firstName;
    string m_lastName;
    UInt16 m_age;
    Person m_bestFriend;
   // [Embed(false)]
    List<Person> m_friends;
    [AutoIncrement]
#pragma warning disable 0169
    UInt64 m_autoIncrement;
#pragma warning restore 0169 
    public Person(string firstName, string lastName, UInt16 age, Person bestFriend = null)
    {
      m_firstName = firstName;
      m_lastName = lastName;
      m_age = age;
      m_bestFriend = bestFriend;
      m_friends = new List<Person>();
    }

    public Person BestFriend
    {
      get
      {
        return m_bestFriend;
      }
      set
      {
        Update();
        m_bestFriend = value;
      }
    }

    public string FirstName
    {
      get
      {
        return m_firstName;
      }
      set
      {
        Update();
        m_firstName = value;
      }
    }

    public List<Person> Friends
    {
      get
      {
        return m_friends;
      }
    }
  }
}
