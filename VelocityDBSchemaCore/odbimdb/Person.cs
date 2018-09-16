using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Collection;
using VelocityDb.Collection.BTree;
using VelocityDb.Session;

namespace VelocityDbSchema.odbimdb
{
  public class Person : OptimizedPersistable
  {
    public BTreeSet<InMovieAs> inMovieAs;

    public Person(string name, SessionBase session)
    {
      Name = name;
      inMovieAs = new BTreeSet<InMovieAs>(null, session);
    }
    public string Name { get; set; }
    public string PersonId { get; set; }
    public Image Photo { get; set; }
    public DateTime Birthday { get; set; }
    public string Biography { get; set; }
    public string Nominations { get; set; }
    public string Character { get; set; }
  }
}
