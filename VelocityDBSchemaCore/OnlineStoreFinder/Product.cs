using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;

namespace VelocityDbSchema.OnlineStoreFinder
{
  public class Product : OptimizedPersistable
  {
    public enum AvailabilityEnum : byte { InStock, OutOfStock, PreOrder, BackOrder }
    AvailabilityEnum _availability;
    string _storeName;
    DateTime? _expireDate;
    string _productName;
    string _shortDescription;
    string _longDescription;
    string _buyUrl;
    string _imageUrl;
    string _currency;
    Decimal _price;
    Decimal? _saleprice;
    Decimal? _discountAmount;
    Decimal? _shippingCost;
    string _sku;
    string _keywords;
    string _manufacturer;
    string _manufacturerId;
    byte _percentDiscount;
    string _brand;
    string _universalProductCode;
    int _category;

    public override bool AllowOtherTypesOnSamePage
    {
      get
      {
        return false;
      }
    }

    public AvailabilityEnum Availability
    {
      get
      {
        return _availability;
      }
      set
      {
        Update();
        _availability = value;
      }
    }

    public string Brand
    {
      get
      {
        return _brand;
      }
      set
      {
        Update();
        _brand = value;
      }
    }

    public string BuyUrl
    {
      get
      {
        return _buyUrl;
      }
      set
      {
        Update();
        _buyUrl = value;
      }
    }

    public string Currency
    {
      get
      {
        return _currency;
      }
      set
      {
        Update();
        _currency = value;
      }
    }

    public Decimal? DiscountAmount
    {
      get
      {
        return _discountAmount;
      }
      set
      {
        Update();
        _discountAmount = value;
      }
    }

    public string ManufacturerId
    {
      get
      {
        return _manufacturerId;
      }
      set
      {
        Update();
        _manufacturerId = value;
      }
    }

    public Decimal? ShippingCost
    {
      get
      {
        return _shippingCost;
      }
      set
      {
        Update();
        _shippingCost = value;
      }
    }

    public byte PercentDiscount
    {
      get
      {
        return _percentDiscount;
      }
      set
      {
        Update();
        _percentDiscount = value;
      }
    }

    public string LongDescription
    {
      get
      {
        return _longDescription;
      }
      set
      {
        Update();
        _longDescription = value;
      }
    }

    public Decimal? Saleprice
    {
      get
      {
        return _saleprice;
      }
      set
      {
        Update();
        _saleprice = value;
      }
    }

    public string ShortDescription
    {
      get
      {
        return _shortDescription;
      }
      set
      {
        Update();
        _shortDescription = value;
      }
    }

    public DateTime? ExpireDate
    {
      get
      {
        return _expireDate;
      }
      set
      {
        Update();
        _expireDate = value;
      }
    }

    public string ImageUrl
    {
      get
      {
        return _imageUrl;
      }
      set
      {
        Update();
        _imageUrl = value;
      }
    }

    public string ImageOrNoneUrl
    {
      get
      {
        if (_imageUrl != null && _imageUrl.Length > 0)
          return _imageUrl;
        else
          return "~/i/transparent.gif";
      }
      set
      {
        Update();
        _imageUrl = value;
      }
    }

    public string Keywords
    {
      get
      {
        return _keywords;
      }
      set
      {
        Update();
        _keywords = value;
      }
    }

    public string Manufacturer
    {
      get
      {
        return _manufacturer;
      }
      set
      {
        Update();
        _manufacturer = value;
      }
    }

    public Decimal Price
    {
      get
      {
        return _price;
      }
      set
      {
        Update();
        _price = value;
      }
    }

    public string ProductName
    {
      get
      {
        return _productName;
      }
      set
      {
        Update();
        _productName = value;
      }
    }

    public string StoreName
    {
      get
      {
        return _storeName;
      }
      set
      {
        Update();
        _storeName = value;
      }
    }

    public override int GetHashCode()
    {
      if (_sku != null)
        return _sku.GetHashCode();
      return _productName.GetHashCode();
    }

    public override int CompareTo(object obj)
    {
      Product otherProduct = obj as Product;
      if (_sku != null)
      {
        if (otherProduct._sku == null)
          return 1;
        return _sku.CompareTo(otherProduct.Sku);
      }
      return _productName.CompareTo(otherProduct.ProductName);
    }

    public string Sku
    {
      get
      {
        return _sku;
      }
      set
      {
        Update();
        _sku = value;
      }
    }

    public string UniversalProductCode
    {
      get
      {
        return _universalProductCode;
      }
      set
      {
        Update();
        _universalProductCode = value;
      }
    }
  }
}
