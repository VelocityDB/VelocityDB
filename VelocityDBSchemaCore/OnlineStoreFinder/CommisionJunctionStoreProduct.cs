using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Session;

namespace VelocityDbSchema.OnlineStoreFinder
{
    public class CommisionJunctionStoreProduct : Product
  {
    enum CjFlags : short { FromPrice = 1, Special = 2, Gift = 4, Offline = 8, Online = 16, InStock = 32};
    string m_programurl;
    string m_catalogname;
    DateTime m_lastupdated;
    double m_upc;
    string m_isbn;
    Decimal m_retailprice;
    string m_impressionurl;
    string m_advertisercategory;
    string m_thirdpartyid;
    string m_thirdpartycategory;
    string m_author;
    string m_artist;
    string m_title;
    string m_publisher;
    string m_label;
    string m_format;
    string m_promotionaltext;
    DateTime? m_startdate;
    string m_condition;
    string m_warranty;
    Int16 m_flags = 0;

    public string Programurl
    {
      get
      {
        return m_programurl;
      }
      set
      {
        Update();
        m_programurl = value;
      }
    }

    public string Catalogname
    {
      get
      {
        return m_catalogname;
      }
      set
      {
        Update();
        m_catalogname = value;
      }
    }

    public DateTime Lastupdated
    {
      get
      {
        return m_lastupdated;
      }
      set
      {
        Update();
        m_lastupdated = value;
      }
    }

    public override UInt16 ObjectsPerPage
    {
      get
      {
        return 1000;
      }
    }

    public override bool AllowOtherTypesOnSamePage
    {
      get
      {
        return false;
      }
    }
    
    public double Upc
    {
      get
      {
        return m_upc;
      }
      set
      {
        Update();
        m_upc = value;
      }
    }

    public string Isbn
    {
      get
      {
        return m_isbn;
      }
      set
      {
        Update();
        m_isbn = value;
      }
    }

    public Decimal Retailprice
    {
      get
      {
        return m_retailprice;
      }
      set
      {
        Update();
        m_retailprice = value;
      }
    }

    public bool Fromprice
    {
      get
      {
        return (m_flags & (short) CjFlags.FromPrice) > 0;
      }
      set
      {
        Update();
        if (value)
          m_flags |= (short)CjFlags.FromPrice;
        else
          m_flags &= ~(short)CjFlags.FromPrice;
      }
    }

    public string Impressionurl
    {
      get
      {
        return m_impressionurl;
      }
      set
      {
        Update();
        m_impressionurl = value;
      }
    }

    public string Advertisercategory
    {
      get
      {
        return m_advertisercategory;
      }
      set
      {
        Update();
        m_advertisercategory = value;
      }
    }

    public string Thirdpartyid
    {
      get
      {
        return m_thirdpartyid;
      }
      set
      {
        Update();
        m_thirdpartyid = value;
      }
    }

    public string Thirdpartycategory
    {
      get
      {
        return m_thirdpartycategory;
      }
      set
      {
        Update();
        m_thirdpartycategory = value;
      }
    }
    
    public string Author
    {
      get
      {
        return m_author;
      }
      set
      {
        Update();
        m_author = value;
      }
    }

    public string Artist
    {
      get
      {
        return m_artist;
      }
      set
      {
        Update();
        m_artist = value;
      }
    }

    public string Title
    {
      get
      {
        return m_title;
      }
      set
      {
        Update();
        m_title = value;
      }
    }

    public string Publisher
    {
      get
      {
        return m_publisher;
      }
      set
      {
        Update();
        m_publisher = value;
      }
    }

    public string Label
    {
      get
      {
        return m_label;
      }
      set
      {
        Update();
        m_label = value;
      }
    }

    public string Format
    {
      get
      {
        return m_format;
      }
      set
      {
        Update();
        m_format = value;
      }
    }

    public bool Special
    {
      get
      {
        return (m_flags & (short)CjFlags.Special) > 0;
      }
      set
      {
        Update();
        if (value)
          m_flags |= (short)CjFlags.Special;
        else
          m_flags &= ~(short)CjFlags.Special;
      }
    }

    public bool Gift
    {
      get
      {
        return (m_flags & (short)CjFlags.Gift) > 0;
      }
      set
      {
        Update();
        if (value)
          m_flags |= (short)CjFlags.Gift;
        else
          m_flags &= ~(short)CjFlags.Gift;
      }
    }


    public string Promotionaltext
    {
      get
      {
        return m_promotionaltext;
      }
      set
      {
        Update();
        m_promotionaltext = value;
      }
    }

    public DateTime? Startdate
    {
      get
      {
        return m_startdate;
      }
      set
      {
        Update();
        m_startdate = value;
      }
    }

    public bool Offline
    {
      get
      {
        return (m_flags & (short)CjFlags.Offline) > 0;
      }
      set
      {
        Update();
        if (value)
          m_flags |= (short)CjFlags.Offline;
        else
          m_flags &= ~(short)CjFlags.Offline;
      }
    }

    public bool Online
    {
      get
      {
        return (m_flags & (short)CjFlags.Online) > 0;
      }
      set
      {
        Update();
        if (value)
          m_flags |= (short)CjFlags.Online;
        else
          m_flags &= ~(short)CjFlags.Online;
      }
    }

    public bool Instock
    {
      get
      {
        return (m_flags & (short)CjFlags.InStock) > 0;
      }
      set
      {
        Update();
        if (value)
          m_flags |= (short)CjFlags.InStock;
        else
          m_flags &= ~(short)CjFlags.InStock;
      }
    }
      
    public string Condition
    {
      get
      {
        return m_condition;
      }
      set
      {
        Update();
        m_condition = value;
      }
    }

    public string Warranty
    {
      get
      {
        return m_warranty;
      }
      set
      {
        Update();
        m_warranty = value;
      }
    }
  }
}
