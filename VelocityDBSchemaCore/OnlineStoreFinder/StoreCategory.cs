using VelocityDb;
using VelocityDb.Collection;
using System;

namespace VelocityDbSchema.OnlineStoreFinder
{
  public class StoreCategory : OptimizedPersistable
  {    
    public StoreCategory()
    {
      storeInCategoryList = new VelocityDbList<StoreInCategory>();
      categoryList = new VelocityDbList<StoreCategory>();
      adList = new VelocityDbList<CategoryAd>();
    }
    public StoreCategory(string name, string url, int level, StoreCategory parent):this()
    {
      this.name = name;
      this.url = url;
      this.level = level;
      if (level == 0)
        this.parent = null;
      else
        this.parent = parent;
    }

    string name;
    string url;
    int level;
    public StoreCategory parent;
    public VelocityDbList<StoreInCategory> storeInCategoryList;
    public VelocityDbList<StoreCategory> categoryList;
    public VelocityDbList<CategoryAd> adList;

    public int Level
    {
      get
      {
        return level;
      }
      set
      {
        Update();
        level = value;
      }
    }

    public string Name
    {
      get
      {
        return name;
      }
      set
      {
        Update();
        name = value;
      }
    }

    public StoreCategory Parent
    {
      get
      {
        return parent;
      }
      set
      {
        Update();
        parent = value;
      }
    }

    public string Url
    {
      get
      {
        return url;
      }
      set
      {
        Update();
        url = value;
      }
    }

    public override string ToString()
    {
      return name + " " + Oid.ToString();
    }
  }
}

