using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
    string m_firstName;
    [StringLength(250)]
		string m_lastName;
		UInt16 m_age;
		public long m_ssn;
    //[AutoIncrement] // comment out die to not working well in multithreaded tests - lock conflict
    UInt64 m_createNumber = ++NextNum;
    //[Index]
#pragma warning disable 0169
    DateTime m_dateOfBirth;
#pragma warning restore 0169 
    Guid m_personID;
    //[AutoIncrement]// comment out die to not working well in multithreaded tests - lock conflict
    UInt64 m_autoIncrement;
    public WeakIOptimizedPersistableReference<Person> m_spouse;
    public List<Pet> m_pets;
    public WeakIOptimizedPersistableReference<Person> m_bestFriend;
    public VelocityDbList<WeakIOptimizedPersistableReference<Person>> m_friends;
    double m_longitude;
    double m_latitude;
    public static Random s_randGen = new Random(5);

    public Person(int arrayCapacity = 0) 
    {
      m_autoIncrement = 0;
      int r = s_randGen.Next(99999);
      m_firstName = r.ToString();
      r = s_randGen.Next(99999999);
      m_lastName = r.ToString();
      m_age = (UInt16)s_randGen.Next(150);
      m_ssn = s_randGen.Next();
      m_friends = new VelocityDbList<WeakIOptimizedPersistableReference<Person>>(arrayCapacity);
      this.m_pets = new List<Pet>(arrayCapacity);
      PersonID = Guid.NewGuid();
      m_longitude = (s_randGen.Next(360) - 180) * s_randGen.NextDouble();
      m_latitude = (s_randGen.Next(180) - 90) * s_randGen.NextDouble();
    }

    public Person(string firstName, string lastName, UInt16 age, long ssn, Person bestFriend, Person spouse)
		{
			m_firstName = firstName;
			m_lastName = lastName;
			m_age = age;
			m_ssn = ssn;
      if (spouse != null)
			  m_spouse = new WeakIOptimizedPersistableReference<Person>(spouse);
      m_pets = new List<Pet>();
      m_friends = new VelocityDbList<WeakIOptimizedPersistableReference<Person>>(0);
      if (bestFriend != null)
      {
        m_bestFriend = new WeakIOptimizedPersistableReference<Person>(bestFriend);
        m_friends.Add(new WeakIOptimizedPersistableReference<Person>(bestFriend));
      }
		}

		public Person(Person spouse, Person bestFriend)
		{
			//int r = randGen.Next(99999);
			//firstName = r.ToString();
			//r = randGen.Next(999999999);
			//lastName = r.ToString();
      m_firstName = "用して更新されます";
      m_lastName = "イベントサブスクリプションと通知は、クライアント/サーバAPIに追加";
      m_age = (UInt16)s_randGen.Next(150);
			m_ssn = s_randGen.Next();
      if (spouse != null)
        this.m_spouse = new WeakIOptimizedPersistableReference<Person>(spouse);
      this.m_pets = new List<Pet>();
      if (bestFriend != null)
        this.m_bestFriend = new WeakIOptimizedPersistableReference<Person>(bestFriend);
		}

    public override int CompareTo(object obj)
    {
      Person otherPerson = (Person)obj;
      if (otherPerson != null)
      {
        return m_ssn.CompareTo(otherPerson.m_ssn);
      }
      else
      {
        throw new ArgumentException("object is not a Person or is null");
      }
    }
    public Guid PersonID
    {
      get
      {
        return m_personID;
      }
      set
      {
        Update();
        m_personID = value;
      }
    }

    public UInt16 Age
    {
      get
      {
        return m_age;
      }
      set
      {
        Update();
        m_age = value;
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

    public string LastName
    {
      get
      {
        return m_lastName;
      }
      set
      {
        Update();
        m_lastName = value;
      }
    }

    public double Latitude
    {
      get
      {
        return m_latitude;
      }
      set
      {
        Update();
        m_latitude = value;
      }
    }

    public double Longitude
    {
      get
      {
        return m_longitude;
      }
      set
      {
        Update();
        m_longitude = value;
      }
    }  

    public UInt64 AutoIncrement
    {
      get
      {
        return m_autoIncrement;
      }
    }
    
    public UInt64 Count
    {
      get
      {
        return m_createNumber;
      }
    }
	}
}