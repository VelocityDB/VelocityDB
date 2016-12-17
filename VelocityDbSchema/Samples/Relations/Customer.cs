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
  public class Customer : OptimizedPersistable
  {
    string _name;
    string _surname;
    string _address;
    decimal _balance;
    BTreeSet<Interaction> m_interactions;

    public void AddInteraction(Interaction interaction)
    {
      if (m_interactions == null)
        m_interactions = new BTreeSet<Interaction>();
      if (m_interactions.Add(interaction))
      {
        var reference = new Reference(m_interactions, "m_interactions");
        interaction.References.AddFast(reference); // may be possible to automatize setting reference
      }
    }

    public string Address
    {
      get
      {
        return _address;
      }
      set
      {
        Update();
        _address = value;
      }
    }

    public Decimal Balance
    {
      get
      {
        return _balance;
      }
      set
      {
        Update();
        _balance = value;
      }
    }

    public BTreeSet<Interaction> Interactions
    {
      get
      {
        return m_interactions;
      }
    }

    public string Name
    {
      get
      {
        return _name;
      }
      set
      {
        Update();
        _name = value;
      }
    }

    public string Surame
    {
      get
      {
        return _surname;
      }
      set
      {
        Update();
        _surname = value;
      }
    }

    public override void Unpersist(SessionBase session)
    {
      if (m_interactions != null)
      {
        foreach (var r in m_interactions)
          r.RemoveCustomerRelation(this);
        m_interactions.Unpersist(session);
      }
      base.Unpersist(session);
    }
  }
}
