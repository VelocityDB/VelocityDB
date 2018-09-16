using System;
using VelocityDb;

namespace VelocityDbSchema.OnlineStoreFinder
{
  public class StoreInCategory : OptimizedPersistable
  {    
    public Store storeT;
    public string click;
    public string image;
    public string specialImage;
    public string script;
    public string textLink;
    public string textClick;
    public string controlUrl;
    public StoreCategory category;
    DateTime createDate;
    DateTime modifyDate;
    public StoreInCategory() 
    {
      modifyDate = createDate = DateTime.Now;
    }

    public StoreInCategory(Store store, StoreCategory category)
    {
      this.storeT = store;
      this.category = category;
      createDate = modifyDate = DateTime.Now;
    }

    public StoreInCategory(Store store, string click, string image, string specialImage, string script, string textLink,
      string textClick, string controlUrl, StoreCategory category, DateTime createDate, DateTime modifyDate)
    {
      this.storeT = store;
      this.click = click;
      this.image = image;
      this.specialImage = specialImage;
      this.script = script;
      this.textLink = textLink;
      this.textClick = textClick;
      this.controlUrl = controlUrl;
      this.category = category;
      this.createDate = createDate;
      this.modifyDate = modifyDate;
    }

    public DateTime DateTimeCreated
    {
      get
      {
        return createDate;
      }
    }

    public DateTime DateTimeUpdated
    {
      get
      {
        return modifyDate;
      }
    }

    public Store Store
    {
      get
      {
        return storeT;
      }
      set
      {
        Update();
        modifyDate = DateTime.Now;
        storeT = value;
      }
    }

    public string Click
    {
      get
      {
        return click == null ? "" : click;
      }
      set
      {
        Update();
        modifyDate = DateTime.Now;
        click = value;
      }
    }

    public string Image
    {
      get
      {
        return image == null ? "" : image;
      }
      set
      {
        Update();
        modifyDate = DateTime.Now;
        image = value;
      }
    }    
    
    public string SpecialImage
    {
      get
      {
        return specialImage == null ? "" : specialImage;
      }
      set
      {
        Update();
        modifyDate = DateTime.Now;
        specialImage = value;
      }
    }    
    
    public string Script
    {
      get
      {
        return script == null ? "" : script;
      }
      set
      {
        Update();
        modifyDate = DateTime.Now;
        script = value;
      }
    }

    public string TextLink
    {
      get
      {
        return textLink == null ? "" : textLink; ;
      }
      set
      {
        Update();
        modifyDate = DateTime.Now;
        textLink = value;
      }
    }    
    
    public string TextClick
    {
      get
      {
        return textClick == null ? "" : textClick;
      }
      set
      {
        Update();
        modifyDate = DateTime.Now;
        textClick = value;
      }
    }   
    
    public string ControlUrl
    {
      get
      {
        return controlUrl == null ? "" : controlUrl;
      }
      set
      {
        Update();
        modifyDate = DateTime.Now;
        controlUrl = value;
      }
    }

    public StoreCategory Category
    {
      get
      {
        return category;
      }
      set
      {
        Update();
        modifyDate = DateTime.Now;
        category = value;
      }
    }

    public override int CompareTo(object obj)
    {
      StoreInCategory other = obj as StoreInCategory;
      int compareValue = storeT.Name.CompareTo(other.storeT.Name);
      if (compareValue != 0)
        return compareValue;
      return category.Name.CompareTo(other.category.Name);
    }
  }
}
