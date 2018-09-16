using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Collection.BTree;
using VelocityDb.Session;
using VelocityDb.TypeInfo;

namespace VelocityDbSchema.Samples.Relations
{
  public class Interaction : ReferenceTracked
  {
    string _subject;
    string _notes;

    public Interaction(Customer customer, User user, SessionBase session)
    {
      session.Persist(this);
      customer.AddInteraction(this);
      user.AddInteraction(this);
    }

    public void RemoveUserRelation(User user)
    {
      References.Remove(References.Where(r => r.To == user.Interactions).First());
      if (References.Count() == 0)
        Unpersist(Session);
    }

    public void RemoveCustomerRelation(Customer customer)
    {
      References.Remove(References.Where(r => r.To == customer.Interactions).First());
      if (References.Count() == 0)
        Unpersist(Session);
    }

    public string Notes
    {
      get
      {
        return _notes;
      }
      set
      {
        Update();
        _notes = value;
      }
    }
    public string Subject
    {
      get
      {
        return _subject;
      }
      set
      {
        Update();
        _subject = value;
      }
    }
  }
}
