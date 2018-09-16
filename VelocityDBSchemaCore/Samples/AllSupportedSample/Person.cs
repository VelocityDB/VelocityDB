using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Collection;
using VelocityDb.Session;
using VelocityDb.TypeInfo;

namespace VelocityDbSchema.Samples.AllSupportedSample
{
  public class Person : OptimizedPersistable
  {
    string m_firstName;
    string m_lastName;
    ushort m_age;
    UInt64 m_idNumber;
    Person m_bestFriend;
    [AutoIncrement]
#pragma warning disable 0169
    UInt64 m_autoIncrement;
#pragma warning restore 0169
    SortedSetAny<Person> m_friends;
    static Random s_randGen = new Random(5);
    public Person()
    {
      int r = s_randGen.Next(9999);
      m_firstName = r.ToString();
      r = s_randGen.Next(999999);
      m_lastName = r.ToString();
      m_age = (ushort)s_randGen.Next(150);
      m_idNumber = (ulong) s_randGen.Next();
      m_idNumber <<= 32;
      m_idNumber += (ulong) s_randGen.Next();
    }
    public Person(UInt64 idNumber)
    {
      m_idNumber = idNumber;
    }
    public Person(string firstName, string lastName, ushort age, Person bestFriend = null)
    {
      m_firstName = firstName;
      m_lastName = lastName;
      m_age = age;
      m_bestFriend = bestFriend;
      m_friends = new SortedSetAny<Person>();
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

    public SortedSetAny<Person> Friends
    {
      get
      {
        return m_friends;
      }
    }

    // Required to make automatic use of BTreeSet lookup use binary search instead of linear search (must know what field this property accesses)
    [FieldAccessor("m_idNumber")]
    public UInt64 IdNumber
    {
      get
      {
        return m_idNumber;
      }
    }
    
    public override UInt32 PlacementDatabaseNumber
    {
      get
      {
        return 11;
      }
    }

    public override string ToString()
    {
        return base.ToString() + " FirstName: " + FirstName + " LastName: " + m_lastName + "Age: " + m_age + " IdNumber: " + IdNumber;
    }
  }
}
