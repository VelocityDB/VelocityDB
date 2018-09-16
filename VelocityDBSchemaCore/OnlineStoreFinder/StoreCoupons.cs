using System;
using System.IO;
using VelocityDb;
using VelocityDb.Collection;

namespace VelocityDbSchema.OnlineStoreFinder
{
  public class StoreCoupons : OptimizedPersistable
  {
    public string name;
    public string fileName;
    public string description;
    public string image;
    public string link;
    public int stars;
    public VelocityDbList<Coupon> couponList;

    public StoreCoupons(Store store, string fileName, string description, string image, string link, int stars)
    {
      name = store.Name;
      this.fileName = fileName;
      this.description = description;
      this.image = image;
      this.link = link;
      this.stars = stars;
      couponList = new VelocityDbList<Coupon>();
    }

    public StoreCoupons(string path)
    {
      fileName = path.Substring(0, path.LastIndexOf(".txt"));
      fileName = fileName.Substring(path.LastIndexOf(Path.DirectorySeparatorChar) + 1);
      couponList = new VelocityDbList<Coupon>();
      StreamReader stream = new StreamReader(path);
      try
      {
        name = stream.ReadLine();
        description = stream.ReadLine();
        image = stream.ReadLine();
        link = stream.ReadLine();
        string t = stream.ReadLine();
        stars = Int16.Parse(t);
        Coupon c = new Coupon();
        c.Category = stream.ReadLine();
        while (c.Category != null && !c.Category.Equals("."))
        {
          t = stream.ReadLine();
          if (t != null && t.Length > 0)
          {
            c.StartDate = DateTime.Parse(t);
          }
          else
            c.StartDate = DateTime.MinValue;
          t = stream.ReadLine();     
          if (t != null && t.Length > 0)
          {
            DateTime dt;        
            DateTime.TryParse(t, out dt);
            c.ExpireDate = dt;
          }
          else
            c.ExpireDate = DateTime.MaxValue;
          t = stream.ReadLine();
          if (t != null && t.Length > 0)
            c.PromotionalType = Int16.Parse(t);
          else
            c.PromotionalType = 0;
          c.Description = stream.ReadLine();
          t = stream.ReadLine();
          if (t.CompareTo("inLinkActivated") == 0)
            c.Code = null;
          else
            c.Code = t;
          c.Image = stream.ReadLine();
          c.Link = stream.ReadLine();
          couponList.Add(c);
          c = new Coupon();
          c.Category = stream.ReadLine();
        }
        stream.Close();
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex);
        stream.Close();
      }
    }

    public bool anyCouponsToShow(bool freeShipping, bool dollarOff, bool gift, bool percent, bool showExpired)
    {
      for (int i = 0; i < couponList.Count; i++)
      {
        Coupon coupon = couponList[i];
        bool dateOK = (coupon.ExpireDate >= DateTime.Now) && (DateTime.Now >= coupon.StartDate);
        bool typeOK = (coupon.PromotionalType == 1 && freeShipping) || (coupon.PromotionalType == 2 && dollarOff) ||
          (coupon.PromotionalType == 3 && percent) || (coupon.PromotionalType == 4 && gift) ||
          (!freeShipping && !dollarOff && !gift && !percent);
        bool show = dateOK && typeOK && !showExpired;
        show = show || !dateOK && showExpired;
        if (show)
          return true;
      }
      return false;
    }

    public string StoreName
    {
      get
      {
        return name;
      }
    }
  }
}
