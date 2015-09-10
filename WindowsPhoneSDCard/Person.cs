using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VelocityDb;
using VelocityDb.Collection;

namespace VelocityDbSchema.Samples.AllSupportedSample
{
  public class Person : OptimizedPersistable
  {
    string firstName;
    string lastName;
    ushort age;
    UInt64 idNumber;
    Person bestFriend;
    SortedSetAny<Person> friends;
    static Random randGen = new Random(5);
    public Person()
    {
      int r = randGen.Next(9999);
      firstName = r.ToString();
      r = randGen.Next(999999);
      lastName = r.ToString();
      age = (ushort)randGen.Next(150);
      idNumber = (ulong)randGen.Next();
      idNumber <<= 32;
      idNumber += (ulong)randGen.Next();
    }
    public Person(string firstName, string lastName, ushort age, Person bestFriend = null)
    {
      this.firstName = firstName;
      this.lastName = lastName;
      this.age = age;
      this.bestFriend = bestFriend;
      friends = new SortedSetAny<Person>();
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

    public SortedSetAny<Person> Friends
    {
      get
      {
        return friends;
      }
    }

    // Required to make automatic use of BTreeSet lookup use binary search instead of linear search (must know what field this propert accesses)
    [FieldAccessor("idNumber")]
    public UInt64 IdNumber
    {
      get
      {
        return idNumber;
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
      return base.ToString() + " FirstName: " + FirstName + " LastName: " + lastName + "Age: " + age + " IdNumber: " + IdNumber;
    }
  }
}
