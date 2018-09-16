using System;
using VelocityDb;
using VelocityDb.Collection;

namespace VelocityDbSchema.Samples.Sample4
{
  public class Person : OptimizedPersistable
  {
    string m_firstName;
    string m_lastName;
    UInt16 m_age;
    Person m_bestFriend;
    VelocityDbList<Person> m_friends;

    public Person(string firstName, string lastName, UInt16 age, Person bestFriend = null)
    {
      m_firstName = firstName;
      m_lastName = lastName;
      m_age = age;
      m_bestFriend = bestFriend;
      m_friends = new VelocityDbList<Person>();
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

    public VelocityDbList<Person> Friends
    {
      get
      {
        return m_friends;
      }
    }
  }
}
