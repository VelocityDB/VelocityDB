using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb.Session;

namespace VelocityDbSchema
{
  public class Woman : Person
  {
    public Woman():base()
    {
    }

    public Woman(Person spouse, Person bestFriend)
      : base(spouse, bestFriend)
    {
    }

    public Woman(string firstName, string lastName, UInt16 age, long ssn, Person bestFriend, Person spouse)
      : base(firstName, lastName, age, ssn, bestFriend, spouse)
		{
		}

    public bool OlderThan50
    {
      get
      {
        return Age > 50;
      }
    }

    public override string ToString()
    {
      return base.ToString() + " age: " + Age;
    }
  }
}
