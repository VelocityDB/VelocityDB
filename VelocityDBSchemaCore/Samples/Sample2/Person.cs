using System;
using VelocityDb;

namespace VelocityDbSchema.Samples.Sample2
{
  public class Person : OptimizedPersistable
  {
    string m_firstName;
    string m_lastName;
    UInt16 m_age;
    Person m_bestFriend;

    public Person(string firstName, string lastName, UInt16 age, Person bestFriend = null)
    {
      m_firstName = firstName;
      m_lastName = lastName;
      m_age = age;
      m_bestFriend = bestFriend;
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
  }
}
