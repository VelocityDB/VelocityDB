using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VelocityDb;

using VelocityDb.Session;
using VelocityDbSchema.NUnit;

namespace NUnitTests
{
  
  class DynamicObjectTest
  {
    public class TestEvent : OptimizedPersistable
    {
      public string Name { get; set; }
      public DateTime Date { get; set; }
    }

    [TestFixture]
    public class DynamicDictionaryTests
    {
      private readonly string SystemDir = "DynamicTest";
      private readonly string LicenseFile = "c:/4.odb";
      private UInt64 _personId;
      private UInt64 _person2Id;
      private static string s_TestDynamicPerson = nameof(s_TestDynamicPerson);

      [SetUp]
      public void Initialize()
      {
        Console.WriteLine("\n***************Initializing...***************");

        using (var session = new SessionNoServer(SystemDir))
        {
          session.BeginUpdate();

          var path = Path.Combine(session.SystemDirectory, "4.odb");

          if (!File.Exists(path))
            File.Copy(LicenseFile, path, true);

          session.RegisterClass(typeof(DynamicDictionary));
          session.RegisterClass(typeof(PersistableDynamicDictionary));
          session.RegisterClass(typeof(TestEvent));

          session.Commit();
        }

        Console.WriteLine("***************Complete***************");
      }

      [Test]
      [Category("VelocityDb Dynamic")]
      [Property("Time", "Short")]
      public void DynamicDictionaryTest()
      {
        ExecuteTests<DynamicDictionary>();
      }

      [Test]
      [Category("VelocityDb Dynamic")]
      [Property("Time", "Short")]
      public void PersistableDynamicDictionaryTest()
      {
        ExecuteTests<PersistableDynamicDictionary>();
      }

      private void ExecuteTests<T>()
          where T : DynamicDictionary, new()
      {
        Console.WriteLine("\n***************Saving...***************");

        Save<T>();

        Console.WriteLine("***************Complete***************");

        Console.WriteLine("\n***************Loading...***************");

        Open();

        LoadAll();

        LoadByDynamicQuery();

        if (typeof(T) == typeof(PersistableDynamicDictionary))
          LoadByIndex();

        Console.WriteLine("***************Complete***************");
      }

      private void Save<T>()
          where T : DynamicDictionary, new()
      {
        using (SessionNoServer session = new SessionNoServer(SystemDir))
        {
          dynamic person = CreatePerson<T>();
          dynamic person2 = CreatePerson2<T>(person);

          OutputProperties("Person", person);

          OutputProperties("Person2", person2);

          session.BeginUpdate();

          _personId = session.Persist(person);
          _person2Id = session.Persist(person2);

          if (typeof(T) == typeof(PersistableDynamicDictionary))
          {
            session.Persist(new PersistableDynamicDictionary() { TypeName = "Test", ["Test"] = "Hello" });
            session.Persist(new PersistableDynamicDictionary() { TypeName = "Test", ["Test"] = "World" });
          }

          session.Commit();
        }
      }

      private dynamic CreatePerson<T>()
          where T : DynamicDictionary, new()
      {
        dynamic person = new T();
        dynamic child = new T();

        child.TypeName = s_TestDynamicPerson;
        child.FirstName = "Jane";
        child.LastName = "Doe";

        // Adding new dynamic properties. 
        // The TrySetMember method is called.
        person.TypeName = s_TestDynamicPerson;
        person.FirstName = "Ellen"; // String Works
        person.LastName = "Adams";
        person.IsRetired = false; // Bool Works
        person.Age = 16; // Integer Works
        person.Pi = 3.14159; // Double Works
        person.Color = ConsoleColor.DarkBlue; // Enum Works
        person.Duration = new TimeSpan(5, 5, 5, 5); // TimeSpan (Structure) Works
        person.Point = new System.Drawing.Point(1, 1); // Point (Structure) Works

        if (typeof(T) == typeof(DynamicDictionary))
        {
          person.Child = child; // Child T Works... On POCO Objects
        }

        // Guid -> Throws a Null-Reference Exception in TypeVersion.decodeISerializable(Byte[], SessionBase, ...)
        person.UniqueId = Guid.Parse("10000000-0000-0000-0000-000000000000");

        // DateTime -> Throws an ArgumentOutOfRangeException in DateTime.ctor(Int64 ticks) from DataMember.a(Type, Byte[], Object, ...) 
        person.BirthDate = new DateTime(1980, 1, 1);

        // List<TestEvent> -> Throws Null-Reference Exception in DataMember.b(Byte[], Int32, Type, ...)
        person.ScheduledEvents = new List<TestEvent>()
        {
            new TestEvent() { Name = "Election Day", Date = new DateTime(2016, 11, 8) },
            new TestEvent() { Name = "Inauguration Day ", Date = new DateTime(1017, 1, 20) }
        };

        return person;
      }

