using System;
using VelocityDb;

namespace VelocityDbSchema.OnlineStoreFinder
{
  public class Coupon : OptimizedPersistable
  {    
    string m_category;
    DateTime m_startDate;
    DateTime m_expireDate;
    int m_promotionType;
    string m_description;
    string m_code;
    string m_link;
    string m_image;

    public Coupon() { }

    public Coupon(DateTime start, DateTime expire, string promotion, string description, string code, string link, string image)
    {
      m_startDate = start;
      m_expireDate = expire;
      int.TryParse(promotion, out m_promotionType);
      m_description = description;
      m_code = code;
      m_link = link;
      m_image = image;
    }

    public Coupon(DateTime start, DateTime expire, int promotionType, string description, string code, string link, string image)
    {
      m_startDate = start;
      m_expireDate = expire;
      m_promotionType = promotionType;
      m_description = description;
      m_code = code;
      m_link = link;
      m_image = image;
    }

    public string Category
    {
      get { return m_category;  }
      set
      {
        Update();
        m_category = value;
      }
    }
    public DateTime ExpireDate
    {
      get { return m_expireDate;  }
      set
      {
        Update();
        m_expireDate = value;
      }
    }

    public DateTime StartDate
    {
      get { return m_startDate;  }
      set
      {
        Update();
        m_startDate = value;
      }
    }

    public int PromotionalType
    {
      get
      {
        return m_promotionType;
      }
      set
      {
        Update();
        m_promotionType = value;
      }
    }
    public string Description
    {
      get
      {
        return m_description;
      }
      set
      {
        Update();
        m_description = value;
      }
    }
    public string Code
    {
      get
      {
        return m_code;
      }
      set
      {
        Update();
        m_code = value;
      }
    }
    public string Link
    {
      get
      {
        return m_link;
      }
      set
      {
        Update();
        m_link = value;
      }
    }
    public string Image
    {
      get
      {
        return m_image;
      }
      set
      {
        Update();
        m_image = value;
      }
    }
  }
}
