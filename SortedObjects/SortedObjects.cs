using System;
using System.Collections.Generic;
using System.Linq;
using VelocityDb;
using VelocityDb.Session;
using VelocityDb.Collection.BTree;
using VelocityDb.Collection.Comparer;
using VelocityDbSchema.Samples.AllSupportedSample;
using System.IO;

namespace SortedObjects
{
  class SortedObjects
  {
    static readonly string s_systemDir = "SortedObjects"; // appended to SessionBase.BaseDatabasePath for a full path

    static void Main(string[] args)
    {
      try
      {
        Oid bTreeId;
        bool createOnly = args.Length > 0;
        UInt64 someRandomPersonsIdNumber = 0;
        using (SessionNoServer session = new SessionNoServer(s_systemDir))
        {
          Console.WriteLine("Running with databases in directory: " + session.SystemDirectory);
          const UInt32 numberOfPersons = 1000000;
          const ushort nodeMaxSize = 5000;
          const ushort comparisonByteArraySize = sizeof(UInt64); // enough room to hold entire idNumber of a Person
          const bool comparisonArrayIsCompleteKey = true;
          const bool addIdCompareIfEqual = false;
          Person person;
          session.BeginUpdate();
          CompareByField<Person> compareByField = new CompareByField<Person>("idNumber", session, addIdCompareIfEqual);
          BTreeSet<Person> bTree = new BTreeSet<Person>(compareByField, session, nodeMaxSize, comparisonByteArraySize, comparisonArrayIsCompleteKey);
          session.Persist(bTree); // Persist the root of the BTree so that we have something persisted that can be flushed to disk if memory available becomes low
          for (int i = 0; i < numberOfPersons; i++)
          {
            person = new Person();
            if (i % 1000 == 0)
              someRandomPersonsIdNumber = person.IdNumber;
            bTree.AddFast(person);
          }
          bTreeId = bTree.Oid;
          session.Commit();
        }
        if (!createOnly)
          using (SessionNoServer session = new SessionNoServer(s_systemDir))
          {
            session.BeginRead();
            BTreeSet<Person> bTree = session.Open<BTreeSet<Person>>(bTreeId);
            foreach (Person person in (IEnumerable<Person>)bTree)
            {
              if (person.IdNumber > 196988888791402)
              {
                Console.WriteLine(person);
                break;
              }
            }
            session.Commit();
            session.BeginRead();
            Person lookupPerson = new Person(someRandomPersonsIdNumber);
            UInt64 id = bTree.GetKeyId(lookupPerson); // lookup without opening object having the key field value of someRandomPersonsIdNumber
            Person lookedUpPerson = null;
            bTree.TryGetKey(lookupPerson, ref lookedUpPerson);
            if (id != lookedUpPerson.Id)
              throw new UnexpectedException("Should match");
            // this LINQ statement will trigger a binary search lookup (not a linear serach) of the matching Person objects in the BTreeSet
            Console.WriteLine((from person in bTree where person.IdNumber > 196988888791402 select person).First());
            session.Commit();
          }
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.ToString());
      }
    }
  }
}
