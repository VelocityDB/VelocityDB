using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;

namespace VelocityDbSchema.Samples.Sample1
{
  public class Person : OptimizedPersistable
  {
    string firstName;
    string lastName;
    UInt16 age;

    public Person(string firstName, string lastName, UInt16 age)
    {
      this.firstName = firstName;
      this.lastName = lastName;
      this.age = age;
    }
  }
}
