using System;
using VelocityDb;

namespace VelocityDbSchema.OnlineStoreFinder
{
  public class StoreProduct : OptimizedPersistable
  {
    public string storeName;
    public DateTime expireDate;
    public string name;
    public string description;
    public string link;
    public string image;

    public StoreProduct(string storeName)
    {
      this.storeName = storeName;
    }

    public StoreProduct(string storeStr, string expireDate, string name, string description, string link, string image)
    {
      storeName = storeStr;
      DateTime.TryParse(expireDate, out this.expireDate);
      this.name = name;
      this.description = description;
      this.link = link;
      this.image = image;
    }
  }
}
