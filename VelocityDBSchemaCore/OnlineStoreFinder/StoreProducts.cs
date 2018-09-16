using System;
using System.Collections;
using System.IO;
using VelocityDb;
using VelocityDb.Collection;

namespace VelocityDbSchema.OnlineStoreFinder
{
  public class StoreProducts : OptimizedPersistable
  {    
    public string storeName;
    public string fileName;
    public string description;
    public string image;
    public string link;
    public int stars;
    public VelocityDbList<StoreProduct> productList;

    public StoreProducts(string storeStr, string fileName, string description, string link, string image)
    {
      storeName = storeStr;
      this.fileName = fileName;
      this.description = description;
      this.link = link;
      this.image = image;
      productList = new VelocityDbList<StoreProduct>();
    }

    public StoreProducts(string path)
    {
      fileName = path.Substring(0, path.LastIndexOf(".txt"));
      fileName = fileName.Substring(path.LastIndexOf(Path.DirectorySeparatorChar) + 1);
      productList = new VelocityDbList<StoreProduct>();
      StreamReader stream = new StreamReader(path);
      try
      {
        storeName = stream.ReadLine();
        description = stream.ReadLine();
        image = stream.ReadLine();
        link = stream.ReadLine();
        string t = stream.ReadLine();
        stars = Int16.Parse(t);
        StoreProduct c = new StoreProduct(storeName);
        t = stream.ReadLine();
        while (t != null)
        {
          if (t != null && t.Length > 0)
            c.expireDate = DateTime.Parse(t);
          else
            c.expireDate = DateTime.MaxValue;
          c.name = stream.ReadLine();
          c.description = stream.ReadLine();
          c.image = stream.ReadLine();
          c.link = stream.ReadLine();
          productList.Add(c);
          c = new StoreProduct(storeName);
          t = stream.ReadLine();
        }
        stream.Close();
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex);
        stream.Close();
      }
    }

    public bool anyProductsToShow(bool freeShipping, bool dollarOff, bool gift, bool percent, bool showExpired)
    {
      for (int i = 0; i < productList.Count; i++)
      {
        StoreProduct p = (StoreProduct)productList[i];
        bool dateOK = (p.expireDate >= DateTime.Now);
        bool show = dateOK && !showExpired;
        show = show || !dateOK && showExpired;
        if (show)
          return true;
      }
      return false;
    }
  }
}