      private dynamic CreatePerson2<T>(dynamic person)
          where T : DynamicDictionary, new()
      {
        dynamic person2 = new T();

        person2.TypeName = s_TestDynamicPerson;
        person2.FirstName = "John";
        person2.LastName = "Doe";

        if (typeof(T) == typeof(DynamicDictionary))
        {
          person2.Child = person; // Child T Works... On POCO Objects
        }

        // TestEvent -> Throws a Null-Reference Exception in DataMember.ctor(TypeVersion, FieldInfo)
        //  Or Out Of Memory Exception in DataMember.a(Byte[], SessionBase)
        person2.Event = new TestEvent() { Name = "Birth Date", Date = new DateTime(1980, 1, 1) };

        // List<string> -> Throws a Null-Reference Exception in DataMember.b(Byte[], Int32, Type, ...)
        person2.KnownLanguages = new List<string> { "American English", "C\\C++\\C#" };

        return person2;
      }

      private void Open()
      {
        using (SessionNoServer session = new SessionNoServer(SystemDir))
        {
          session.BeginRead();

          dynamic person = session.Open(_personId);

          dynamic person2 = session.Open(_person2Id);

          session.Commit();

          OutputProperties("Person", person);

          OutputProperties("Person2", person2);

          AssertPerson(person);

          AssertPerson2(person2);
        }
      }

      private void LoadAll()
      {
        using (SessionNoServer session = new SessionNoServer(SystemDir))
        {
          Console.WriteLine("\nAllObjects<DynamicDictionary>().ToList()");

          session.BeginRead();

          var people = session.AllObjects<DynamicDictionary>().ToList();

          session.Commit();

          Console.WriteLine($"People.Count: {people.Count}");
        }
      }

      private void LoadByDynamicQuery()
      {
        using (SessionNoServer session = new SessionNoServer(SystemDir))
        {
          Console.WriteLine("\nDynamic Query");

          session.BeginRead();

          // Query on the Dynamic Property, Iterates over AllObjects<T>
          //  Note: This will throw an Exception if FirstName and LastName are not both defined on every DynamicDictionary.
          dynamic dynamicQuery1 =
              session.AllObjects<DynamicDictionary>()
                  .FirstOrDefault(x => x["FirstName"] == "Ellen" && x["LastName"] == "Adams");

          // Avoids Exception but still Iterates over AllObjects<T>
          dynamic dynamicQuery2 =
              session.AllObjects<DynamicDictionary>()
                  .FirstOrDefault((x) =>
                  {
                    dynamic dynX = x;
                    return dynX.ContainsProperty("FirstName") && dynX.ContainsProperty("LastName") &&
                                 dynX.FirstName == "John" && dynX.LastName == "Doe";
                  });

          // Dynamic Query Asserts
          AssertPerson(dynamicQuery1);
          AssertPerson2(dynamicQuery2);
        }
      }

      private void LoadByIndex()
      {
        using (SessionNoServer session = new SessionNoServer(SystemDir))
        {
          Console.WriteLine("\nLoad by Index Dynamic Query");

          session.BeginRead();

          session.TraceIndexUsage = true;

          // In Theory, this should use the TypeName Index on PersistableDynamicDictionary to return only those that are of the TestDynamicPerson type
          //  However, Index Tracing is not returning anything promising
          var dynamicIndex = session.Index<PersistableDynamicDictionary>()
              .Where(x => x.TypeName == s_TestDynamicPerson);

          Assert.IsNotNull(dynamicIndex, "Null Index");

          // Use the VelocityDb Linq Extension to return the count of all the PersistableDynamicDictionary objects in the Db
          Console.WriteLine($"Index Count: {session.Index<PersistableDynamicDictionary>().Count()}");

          // Enumerates the TypeName Filtered Index to Query by Dynamic Properties
          //  Note: This will throw an Exception if FirstName and LastName are not both defined on the TestDynamicPerson type.
          dynamic dynamicQuery1 = dynamicIndex
              .FirstOrDefault(x => x["FirstName"] == "Ellen" && x["LastName"] == "Adams");

          // Safer
          dynamic dynamicQuery2 = dynamicIndex
              .FirstOrDefault(
                  x =>
                      x.ContainsProperty("FirstName") && x.ContainsProperty("LastName") &&
                      x["FirstName"] == "John" && x["LastName"] == "Doe");

          session.Commit();

          // Dynamic Query Asserts
          AssertPerson(dynamicQuery1);
          AssertPerson2(dynamicQuery2);
        }
      }

      private void OutputProperties(string desc, dynamic dynamicObject)
      {
        Console.WriteLine($"\n---------------{desc}---------------");

        // Getting the value of the TypeName property.
        //  The TryGetMember is not called, because the property is defined in the class.
        Console.WriteLine($"TypeName: '{dynamicObject.TypeName}'");

        // Getting the value of the Count property.
        //  The TryGetMember is not called, because the property is defined in the class.
        Console.WriteLine($"Number of dynamic properties: {dynamicObject.Count}\n");

        // Getting values of the dynamic properties.
        //  The TryGetMember method is called.
        //  Note that property names are case-insensitive.
        Console.WriteLine($"{dynamicObject.firstName}  {dynamicObject.lastname}");

        foreach (var propertyName in dynamicObject.GetPropertyNames())
        {
          Console.WriteLine($"{propertyName} = {dynamicObject[propertyName.ToUpper()]?.ToString() ?? "<Null>"}");
        }

        // The following statement throws an exception at run time.
        //  There is no "address" property, so the TryGetMember method returns false and this causes a RuntimeBinderException.
        // Console.WriteLine(person.address);

        Console.WriteLine("----------------------------------------");
      }

