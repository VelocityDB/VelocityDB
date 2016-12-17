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
  public class User : ReferenceTracked
  {
    string _name;
    string _surname;
    BTreeSet<Interaction> m_interactions;
    User m_backup;

    public User()
    {
    }

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

    public User Backup
    {
      get
      {
        return m_backup;
      }
      set
      {
        if (m_backup != value)
        {
          Update();
          var reference = new Reference(this, "m_backup");
          if (m_backup != null)
            m_backup.References.Remove(m_backup.References.Where(r => r.To == this).First()); // may be possible to automatize removing reference
          m_backup = value;
          if (m_backup != null)
            m_backup.References.AddFast(reference); // may be possible to automatize setting reference
        }
      }
    }

    public BTreeSet<Interaction> Interactions
    {
      get
      {
        return m_interactions;
      }
      set
      {
        Update();
        m_interactions = value;
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
          r.RemoveUserRelation(this);
        m_interactions.Unpersist(session);
      }
      base.Unpersist(session);
    }
  }
}
