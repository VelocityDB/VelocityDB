using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VelocityDbSchema.OnlineStoreFinder
{
  public class LinkShareProduct : Product
  {
    enum ModificationEnum { Insert, Update, Delete };
    UInt64 m_productId;
    string m_primaryCategory;
    string m_secondaryCategories; // optional
    int m_classID; // optional
    string m_M1; // optional
    string m_pixel; // optional
    string m_miscellaneousAttribute; // optional
    string m_attribute2; // optional
    string m_attribute3; // optional
    string m_attribute4; // optional
    string m_attribute5; // optional
    string m_attribute6; // optional
    string m_attribute7; // optional
    string m_attribute8; // optional
    string m_attribute9; // optional
    string m_attribute10; // optional
    ModificationEnum m_modification;

    public UInt64 ProductId
    {
      get
      {
        return m_productId;
      }
      set
      {
        Update();
        m_productId = value;
      }
    }

    public string PrimaryCategory
    {
      get
      {
        return m_primaryCategory;
      }
      set
      {
        Update();
        m_primaryCategory = value;
      }
    }

    public string SecondaryCategories
    {
      get
      {
        return m_secondaryCategories;
      }
      set
      {
        Update();
        m_secondaryCategories = value;
      }
    }
  }
}
