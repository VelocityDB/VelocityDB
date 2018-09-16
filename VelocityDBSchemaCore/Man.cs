using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb.Session;

namespace VelocityDbSchema
{
  public class Man : Person
  {
    DateTime birthday;

    public Man()
    {
    }

    public Man(Person spouse, Person bestFriend, DateTime birthday)
      : base(spouse, bestFriend)
    {
      this.birthday = birthday;
    }

    public Man(string firstName, string lastName, UInt16 age, long ssn, DateTime birthday, Person bestFriend = null, Person spouse = null)
      : base(firstName, lastName, age, ssn, bestFriend, spouse)
		{
      this.birthday = birthday;
    }
  }
}
