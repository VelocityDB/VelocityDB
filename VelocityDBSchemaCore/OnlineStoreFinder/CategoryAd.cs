using System;
using VelocityDb;

namespace VelocityDbSchema.OnlineStoreFinder
{
  public class CategoryAd : OptimizedPersistable
  {
    public const UInt32 PlaceInDatabase = 18;

    public string categoryName;
    public int price;
    public string text;
    public string image;
    public string click;
    public DateTime expireDate;

    public CategoryAd(string categoryName)
    {
      this.categoryName = categoryName;
    }

    public CategoryAd(string categoryName, string price, string text, string image, string click, string expireDate)
    {
      this.categoryName = categoryName;
      int.TryParse(price, out this.price);
      this.text = text;
      this.image = image;
      this.click = click;
      DateTime.TryParse(expireDate, out this.expireDate);
    }

    public override UInt32 PlacementDatabaseNumber
    {
      get
      {
        return PlaceInDatabase;
      }
    }
  }
}
