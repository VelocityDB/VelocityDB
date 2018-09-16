using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Collection;
using VelocityDb.Session;

namespace VelocityDbSchema.Samples.WeakIOptimizedPersistableReferences
{
  public class Person : OptimizedPersistable
  {
    static Random randGen = new Random(5);

    string firstName;
    string lastName;
    WeakIOptimizedPersistableReference<VelocityDbList<Person>> friendsRef;

    public Person(Person person = null) // creates a random Person object
    {
      int r = randGen.Next(99999);
      firstName = r.ToString();
      r = randGen.Next(99999999);
      lastName = r.ToString();
      VelocityDbList<Person> personList = new VelocityDbList<Person>();
      if (person != null && person.IsPersistent)
      {
        personList.Persist(person.Session, person);
        personList.Add(person);
        friendsRef = new WeakIOptimizedPersistableReference<VelocityDbList<Person>>(personList);
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
        return friendsRef.GetTarget(false, null);
      }
    }
  }
}