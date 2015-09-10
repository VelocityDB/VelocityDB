using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Collection;
using VelocityDb.Indexing;
using VelocityDb.Session;
using VelocityDbSchema.Samples.AllSupportedSample;

namespace VelocityDbSchema
{
	public class Person : OptimizedPersistable 
	{
    static UInt64 NextNum = 0;
    //string schemaChange;
		public string firstName;
		public string lastName;
		UInt16 age;
		public long ssn;
    //[AutoIncrement] // comment out die to not working well in multithreaded tests - lock conflict
    UInt64 createNumber = ++NextNum;
    //[Index]
#pragma warning disable 0169
    DateTime dateOfBirth;
#pragma warning restore 0169 
    Guid personID;
    //[AutoIncrement]// comment out die to not working well in multithreaded tests - lock conflict
    UInt64 autoIncrement;
    public WeakIOptimizedPersistableReference<Person> spouse;
    public List<Pet> pets;
    public WeakIOptimizedPersistableReference<Person> bestFriend;
    public VelocityDbList<WeakIOptimizedPersistableReference<Person>> friends;
    public static Random randGen = new Random(5);

    public Person(int arrayCapacity = 0) 
    {
      autoIncrement = 0;
      int r = randGen.Next(99999);
      firstName = r.ToString();
      r = randGen.Next(99999999);
      lastName = r.ToString();
      age = (UInt16)randGen.Next(150);
      ssn = randGen.Next();
      friends = new VelocityDbList<WeakIOptimizedPersistableReference<Person>>(arrayCapacity);
      this.pets = new List<Pet>(arrayCapacity);
      PersonID = Guid.NewGuid();
    }

    public Person(string firstName, string lastName, UInt16 age, long ssn, Person bestFriend, Person spouse)
		{
			this.firstName = firstName;
			this.lastName = lastName;
			this.age = age;
			this.ssn = ssn;
      if (spouse != null)
			  this.spouse = new WeakIOptimizedPersistableReference<Person>(spouse);
      this.pets = new List<Pet>();
      friends = new VelocityDbList<WeakIOptimizedPersistableReference<Person>>(0);
      if (bestFriend != null)
      {
        this.bestFriend = new WeakIOptimizedPersistableReference<Person>(bestFriend);
        friends.Add(new WeakIOptimizedPersistableReference<Person>(bestFriend));
      }
		}

		public Person(Person spouse, Person bestFriend)
		{
			//int r = randGen.Next(99999);
			//firstName = r.ToString();
			//r = randGen.Next(999999999);
			//lastName = r.ToString();
      firstName = "用して更新されます";
      lastName = "イベントサブスクリプションと通知は、クライアント/サーバAPIに追加";
      age = (UInt16)randGen.Next(150);
			ssn = randGen.Next();
      if (spouse != null)
        this.spouse = new WeakIOptimizedPersistableReference<Person>(spouse);
      this.pets = new List<Pet>();
      if (bestFriend != null)
        this.bestFriend = new WeakIOptimizedPersistableReference<Person>(bestFriend);
		}

    public Guid PersonID
    {
      get
      {
        return personID;
      }
      set
      {
        Update();
        personID = value;
      }
    }

    public UInt16 Age
    {
      get
      {
        return age;
      }
      set
      {
        Update();
        age = value;
      }
    }    

    public UInt64 AutoIncrement
    {
      get
      {
        return autoIncrement;
      }
    }
    
    public UInt64 Count
    {
      get
      {
        return createNumber;
      }
    }
	}
}