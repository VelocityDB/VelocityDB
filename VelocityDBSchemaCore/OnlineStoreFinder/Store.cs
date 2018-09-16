using System;
using VelocityDb;
using VelocityDb.Collection;

namespace VelocityDbSchema.OnlineStoreFinder
{
  public class Store : OptimizedPersistable
  {    
    public Store()
    {
      m_categoryList = new VelocityDbList<StoreInCategory>();
      m_dateTimeCreated = DateTime.Now;
      m_isValid = true;
    }
    public Store(string name)
    {
      this.m_name = name;
      m_dateTimeCreated = m_modifyDate = DateTime.Now;
      m_categoryList = new VelocityDbList<StoreInCategory>();
      m_isValid = true;
    }

    public Store(string name, string description, string keyWords, string rating, string dateTimeCreated, string dateTimeUpdated, string isValid)
    {
      this.m_name = name;
      this.m_description = description;
      this.m_keyWords = description;
      int.TryParse(rating, out this.m_rating);
      DateTime.TryParse(dateTimeCreated, out this.m_dateTimeCreated);
      DateTime.TryParse(dateTimeUpdated, out this.m_modifyDate);
      m_categoryList = new VelocityDbList<StoreInCategory>();
      this.m_isValid = bool.Parse(isValid);
    }

    public Store(string name, string description, string keyWords, int rating)
    {
      this.m_name = name;
      this.m_description = description;
      this.m_keyWords = keyWords;
      this.m_rating = rating;
      m_dateTimeCreated = DateTime.Now;
      m_modifyDate = DateTime.Now;
      m_categoryList = new VelocityDbList<StoreInCategory>();
      m_isValid = true;
    }

    string m_name;
    string m_description;
    string m_keyWords;
    bool m_isValid;
    public VelocityDbList<StoreInCategory> m_categoryList;
    public DateTime m_dateTimeCreated;
    public DateTime m_modifyDate;
    public int m_rating;
    [NonSerialized]
    public StoreBase m_cjStore;

    public string Description
    {
      get
      {
        return m_description;
      }
      set
      {
        Update();
        m_modifyDate = DateTime.Now;
        m_description = value;
      }
    }

    public DateTime DateTimeCreated
    {
      get
      {
        return m_dateTimeCreated;
      }
    }
    
    public bool IsValid
    {
      get
      {
        return m_isValid;
      }
      set
      {
        Update();
        m_modifyDate = DateTime.Now;
        m_isValid = value;
      }
    }

    public string KeyWords
    {
      get
      {
        return m_keyWords;
      }
      set
      {
        Update();
        m_modifyDate = DateTime.Now;
        m_keyWords = value;
      }
    }

     public string Name
    {
      get
      {
        return m_name;
      }
      set
      {
        Update();
        m_modifyDate = DateTime.Now;
        m_name = value;
      }
    }   

    public int Rating
    {
      get
      {
        return m_rating;
      }
      set
      {
        Update();
        m_modifyDate = DateTime.Now;
        m_rating = value;
      }
    }

    public override string ToString()
    {
      return m_name + " " + Oid.ToString();
    }
  }
}
