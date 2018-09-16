using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using VelocityDb;
using VelocityDb.Session;
using VelocityDbSchema.Samples.Sample4;

namespace DesEncrypted
{
  class DesEncrypted
  {
    const UInt32 desEncryptedStartDatabaseNumber = 10;
    static readonly string s_systemDir = "DesEncryptedCore";

    static void Main(string[] args)
    {
      try
      {
        using (SessionNoServer session = new SessionNoServer(s_systemDir))
        {
          DatabaseLocation localLocation = new DatabaseLocation(Dns.GetHostName(), Path.Combine(session.SystemDirectory, "desEncryptedLocation"), desEncryptedStartDatabaseNumber, UInt32.MaxValue,
            session, PageInfo.compressionKind.LZ4, PageInfo.encryptionKind.desEncrypted);
          session.BeginUpdate();
          session.NewLocation(localLocation);
          localLocation.DesKey = SessionBase.TextEncoding.GetBytes("5d9nndwy"); // Des keys are 8 bytes long
          Person robinHood = new Person("Robin", "Hood", 30);
          Person billGates = new Person("Bill", "Gates", 56, robinHood);
          Person steveJobs = new Person("Steve", "Jobs", 56, billGates);
          robinHood.BestFriend = billGates;
          session.Persist(steveJobs);
          steveJobs.Friends.Add(billGates);
          steveJobs.Friends.Add(robinHood);
          billGates.Friends.Add(billGates);
          robinHood.Friends.Add(steveJobs);
          session.Commit();
        }
        using (SessionNoServer session = new SessionNoServer(s_systemDir))
        { // Des keys are not persisted in DatabaseLocation (for safety). Instead they are stored as *.des files
          // in the users document directory of the user that created the DatabaseLocation. These files can be copied
          // to other user's document directory when acccess is desired for other users. 
          // Path to user document dir is given by C#: Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
          session.BeginRead();
          var allPersonsEnum = session.AllObjects<Person>();
          foreach (Person obj in allPersonsEnum)
          {
            Person person = obj as Person;
            if (person != null)
              Console.WriteLine(person.FirstName);
          }
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
