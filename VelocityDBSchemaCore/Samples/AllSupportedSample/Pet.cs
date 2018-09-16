using System;
using System.Collections.Generic;
using System.Text;
using VelocityDb.Session;

namespace VelocityDbSchema.Samples.AllSupportedSample
{
  [Serializable]
  public class Pet
  {
    string name;
    short age;
    public List<Pet> friends;
    public Pet() { }
    public Pet(string aName, short anAge)
    {
      name = aName;
      age = anAge;
      friends = new List<Pet>(2);
    }

    public string Name
    {
      get
      {
        return name;
      }
      set
      {
        name = value;
      }
    }
  }
}