      private void AssertPerson(dynamic dynamicTestPerson)
      {
        // Person Asserts
        Assert.IsNotNull(dynamicTestPerson, "Null Person1");
        Assert.IsTrue(dynamicTestPerson.TypeName == s_TestDynamicPerson, "TypeName");
        Assert.IsTrue(dynamicTestPerson.FirstName == "Ellen", "FirstName");
        Assert.IsTrue(dynamicTestPerson.LastName == "Adams", "LastName");
        Assert.IsTrue(dynamicTestPerson.IsRetired == false, "IsRetired");
        Assert.IsTrue(dynamicTestPerson.Age == 16, "Age");
        Assert.IsTrue(dynamicTestPerson.Pi == 3.14159, "Pi");
        Assert.IsTrue((ConsoleColor)dynamicTestPerson.Color == ConsoleColor.DarkBlue, "Color");
        Assert.IsTrue((System.Drawing.Point)dynamicTestPerson.Point == new System.Drawing.Point(1, 1), "Point");
        Assert.IsTrue(dynamicTestPerson.Duration == new TimeSpan(5, 5, 5, 5));

        if (dynamicTestPerson.GetType() == typeof(DynamicDictionary))
        {
          Assert.IsTrue(dynamicTestPerson.Child.TypeName == s_TestDynamicPerson, "TypeName");
          Assert.IsTrue(dynamicTestPerson.Child.FirstName == "Jane", "Child.FirstName");
          Assert.IsTrue(dynamicTestPerson.Child.LastName == "Doe", "Child.LastName");
        }

        Assert.IsTrue(dynamicTestPerson.UniqueId == Guid.Parse("10000000-0000-0000-0000-000000000000"), "UniqueId");
        Assert.IsTrue(dynamicTestPerson.BirthDate == new DateTime(1980, 1, 1), "BirthDate");
        Assert.IsNotNull(dynamicTestPerson.ScheduledEvents, "ScheduledEvents");
        Assert.IsTrue(dynamicTestPerson.ScheduledEvents.Count == 2, "ScheduledEvents.Count");
        Assert.IsTrue(dynamicTestPerson.ScheduledEvents[0].Name == "Election Day", "ScheduledEvents[0].Name");
      }

      private void AssertPerson2(dynamic dynamicTestPerson)
      {
        // Person 2 Asserts
        Assert.IsNotNull(dynamicTestPerson, "Null Person2");
        Assert.IsTrue(dynamicTestPerson.TypeName == s_TestDynamicPerson, "TypeName");
        Assert.IsTrue(dynamicTestPerson.FirstName == "John", "FirstName");
        Assert.IsTrue(dynamicTestPerson.LastName == "Doe", "LastName");

        if (dynamicTestPerson.GetType() == typeof(DynamicDictionary))
        {
          Assert.IsTrue(dynamicTestPerson.Child.TypeName == s_TestDynamicPerson, "TypeName");
          Assert.IsTrue(dynamicTestPerson.Child.FirstName == "Ellen", "Child.FirstName");
          Assert.IsTrue(dynamicTestPerson.Child.LastName == "Adams", "Child.LastName");
        }

        Assert.IsTrue(dynamicTestPerson.Event.Name == "Birth Date", "Event.Name");
        Assert.IsTrue(dynamicTestPerson.Event.Date == new DateTime(1980, 1, 1), "Event.Date");
        Assert.IsNotNull(dynamicTestPerson.KnownLanguages, "KnownLanguages");
        Assert.IsTrue(dynamicTestPerson.KnownLanguages.Count == 2, "KnownLanguages.Count");
        Assert.IsTrue(dynamicTestPerson.KnownLanguages[0] == "American English", "KnownLanguages[0]");
      }

      [TearDown]
      public void CleanUp()
      {
        Console.WriteLine("\n***************Cleaning up...***************");

        // Delete the DB
        //  Prevents AllObjects<DynamicDictionary> from Breaking due to Not Yet Supported Dynamic Properties
        try
        {
          var path = Path.Combine(SessionBase.BaseDatabasePath, SystemDir);

          var dirInfo = new DirectoryInfo(path);

          if (dirInfo.Exists)
            dirInfo.Delete(true);
        }
        catch (Exception ex)
        {
          Console.WriteLine($"Exception Occured in CleanUp(): {ex.Message}");
        }

        Console.WriteLine("***************Complete***************");
      }
    }

  }
}
