#if !NET35
using System;
using VelocityDb.Session;
using VelocityDb;
using VelocityDb.Collection;
using System.IO;
using VelocityDb.Collection.BTree;
using System.IO.Compression;
using System.Collections.Generic;
using VelocityDb.TypeInfo;
using VelocityDb.Collection.Comparer;
using System.Text.RegularExpressions;
using LumenWorks.Framework.IO.Csv;
using VelocityDb.Exceptions;

namespace VelocityDbSchema.OnlineStoreFinder
{
  public class ShoppingRoot : OptimizedPersistable
  {
    static readonly char[] s_trimEndChars = new char[] { '\"', ';', '.', '"', ',', '\r', ':', ':', ']', '!', '?', '+', '(', ')', '\'', '{', '}', '-' };
    static readonly char[] s_trimStartChars = new char[] { '\"', '&', '-', '#', '*', '[', '.', '"', ',', '\r', ')', '(', '\'', '{', '}', '-', '`' };
    static readonly char[] s_splitChars = new char[] { ' ', '\n', '(', '"', '!', ',', '(', ')', ':', '/', '.', '-', '\'', '?', '&', ';' };
    //static readonly char[] splitChars = new char[] { ' ', '\n', '(', '"', '!', ',', '(', ')', ':', '/', '.', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', '-', '\'', '?', '&', ';'};
    static readonly string[] s_skipWords = new string[] { "an", "and", "are", "at", "be", "do", "from", "in", "is", "it", "null", "of", "or", "that", "the", "this", "to", "with", "you"};
    public SortedMap<string, SortedMap<string, Store>> m_keywordSet;
    public SortedMap<string, Store> m_storeSet;
    public BTreeSet<StoreBase> m_cjLinkShareStoreSet;
    public SortedMap<string, StoreCategory> m_categorySet;
    public SortedSetAny<StoreInCategory> m_inCategorySet;
    public SortedMap<string, StoreCoupons> m_couponSet;
    public SortedMap<string, StoreProducts> m_productSet;
    public VelocityDbList<StoreCategory> m_categoryList;
    public bool m_hideClickUrls;
    public bool m_hideImageUrls;
    public Placement m_cjStorePlacement;
    public SimpleComparer<string> m_stringComparer;
    public HashCodeComparer<Product> m_hashCodeComparerCjProduct;
    public HashCodeComparer<TokenType<string>> m_stringTokenComparer;
    TokenInProductComparer m_tokenInProductComparer;
    public CjStoreHashCodeComparer m_cjStoreHashCodeComparer;
    UInt64 m_invertedIndexId;
    [NonSerialized]
    InvertedIndex m_invertedIndex;
    public static string CjCategoryToCategory(string cjCategory)
    {
      switch (cjCategory)
      {
        case "Accessories":
          return "shoes and accessories";
        case "Air":
          return "airlines";
        case "Apparel":
          return "apparel";
        case "Art":
          return "arts";
        case "Astrology":
          return "miscellaneous";
        case "Auction":
          return "miscellaneous";
        case "Audio Books":
          return "books";
        case "Babies":
          return "baby";
        case "Banking/Trading":
          return "bank";
        case "Bath & Body":
          return "beauty & fragrance";
        case "Books":
          return "books";
        case "Business-to-Business":
          return "business-to-business";
        case "Car":
          return "automotive";
        case "Cars & Trucks":
          return "automotive";
        case "Children":
          return "miscellaneous";
        case "Collectibles":
          return "collectibles";
        case "College":
          return "college";
        case "Communities":
          return "miscellaneous";
        case "Computer HW":
          return "computers";
        case "Computer SW":
          return "software";
        case "Construction":
          return "miscellaneous";
        case "Consumer Electronics":
          return "electronics";
        case "Cosmetics":
          return "beauty & fragrance";
        case "Credit Cards":
          return "credit cards";
        case "Department Stores":
          return "department stores";
        case "Discounts":
          return "miscellaneous";
        case "Domain Registrations":
          return "domain names";
        case "E-commerce Solutions/Providers":
          return "internet";
        case "Email Marketing":
          return "advertising";
        case "Employment":
          return "jobs";
        case "Entertainment":
          return "entertainment";
        case "Equipment":
          return "sports";
        case "Events":
          return "miscellaneous";
        case "Exercise & Health":
          return "health";
        case "Flowers":
          return "flowers";
        case "Fragrance":
          return "beauty & fragrance";
        case "Furniture":
          return "furniture";
        case "Games":
          return "games";
        case "Garden":
          return "garden";
        case "Gifts":
          return "gifts";
        case "Golf":
          return "golf";
        case "Gourmet":
          return "fine food";
        case "Greeting Cards":
          return "greeting cards";
        case "Groceries":
          return "grocery";
        case "Halloween":
          return "miscellaneous";
        case "Handbags":
          return "handbags";
        case "Health Food":
          return "food & cooking";
        case "Home Appliances":
          return "appliances";
        case "Hotel":
          return "hotel";
        case "Internet Service Providers":
          return "internet";
        case "Investment":
          return "investment";
        case "Jewelry":
          return "jewelry";
        case "Kitchen":
          return "kitchen";
        case "Languages":
          return "education and training";
        case "Luggage":
          return "luggage";
        case "Magazines":
          return "magazines";
        case "Malls":
          return "virtual malls";
        case "Marketing":
          return "advertising";
        case "Matchmaking":
          return "matchmaking";
        case "Memorabilia":
          return "gifts";
        case "Men's":
          return "mens";
        case "Mortgage Loans":
          return "mortgage";
        case "Motorcycles":
          return "motorcycles";
        case "Music":
          return "music";
        case "News":
          return "news";
        case "Nutritional Supplements":
          return "nutritional supplements";
        case "Office":
          return "office supplies";
        case "Online/Wireless":
          return "wireless";
        case "Outdoors":
          return "outdoors";
        case "Parts & Accessories":
          return "car parts & accessories";
        case "Party Goods":
          return "party goods";
        case "Personal Insurance":
          return "insurance";
        case "Personal Loans":
          return "loans";
        case "Pets":
          return "pets";
        case "Photo":
          return "photography";
        case "Productivity Tools":
          return "internet";
        case "Real Estate":
          return "real estate";
        case "Real Estate Services":
          return "real estate";
        case "Search Engine":
          return "advertising";
        case "Self Help":
          return "miscellaneous";
        case "Services":
          return "services";
        case "Shoes":
          return "shoes and accessories";
        case "Sports":
          return "sports";
        case "Tax Services":
          return "services";
        case "Telephone Services":
          return "communication";
        case "Television":
          return "television";
        case "Toys":
          return "toys";
        case "Vacation":
          return "vacation";
        case "Videos/Movies":
          return "movies";
        case "Virtual Malls":
          return "virtual malls";
        case "Vision Care":
          return "eyewear";
        case "Water Sports":
          return "sports";
        case "Web Design":
          return "web design";
        case "Web Hosting/Servers":
          return "web hosting";
        case "Web Tools":
          return "web tools";
        case "Weddings":
          return "weddings";
        case "Weight Loss":
          return "weight loss";
        case "Wellness":
          return "health";
        case "Wine & Spirits":
          return "wine";
        case "Women's":
          return "womens";
      };
      return "miscellaneous";
    }

    public static string GoogleCategoryToCategory(string googleCategory)
    {
      switch (googleCategory)
      {
        case "Apparel & Accessories":
          return "apparel";
        case "Air":
          return "airlines";

        case "Apparel":
          return "apparel";
        case "Appliances & Electronics":
          return "electronics";
        case "Art":
          return "arts";
        case "Art/Music/Photography":
          return "arts";
        case "Astrology":
          return "miscellaneous";
        case "Automotive":
          return "automotive";
        case "Auction":
          return "miscellaneous";
        case "Audio Books":
          return "books";
        case "Babies":
          return "baby";
        case "Banking/Trading":
          return "bank";
        case "Bath & Body":
          return "beauty & fragrance";
        case "Books":
          return "books";
        case "Books & Magazines":
          return "books";
        case "Books/Media":
          return "books";
        case "Business":
          return "business-to-business";
        case "Business-to-Business":
          return "business-to-business";
        case "Car":
          return "automotive";
        case "Career/Jobs/Employment":
          return "jobs";
        case "Cars & Trucks":
          return "automotive";
        case "Children":
          return "miscellaneous";
        case "Clothing":
          return "apparel";
        case "Collectibles":
          return "collectibles";
        case "College":
          return "college";
        case "Communities":
          return "miscellaneous";
        case "Computer HW":
          return "computers";
        case "Computer SW":
          return "software";
        case "Computers/Electronics":
          return "electronics";
        case "Construction":
          return "miscellaneous";
        case "Consumer Electronics":
          return "electronics";
        case "Cosmetics":
          return "beauty & fragrance";
        case "Credit Cards":
          return "credit cards";
        case "Department Stores":
          return "department stores";
        case "Discounts":
          return "miscellaneous";
        case "Domain Registrations":
          return "domain names";
        case "E-commerce Solutions/Providers":
          return "internet";
        case "Email Marketing":
          return "advertising";
        case "Employment":
          return "jobs";
        case "Entertainment":
          return "entertainment";
        case "Equipment":
          return "sports";
        case "Events":
          return "miscellaneous";
        case "Exercise & Health":
          return "health";
        case "Family":
          return "miscellaneous";
        case "Flowers":
          return "flowers";
        case "Flowers & Gifts":
          return "flowers";
        case "Food/Drink":
          return "food & cooking";
        case "Fragrance":
          return "beauty & fragrance";
        case "Furniture":
          return "furniture";
        case "Games":
          return "games";
        case "Games/Toys":
          return "games";
        case "Garden":
          return "garden";
        case "General Web Services":
          return "services";
        case "Gifts":
          return "gifts";
        case "Golf":
          return "golf";
        case "Gourmet":
          return "fine food";
        case "Greeting Cards":
          return "greeting cards";
        case "Groceries":
          return "grocery";
        case "Halloween":
          return "miscellaneous";
        case "Handbags":
          return "handbags";
        case "Health":
          return "health";
        case "Health Food":
          return "food & cooking";
        case "Home Appliances":
          return "appliances";
        case "Home & Garden":
          return "home & garden";
        case "Hotel":
          return "hotel";
        case "Internet Service Providers":
          return "internet";
        case "Investment":
          return "investment";
        case "Jewelry":
          return "jewelry";
        case "Kitchen":
          return "kitchen";
        case "Languages":
          return "education and training";
        case "Luggage":
          return "luggage";
        case "Magazines":
          return "magazines";
        case "Malls":
          return "virtual malls";
        case "Marketing":
          return "advertising";
        case "Matchmaking":
          return "matchmaking";
        case "Memorabilia":
          return "gifts";
        case "Men's":
          return "mens";
        case "Moving/Moving Supplies":
          return "movers";
        case "Mortgage Loans":
          return "mortgage";
        case "Motorcycles":
          return "motorcycles";
        case "Music":
          return "music";
        case "News":
          return "news";
        case "NONE":
          return "miscellaneous";
        case "Nutritional Supplements":
          return "nutritional supplements";
        case "Office":
          return "office supplies";
        case "Office Supplies":
          return "office supplies";
        case "Online Dating Services":
          return "matchmaking";
        case "Online/Wireless":
          return "wireless";
        case "Outdoor":
          return "outdoors";
        case "Outdoors":
          return "outdoors";
        case "Parts & Accessories":
          return "car parts & accessories";
        case "Party Goods":
          return "party goods";
        case "Personal Insurance":
          return "insurance";
        case "Personal Loans":
          return "loans";
        case "Pets":
          return "pets";
        case "Photo":
          return "photography";
        case "Productivity Tools":
          return "internet";
        case "Real Estate":
          return "real estate";
        case "Real Estate Services":
          return "real estate";
        case "Recreation":
          return "outdoors";
        case "Search Engine":
          return "advertising";
        case "Self Help":
          return "miscellaneous";
        case "Search Engine Submission":
          return "advertising";
        case "Services":
          return "services";
        case "Shoes":
          return "shoes and accessories";
        case "Sports":
          return "sports";
        case "Sports/Fitness":
          return "sports";
        case "Sports & Fitness":
          return "sports";
        case "Tax Services":
          return "services";
        case "Telephone Services":
          return "communication";
        case "Television":
          return "television";
        case "Toys":
          return "toys";
        case "Travel":
          return "travel";
        case "Vacation":
          return "vacation";
        case "Videos/Movies":
          return "movies";
        case "Virtual Malls":
          return "virtual malls";
        case "Vision Care":
          return "eyewear";
        case "Water Sports":
          return "sports";
        case "Web Design":
          return "web design";
        case "Web Hosting":
          return "web hosting";
        case "Web Hosting/Servers":
          return "web hosting";
        case "Webmaster Tools":
          return "web tools";
        case "Web Tools":
          return "web tools";
        case "Weddings":
          return "weddings";
        case "Weight Loss":
          return "weight loss";
        case "Wellness":
          return "health";
        case "Wine & Spirits":
          return "wine";
        case "Women's":
          return "womens";
      };
      return "miscellaneous";
    }

    public static string ShareasaleCategoryToCategory(string shareasaleCategory)
    {
      switch (shareasaleCategory)
      {
        case "Accessories":
          return "shoes and accessories";
        case "Air":
          return "airlines";
        case "Apparel":
          return "apparel";
        case "Art":
          return "arts";
        case "Art/Music/Photography":
          return "arts";
        case "Astrology":
          return "miscellaneous";
        case "Automotive":
          return "automotive";
        case "Auction":
          return "miscellaneous";
        case "Audio Books":
          return "books";
        case "Babies":
          return "baby";
        case "Banking/Trading":
          return "bank";
        case "Bath & Body":
          return "beauty & fragrance";
        case "Books":
          return "books";
        case "Books/Media":
          return "books";
        case "Business":
          return "business-to-business";
        case "Business-to-Business":
          return "business-to-business";
        case "Car":
          return "automotive";
        case "Career/Jobs/Employment":
          return "jobs";
        case "Cars & Trucks":
          return "automotive";
        case "Children":
          return "miscellaneous";
        case "Clothing":
          return "apparel";
        case "Collectibles":
          return "collectibles";
        case "College":
          return "college";
        case "Communities":
          return "miscellaneous";
        case "Computer HW":
          return "computers";
        case "Computer SW":
          return "software";
        case "Computers/Electronics":
          return "electronics";
        case "Construction":
          return "miscellaneous";
        case "Consumer Electronics":
          return "electronics";
        case "Cosmetics":
          return "beauty & fragrance";
        case "Credit Cards":
          return "credit cards";
        case "Department Stores":
          return "department stores";
        case "Discounts":
          return "miscellaneous";
        case "Domain Registrations":
          return "domain names";
        case "E-commerce Solutions/Providers":
          return "internet";
        case "Email Marketing":
          return "advertising";
        case "Employment":
          return "jobs";
        case "Entertainment":
          return "entertainment";
        case "Equipment":
          return "sports";
        case "Events":
          return "miscellaneous";
        case "Exercise & Health":
          return "health";
        case "Family":
          return "miscellaneous";
        case "Flowers":
          return "flowers";
        case "Food/Drink":
          return "food & cooking";
        case "Fragrance":
          return "beauty & fragrance";
        case "Furniture":
          return "furniture";
        case "Games":
          return "games";
        case "Games/Toys":
          return "games";
        case "Garden":
          return "garden";
        case "General Web Services":
          return "services";
        case "Gifts":
          return "gifts";
        case "Golf":
          return "golf";
        case "Gourmet":
          return "fine food";
        case "Greeting Cards":
          return "greeting cards";
        case "Groceries":
          return "grocery";
        case "Halloween":
          return "miscellaneous";
        case "Handbags":
          return "handbags";
        case "Health":
          return "health";
        case "Health Food":
          return "food & cooking";
        case "Home Appliances":
          return "appliances";
        case "Home & Garden":
          return "home & garden";
        case "Hotel":
          return "hotel";
        case "Internet Service Providers":
          return "internet";
        case "Investment":
          return "investment";
        case "Jewelry":
          return "jewelry";
        case "Kitchen":
          return "kitchen";
        case "Languages":
          return "education and training";
        case "Luggage":
          return "luggage";
        case "Magazines":
          return "magazines";
        case "Malls":
          return "virtual malls";
        case "Marketing":
          return "advertising";
        case "Matchmaking":
          return "matchmaking";
        case "Memorabilia":
          return "gifts";
        case "Men's":
          return "mens";
        case "Moving/Moving Supplies":
          return "movers";
        case "Mortgage Loans":
          return "mortgage";
        case "Motorcycles":
          return "motorcycles";
        case "Music":
          return "music";
        case "News":
          return "news";
        case "NONE":
          return "miscellaneous";
        case "Nutritional Supplements":
          return "nutritional supplements";
        case "Office":
          return "office supplies";
        case "Online Dating Services":
          return "matchmaking";
        case "Online/Wireless":
          return "wireless";
        case "Outdoors":
          return "outdoors";
        case "Parts & Accessories":
          return "car parts & accessories";
        case "Party Goods":
          return "party goods";
        case "Personal Insurance":
          return "insurance";
        case "Personal Loans":
          return "loans";
        case "Pets":
          return "pets";
        case "Photo":
          return "photography";
        case "Productivity Tools":
          return "internet";
        case "Real Estate":
          return "real estate";
        case "Real Estate Services":
          return "real estate";
        case "Recreation":
          return "outdoors";
        case "Search Engine":
          return "advertising";
        case "Self Help":
          return "miscellaneous";
        case "Search Engine Submission":
          return "advertising";
        case "Services":
          return "services";
        case "Shoes":
          return "shoes and accessories";
        case "Sports":
          return "sports";
        case "Sports/Fitness":
          return "sports";
        case "Tax Services":
          return "services";
        case "Telephone Services":
          return "communication";
        case "Television":
          return "television";
        case "Toys":
          return "toys";
        case "Travel":
          return "travel";
        case "Vacation":
          return "vacation";
        case "Videos/Movies":
          return "movies";
        case "Virtual Malls":
          return "virtual malls";
        case "Vision Care":
          return "eyewear";
        case "Water Sports":
          return "sports";
        case "Web Design":
          return "web design";
        case "Web Hosting":
          return "web hosting";
        case "Web Hosting/Servers":
          return "web hosting";
        case "Webmaster Tools":
          return "web tools";
        case "Web Tools":
          return "web tools";
        case "Weddings":
          return "weddings";
        case "Weight Loss":
          return "weight loss";
        case "Wellness":
          return "health";
        case "Wine & Spirits":
          return "wine";
        case "Women's":
          return "womens";
      };
      return "miscellaneous";
    }

    public enum LinkShareCategory
    {
      Hobbies = 1, Auto = 2, Clothing = 3, Computer = 4, Entertainment = 5, FinancialServices = 6, FoodDrink = 7, GamesToys = 8,
      GiftFlowers = 9, HealthBeauty = 10, HomeLiving = 11, Mature = 12, Office = 13, Sports = 14, Travel = 15, Internet = 16, Business = 17, DepartmentStore = 18,
      Family = 19, Miscellaneous = 20, Telecommunications = 21, Accessories = 107, Men = 108, Women = 109, Hardware = 110, Consumer = 111, Software = 112, Books = 113,
      Music = 114, Videos = 115, Banking = 116, CreditCards = 117, Loans = 118, Cigars = 119, Gourmet = 120, Wine = 121, Children = 122, Educational = 123,
      Electronic = 124, Gifts = 125, Flowers = 126, GreetingCards = 127, BathBody = 128, Cosmetics = 129, Vitamins = 130, Bed = 131, Garden = 132, Kitchen = 133,
      Apparel = 134, Books2 = 135, Entertainment2 = 136, Equipment = 137, HomeOffice = 138, Supplies = 139, Clothing2 = 140, Collectibles = 141, Equipment2 = 142,
      Airline = 143, Car = 144, Hotel = 145, Services = 146, Development = 147, Hosting = 148, Programs = 149, BtoB = 150, Employment = 151, RealEstate = 152,
      Clothing3 = 153, Gifts2 = 154, Home = 155, Baby = 156, Education = 157, Entertainment3 = 158, Pets = 159, Children2 = 207, Other = 210, Equipment3 = 211,
      LongDistance = 212, Wireless = 213, Improvement = 232, Jewelry = 247, MedicalSupplies = 11248, OnlineDating = 16249
    };

    public static string LinkShareCategoryToCategory(LinkShareCategory cat)
    {
      switch (cat)
      {
        case LinkShareCategory.Hobbies: // Hobbies & Collectibles
          return "outdoors";
        case LinkShareCategory.Auto: // Auto
          return "automotive";
        case LinkShareCategory.Clothing: // Clothing & Accessories
          return "apparel";
        case LinkShareCategory.Computer: // Computer & Electronics
          return "electronics";
        case LinkShareCategory.Entertainment: // Entertainment
          return "entertainment";
        case LinkShareCategory.FinancialServices: // Financial Services
          return "financial services";
        case LinkShareCategory.FoodDrink: // Food & Drink
          return "food & cooking";
        case LinkShareCategory.GamesToys: // Games & Toys
          return "games";
        case LinkShareCategory.GiftFlowers: // Gift & Flowers
          return "gifts";
        case LinkShareCategory.HealthBeauty: // Health & Beauty
          return "health";
        case LinkShareCategory.HomeLiving: // Home & Living
          return "home";
        case LinkShareCategory.Mature: // Mature/Adult
          return "miscellaneous";
        case LinkShareCategory.Office: // Office
          return "office supplies";
        case LinkShareCategory.Sports: // Sports & Fitness
          return "sports";
        case LinkShareCategory.Travel: // Travel
          return "travel";
        case LinkShareCategory.Internet: // Internet & Online
          return "internet";
        case LinkShareCategory.Business: // Business & Career
          return "jobs";
        case LinkShareCategory.DepartmentStore: // Department Store
          return "department stores";
        case LinkShareCategory.Family: // Family
          return "communities";
        case LinkShareCategory.Miscellaneous: // Miscellaneous
          return "miscellaneous";
        case LinkShareCategory.Telecommunications: // Telecommunications
          return "communication";
        case LinkShareCategory.Accessories: // Accessories
          return "shoes and accessories";
        case LinkShareCategory.Men: // Men
          return "mens";
        case LinkShareCategory.Women: // Women
          return "womens";
        case LinkShareCategory.Hardware: // Hardware
          return "hardware and tool";
        case LinkShareCategory.Consumer: // Consumer
          return "miscellaneous";
        case LinkShareCategory.Software: // Software
          return "software";
        case LinkShareCategory.Books: // Books/Magazines
          return "books";
        case LinkShareCategory.Music: // Music
          return "music";
        case LinkShareCategory.Videos: // Videos
          return "movies";
        case LinkShareCategory.Banking: // Banking/Trading
          return "money";
        case LinkShareCategory.CreditCards: // Credit Cards
          return "credit cards";
        case LinkShareCategory.Loans: // Loans
          return "loans";
        case LinkShareCategory.Cigars: // Cigars
          return "miscellaneous";
        case LinkShareCategory.Gourmet: // Gourmet
          return "fine food";
        case LinkShareCategory.Wine: // Wine
          return "wine";
        case LinkShareCategory.Children: // Children
          return "toys";
        case LinkShareCategory.Educational: // Educational
          return "education and training";
        case LinkShareCategory.Electronic: // Electronic
          return "electronics";
        case LinkShareCategory.Gifts: // Gifts
          return "gifts";
        case LinkShareCategory.Flowers: // Flowers
          return "flowers";
        case LinkShareCategory.GreetingCards: // Greeting Cards
          return "greeting cards";
        case LinkShareCategory.BathBody: // Bath/Body
          return "health";
        case LinkShareCategory.Cosmetics: // Cosmetics
          return "beauty & fragrance";
        case LinkShareCategory.Vitamins: // Vitamins
          return "vitamins";
        case LinkShareCategory.Bed: // Bed/Bath
          return "bed & bath";
        case LinkShareCategory.Garden: // Garden
          return "garden";
        case LinkShareCategory.Kitchen: // Kitchen
          return "kitchen";
        case LinkShareCategory.Apparel: // Apparel
          return "apparel";
        case LinkShareCategory.Books2: // Books
          return "books";
        case LinkShareCategory.Entertainment2: // Entertainment
          return "entertainment";
        case LinkShareCategory.Equipment: // Equipment
          return "hardware and tool";
        case LinkShareCategory.HomeOffice: // Home Office
          return "office supplies";
        case LinkShareCategory.Supplies: // Supplies
          return "office supplies";
        case LinkShareCategory.Clothing2: // Clothing
          return "apparel";
        case LinkShareCategory.Collectibles: // Collectibles
          return "collectibles";
        case LinkShareCategory.Equipment2: // Equipment
          return "hardware and tool";
        case LinkShareCategory.Airline: // Airline
          return "airlines";
        case LinkShareCategory.Car: // Car
          return "automotive";
        case LinkShareCategory.Hotel: // Hotel
          return "hotel";
        case LinkShareCategory.Services: // Services
          return "services";
        case LinkShareCategory.Development: // Development
          return "miscellaneous";
        case LinkShareCategory.Hosting: // Hosting
          return "web hosting";
        case LinkShareCategory.Programs: // Programs
          return "software";
        case LinkShareCategory.BtoB: // B-to-B
          return "business-to-business";
        case LinkShareCategory.Employment: // Employment
          return "jobs";
        case LinkShareCategory.RealEstate: // Real Estate
          return "real estate";
        case LinkShareCategory.Clothing3: // Clothing
          return "apparel";
        case LinkShareCategory.Gifts2: // Gifts
          return "gifts";
        case LinkShareCategory.Home: // Home
          return "home";
        case LinkShareCategory.Baby: // Baby
          return "baby";
        case LinkShareCategory.Education: // Education
          return "education and training";
        case LinkShareCategory.Entertainment3: // Entertainment
          return "entertainment";
        case LinkShareCategory.Pets: // Pets
          return "pets";
        case LinkShareCategory.Children2: // Children
          return "toys";
        case LinkShareCategory.Other: // Other, Other Products/Services
          return "miscellaneous";
        case LinkShareCategory.Equipment3: // Equipment
          return "hardware and tool";
        case LinkShareCategory.LongDistance: // Long Distance
          return "long distance service";
        case LinkShareCategory.Wireless: // Wireless
          return "wireless";
        case LinkShareCategory.Improvement: // Improvement
          return "miscellaneous";
        case LinkShareCategory.Jewelry: // Jewelry
          return "jewelry";
        case LinkShareCategory.MedicalSupplies: // Medical Supplies & Services
          return "health";
        case LinkShareCategory.OnlineDating: // Online Dating
          return "matchmaking";
      };
      return "miscellaneous";
    }

    public InvertedIndex InvertedIndex => m_invertedIndex == null ? (m_invertedIndex = Session.Open<InvertedIndex>(m_invertedIndexId)) :  m_invertedIndex;
    public ShoppingRoot(SessionBase session)
    {
      m_stringComparer = new SimpleComparer<string>();
      m_cjStoreHashCodeComparer = new CjStoreHashCodeComparer();
      m_tokenInProductComparer = new TokenInProductComparer();
      m_hashCodeComparerCjProduct = new HashCodeComparer<Product>();
      m_stringTokenComparer = new HashCodeComparer<TokenType<string>>();
      m_keywordSet = new SortedMap<string, SortedMap<string, Store>>();
      m_storeSet = new SortedMap<string, Store>();
      m_cjLinkShareStoreSet = new BTreeSet<StoreBase>(m_cjStoreHashCodeComparer, session, 5000, sizeof(int));
      m_categorySet = new SortedMap<string, StoreCategory>();
      m_inCategorySet = new SortedSetAny<StoreInCategory>();
      m_couponSet = new SortedMap<string, StoreCoupons>();
      m_productSet = new SortedMap<string, StoreProducts>();
      m_categoryList = new VelocityDbList<StoreCategory>();
      var invertedIndex = new InvertedIndex(session, m_stringTokenComparer);
      session.Persist(invertedIndex);
      m_invertedIndexId = invertedIndex.Id;
      m_hideClickUrls = false;
      m_hideImageUrls = false;
    }

    public void ExportSubCategoryTree(StoreCategory subStoreCategory, System.IO.StreamWriter categoryFile, System.IO.StreamWriter categoryAdFile)
    {
      categoryFile.Write("{0}\t", subStoreCategory.Name);
      categoryFile.Write("{0}\t", subStoreCategory.Url);
      categoryFile.Write("{0}\t", subStoreCategory.Parent != null ? subStoreCategory.Parent.Name : "");
      categoryFile.WriteLine("{0}\t", subStoreCategory.Level);
      foreach (CategoryAd ad in subStoreCategory.adList)
      {
        categoryAdFile.Write("{0}\t", ad.categoryName);
        categoryAdFile.Write("{0}\t", ad.price);
        categoryAdFile.Write("{0}\t", ad.text);
        categoryAdFile.Write("{0}\t", ad.image);
        categoryAdFile.Write("{0}\t", ad.click);
        categoryAdFile.WriteLine("{0}\t", ad.expireDate);
      }
      foreach (StoreCategory subSubStoreCategory in subStoreCategory.categoryList)
        ExportSubCategoryTree(subSubStoreCategory, categoryFile, categoryAdFile);
    }

    public void ExportToTextFile(string basePath, bool storesOnly, bool categoriesOnly, string suffix)
    {
      long ticks = DateTime.Now.Ticks;
      if (suffix == null)
        suffix = ticks.ToString();
      if (storesOnly == false)
      {
        string categoryPath = System.IO.Path.Combine(basePath, "backup", "Category" + suffix + ".txt");
        string categoryAdPath = System.IO.Path.Combine(basePath, "backup", "CategoryAd" + suffix + ".txt");
        using (System.IO.StreamWriter categoryFile = new System.IO.StreamWriter(categoryPath),
               categoryAdFile = new System.IO.StreamWriter(categoryAdPath))
        {
          categoryFile.Write("{0}\t", "CategoryName");
          categoryFile.Write("{0}\t", "Url");
          categoryFile.Write("{0}\t", "Parent");
          categoryFile.WriteLine("{0}\t", "Level");
          categoryAdFile.Write("{0}\t", "CategoryName");
          categoryAdFile.Write("{0}\t", "Price");
          categoryAdFile.Write("{0}\t", "Text");
          categoryAdFile.Write("{0}\t", "Image");
          categoryAdFile.Write("{0}\t", "Click");
          categoryAdFile.WriteLine("{0}\t", "ExpireDate");
          foreach (StoreCategory storeCategory in m_categoryList)
          {
            if (storeCategory.Level > 0)
              continue;
            categoryFile.Write("{0}\t", storeCategory.Name);
            categoryFile.Write("{0}\t", storeCategory.Url);
            categoryFile.Write("{0}\t", storeCategory.Parent != null ? storeCategory.Parent.Name : "");
            categoryFile.WriteLine("{0}\t", storeCategory.Level);
            foreach (CategoryAd ad in storeCategory.adList)
            {
              categoryAdFile.Write("{0}\t", ad.categoryName);
              categoryAdFile.Write("{0}\t", ad.price);
              categoryAdFile.Write("{0}\t", ad.text);
              categoryAdFile.Write("{0}\t", ad.image);
              categoryAdFile.Write("{0}\t", ad.click);
              categoryAdFile.WriteLine("{0}\t", ad.expireDate);
            }
            foreach (StoreCategory subStoreCategory in storeCategory.categoryList)
              ExportSubCategoryTree(subStoreCategory, categoryFile, categoryAdFile);
          }
        }
      }
      if (categoriesOnly == false)
      {
        string storePath = System.IO.Path.Combine(basePath, "backup", "Store" + suffix + ".txt");
        string storeInCategoryPath = System.IO.Path.Combine(basePath, "backup", "StoreInCategory" + suffix + ".txt");
        using (System.IO.StreamWriter storeFile = new System.IO.StreamWriter(storePath),
               storeCategoryFile = new System.IO.StreamWriter(storeInCategoryPath))
        {
          storeFile.Write("{0}\t", "StoreName");
          storeFile.Write("{0}\t", "Description");
          storeFile.Write("{0}\t", "KeyWords");
          storeFile.Write("{0}\t", "Rating");
          storeFile.Write("{0}\t", "CreateTime");
          storeFile.Write("{0}\t", "ModifyTime");
          storeFile.WriteLine("{0}\t", "IsValid");
          if (storesOnly == false)
          {
            storeCategoryFile.Write("{0}\t", "StoreName");
            storeCategoryFile.Write("{0}\t", "Category");
            storeCategoryFile.Write("{0}\t", "Click");
            storeCategoryFile.Write("{0}\t", "Image");
            storeCategoryFile.Write("{0}\t", "SpecialImage");
            storeCategoryFile.Write("{0}\t", "Script");
            storeCategoryFile.Write("{0}\t", "TextLink");
            storeCategoryFile.Write("{0}\t", "TextClick");
            storeCategoryFile.Write("{0}\t", "ControlUrl");
            storeCategoryFile.Write("{0}\t", "DateTimeCreated");
            storeCategoryFile.WriteLine("{0}\t", "DateTimeUpdated");
          }
          foreach (Store store in m_storeSet.Values)
          {
            storeFile.Write("{0}\t", store.Name);
            storeFile.Write("{0}\t", store.Description);
            storeFile.Write("{0}\t", store.KeyWords);
            storeFile.Write("{0}\t", store.Rating);
            storeFile.Write("{0}\t", store.m_dateTimeCreated);
            storeFile.Write("{0}\t", store.m_modifyDate);
            storeFile.WriteLine("{0}\t", store.IsValid);
            if (storesOnly == false)
            {
              foreach (StoreInCategory inCategory in store.m_categoryList)
              {
                storeCategoryFile.Write("{0}\t", store.Name);
                storeCategoryFile.Write("{0}\t", inCategory.Category.Name);
                storeCategoryFile.Write("{0}\t", inCategory.click);
                storeCategoryFile.Write("{0}\t", inCategory.image);
                storeCategoryFile.Write("{0}\t", inCategory.specialImage);
                storeCategoryFile.Write("{0}\t", inCategory.script);
                storeCategoryFile.Write("{0}\t", inCategory.textLink);
                storeCategoryFile.Write("{0}\t", inCategory.textClick);
                storeCategoryFile.Write("{0}\t", inCategory.controlUrl);
                storeCategoryFile.Write("{0}\t", inCategory.DateTimeCreated);
                storeCategoryFile.WriteLine("{0}\t", inCategory.DateTimeUpdated);
              }
            }
          }
        }
        if (storesOnly == false)
        {
          string storeCouponPath = System.IO.Path.Combine(basePath, "backup", "StoreCoupon" + suffix + ".txt");
          string couponPath = System.IO.Path.Combine(basePath, "backup", "Coupon" + suffix + ".txt");
          using (System.IO.StreamWriter storeCouponFile = new System.IO.StreamWriter(storeCouponPath))
          {
            using (System.IO.StreamWriter couponFile = new System.IO.StreamWriter(couponPath))
            {
              storeCouponFile.Write("{0}\t", "Name");
              storeCouponFile.Write("{0}\t", "FileName");
              storeCouponFile.Write("{0}\t", "Description");
              storeCouponFile.Write("{0}\t", "Image");
              storeCouponFile.Write("{0}\t", "Link");
              storeCouponFile.WriteLine("{0}\t", "Stars");
              couponFile.Write("{0}\t", "Category");
              couponFile.Write("{0}\t", "StartDate");
              couponFile.Write("{0}\t", "ExpireDate");
              couponFile.Write("{0}\t", "PromotionType");
              couponFile.Write("{0}\t", "Description");
              couponFile.Write("{0}\t", "Code");
              couponFile.Write("{0}\t", "Link");
              couponFile.WriteLine("{0}\t", "Image");
              foreach (StoreCoupons storeCoupons in m_couponSet.Values)
              {
                storeCouponFile.Write("{0}\t", storeCoupons.name);
                storeCouponFile.Write("{0}\t", storeCoupons.fileName);
                storeCouponFile.Write("{0}\t", storeCoupons.description);
                storeCouponFile.Write("{0}\t", storeCoupons.image);
                storeCouponFile.Write("{0}\t", storeCoupons.link);
                storeCouponFile.WriteLine("{0}\t", storeCoupons.stars);
                foreach (Coupon coupon in storeCoupons.couponList)
                {
                  couponFile.Write("{0}\t", storeCoupons.name);
                  couponFile.Write("{0}\t", coupon.StartDate);
                  couponFile.Write("{0}\t", coupon.ExpireDate);
                  couponFile.Write("{0}\t", coupon.PromotionalType);
                  couponFile.Write("{0}\t", coupon.Description);
                  couponFile.Write("{0}\t", coupon.Code);
                  couponFile.Write("{0}\t", coupon.Link);
                  couponFile.WriteLine("{0}\t", coupon.Image);
                }
              }
            }
          }
          string storeProductsPath = System.IO.Path.Combine(basePath, "backup", "StoreProducts" + suffix + ".txt");
          string productPath = System.IO.Path.Combine(basePath, "backup", "Product" + suffix + ".txt");
          using (System.IO.StreamWriter storeProductsFile = new System.IO.StreamWriter(storeProductsPath))
          {
            using (System.IO.StreamWriter productFile = new System.IO.StreamWriter(productPath))
            {
              storeProductsFile.Write("{0}\t", "StoreName");
              storeProductsFile.Write("{0}\t", "FileName");
              storeProductsFile.Write("{0}\t", "Description");
              storeProductsFile.Write("{0}\t", "Link");
              storeProductsFile.WriteLine("{0}\t", "Image");
              productFile.Write("{0}\t", "StoreName");
              productFile.Write("{0}\t", "ExpireDate");
              productFile.Write("{0}\t", "Name");
              productFile.Write("{0}\t", "Description");
              productFile.Write("{0}\t", "Link");
              productFile.WriteLine("{0}\t", "Image");
              foreach (StoreProducts storeProducts in m_productSet.Values)
              {
                storeProductsFile.Write("{0}\t", storeProducts.storeName);
                storeProductsFile.Write("{0}\t", storeProducts.fileName);
                storeProductsFile.Write("{0}\t", storeProducts.description);
                storeProductsFile.Write("{0}\t", storeProducts.link);
                storeProductsFile.WriteLine("{0}\t", storeProducts.image);
                foreach (StoreProduct product in storeProducts.productList)
                {
                  productFile.Write("{0}\t", product.storeName);
                  productFile.Write("{0}\t", product.expireDate);
                  productFile.Write("{0}\t", product.name);
                  productFile.Write("{0}\t", product.description);
                  productFile.Write("{0}\t", product.link);
                  productFile.WriteLine("{0}\t", product.image);
                }
              }
            }
          }
        }
      }
    }

    public int ImportStoreProductsFromTextFile(Stream fileStream)
    {
      int numberOfImports = 0;
      using (System.IO.StreamReader file = new System.IO.StreamReader(fileStream))
      {
        string line;
        int lineNumber = 0;
        while ((line = file.ReadLine()) != null)
        {
          lineNumber++;
          char[] delimiters = new char[] { '\t' };
          string[] parts = line.Split(delimiters);
          int i = 0;
          string storeStr = parts[i++];
          string fileName = parts[i++];
          string description = parts[i++];
          string link = parts[i++];
          string image = parts[i++];
          if (lineNumber > 1)
          {
            Store store;
            if (!this.m_storeSet.TryGetValue(storeStr, out store))
              continue;
            StoreProducts storeProducts = new StoreProducts(storeStr, fileName, description, link, image);
            if (this.m_productSet.Contains(storeStr) == false)
            {
              numberOfImports++;
              this.m_productSet.Add(storeStr, storeProducts);
            }
          }
        }
      }
      return numberOfImports;
    }

#if false
    public void AddCjStringFieldToInvertedIndex(UInt16 fieldPosition, string word, Product cjProduct, SessionBase session, ref StoreBase cjStore)
    {
      if (cjStore.StoreName != cjProduct.StoreName)
      {
        if (!cjGoogleStoreSet.TryGetKey(cjStore, ref cjStore))
        {
          cjStore.Persist(cjStorePlacement, session);
          cjStore.Page.Database.Name = cjStore.StoreName;
          cjGoogleStoreSet.Add(cjStore);
        }
      }
      TokenStoreHit tokenStoreHit;
      Token<string> stringToken = new Token<string>(word);
      //TokenInProduct tokenInProduct = new TokenInProduct(cjProduct, session);
      bool existingLexiconWord;
      lock (invertedIndex)
      {
        existingLexiconWord = invertedIndex.stringLexicon.tokenSet.TryGetKey(stringToken, ref stringToken);
      }
      if (existingLexiconWord)
      {
        if (cjStore.TokenStoreHit.TryGetValue(stringToken, out tokenStoreHit))
        {
          stringToken.GlobalCount = stringToken.GlobalCount + 1;
          if (tokenStoreHit.Last != cjProduct.OidShort) // we don't come back to the same product multiple times in between other products so ...
          {
            //tokenInProduct.Persist(invertedIndex.tokenInProductPlacement, session, true);
            tokenStoreHit.Add(cjProduct.OidShort);
          }
          //tokenInProduct.Add(fieldPosition);
          //verify(false);
        }
        else
        {
          tokenStoreHit = new TokenStoreHit();
          tokenStoreHit.Persist(cjStore.TokenStoreHitPlacement, session);
          tokenStoreHit.Add(cjProduct.OidShort);
          cjStore.TokenStoreHit.Add(stringToken, tokenStoreHit);
          //tokenInProduct.Persist(invertedIndex.tokenInProductPlacement, session, true);
          //tokenInProduct.Add(fieldPosition);
          //tokenHit.tokenInProductSet.Add(tokenInProduct);
          stringToken.TokenCjStoreHit.Add(cjStore);
          stringToken.GlobalCount = stringToken.GlobalCount + 1;
          //doc.wordSet.Add(existingWord);
          //verify(false);
        }
      }
      else
      {
        stringToken = new Token<string>(word, session);
        stringToken.Persist(invertedIndex.tokenPlacement, session, true);
        invertedIndex.stringLexicon.tokenSet.Add(stringToken);

        tokenStoreHit = new TokenStoreHit();
        tokenStoreHit.Persist(cjStore.TokenStoreHitPlacement, session);
        tokenStoreHit.Add(cjProduct.OidShort);
        //tokenInProduct.Persist(invertedIndex.tokenInProductPlacement, session, true);
        //tokenInProduct.Add(fieldPosition);
        //tokenHit.tokenInProductSet.Add(tokenInProduct);
        cjStore.TokenStoreHit.Add(stringToken, tokenStoreHit); // maybe multiple threads are working on the same store in case multiple input files exist for the saME STORE ???
        stringToken.TokenCjStoreHit.Add(cjStore);
        //doc.wordSet.Add(word);
        //verify(false);
      }
    }
#endif

    public void AddCjStringFieldToDocInvertedIndex(UInt16 fieldPosition, string word, Product cjProduct, SessionBase session, ref StoreBase cjStore,
      ref BTreeMap<UInt32, TokenStoreHit> tokenStoreHitMap)
    {
      if (cjStore.StoreName != cjProduct.StoreName)
      {
        if (!m_cjLinkShareStoreSet.TryGetKey(cjStore, ref cjStore))
        {
          session.Persist(cjStore);
          m_cjLinkShareStoreSet.Add(cjStore);
        }
        tokenStoreHitMap = cjStore.TokenStoreHit;
      }
      TokenStoreHit tokenStoreHit;
      var id = InvertedIndex.Lexicon.PossiblyAddToken(word);
      //TokenInProduct tokenInProduct = new TokenInProduct(cjProduct, session);
      if (tokenStoreHitMap.TryGetValue(id, out tokenStoreHit))
      {
        //stringToken.GlobalCount = stringToken.GlobalCount + 1;
 
          //tokenInProduct.Persist(invertedIndex.tokenInProductPlacement, session, true);
          tokenStoreHit.Add(cjProduct.Oid);
      
        //tokenInProduct.Add(fieldPosition);
        //verify(false);
      }
      else
      {
        tokenStoreHit = new TokenStoreHit(session);
        //tokenStoreHit.Persist(cjStore.TokenStoreHitPlacement, session, true); // ? disable flush so that we don't flush and update the same page multiple times
        tokenStoreHit.Add(cjProduct.Oid);
        //stringToken.Persist(cjStore.TokenPlacement, session, true);
        tokenStoreHitMap.AddFast(id, tokenStoreHit);
        //tokenInProduct.Persist(invertedIndex.tokenInProductPlacement, session, true);
        //tokenInProduct.Add(fieldPosition);
        //tokenHit.tokenInProductSet.Add(tokenInProduct);
        //doc.wordSet.Add(existingWord);
        //verify(false);
      }
    }

    /*    public void AddCjProductToInvertedIndexSlower(CommisionJunctionStoreProduct cjProduct, SessionBase session)
        {
          string[] words;
          string aWord;
          string fieldValue;
          UInt16 fieldPosition = 0;
          foreach (DataMember member in cjProduct.GetDataMembers())
          {
            switch (member.TypeCode)
            {
              case TypeCode.String:
                fieldValue = (string)member.GetMemberValue(cjProduct);
                words = fieldValue.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
                foreach (string word in words)
                {
                  aWord = word.TrimEnd(trimEndChars);
                  aWord = aWord.TrimStart(trimStartChars);
                  AddCjStringFieldToInvertedIndex(fieldPosition, aWord, cjProduct, session);
                }
                fieldPosition++;
                break;
            }
          }
        }*/

    public void AddCjStringsieldToInvertedIndex(UInt16 fieldPosition, string strings, Product cjProduct, SessionBase session, StoreBase cjStore,
      ref BTreeMap<UInt32, TokenStoreHit> tokenStoreHit)
    {
      if (strings != null && strings.Length > 0)
      {
        //string[] words = strings.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
        //var words = Regex.Matches(strings, @"((\b[^\s]+\b)((?<=\.\w).)?)"); 
        // not tested var words = Regex.Matches(strings, (@"[\s\pP]* ([\pL\pN_-] (?: [\pL\pN_-] | \pP(?=[\pL\pN\pP_-]) )* )"));
        //var words = Regex.Matches(strings, @"\b\w+");
        var words = Regex.Matches(strings, "[a-z][a-z]+");
        foreach (var word in words)
        {
          string aWord = word.ToString().ToLower();
          //string aWord = word.TrimEnd(trimEndChars);
          //aWord = aWord.TrimStart(trimStartChars).ToLower();
          if (aWord.Length > 1 && Array.BinarySearch<string>(s_skipWords, aWord) < 0)
            AddCjStringFieldToDocInvertedIndex(fieldPosition, aWord, cjProduct, session, ref cjStore, ref tokenStoreHit);
        }
      }
    }

    public void AddCjProductToInvertedIndex(Product cjProduct, SessionBase session, StoreBase cjStore, ref BTreeMap<UInt32, TokenStoreHit> tokenStoreHit)
    {
      UInt16 fieldPosition = 0;
      AddCjStringsieldToInvertedIndex(fieldPosition++, cjProduct.StoreName, cjProduct, session, cjStore, ref tokenStoreHit);
      //AddCjStringsieldToInvertedIndex(fieldPosition++, cjProduct.Advertisercategory, cjProduct, session, cjStore);
      AddCjStringsieldToInvertedIndex(fieldPosition++, cjProduct.ProductName, cjProduct, session, cjStore, ref tokenStoreHit);
      AddCjStringsieldToInvertedIndex(fieldPosition++, cjProduct.Keywords, cjProduct, session, cjStore, ref tokenStoreHit);
      AddCjStringsieldToInvertedIndex(fieldPosition++, cjProduct.LongDescription, cjProduct, session, cjStore, ref tokenStoreHit);
      AddCjStringsieldToInvertedIndex(fieldPosition++, cjProduct.Manufacturer, cjProduct, session, cjStore, ref tokenStoreHit);
      //AddCjStringsieldToInvertedIndex(fieldPosition++, cjProduct.Isbn, cjProduct, session, cjStore);
      //AddCjStringsieldToInvertedIndex(fieldPosition++, cjProduct.Author, cjProduct, session, cjStore);
      //AddCjStringsieldToInvertedIndex(fieldPosition++, cjProduct.Artist, cjProduct, session, cjStore);
      //AddCjStringsieldToInvertedIndex(fieldPosition++, cjProduct.Title, cjProduct, session, cjStore);
      //AddCjStringsieldToInvertedIndex(fieldPosition++, cjProduct.Publisher, cjProduct, session, cjStore);
      // AddCjStringsieldToInvertedIndex(fieldPosition++, cjProduct.Promotionaltext, cjProduct, session, cjStore);
    }

    public int ImportGoogleStoreProductsFromTextFile(string fileName, SessionBase session, ref int numberOfErrors)
    {
      int numberOfImports = 0;
      string priorProgramName = null;
      StoreBase googleStore = null;
      BTreeSet<Product> storeProductSet = null;
      //string storeNameBad = fileName.Substring(0, fileName.IndexOf(".txt"));
      //string storeNameBad = fileName.Substring(0, Path.GetFileName(fileName).IndexOf(".txt"));
      //string storeNameBad = fileName.Substring(0, fileName.IndexOf(".txt"));
      string storeName = Path.GetFileName(fileName);
      storeName = storeName.Substring(0, storeName.IndexOf(".txt"));
      using (FileStream stream = File.OpenRead(fileName))
      //using (ZipFile decompress = new ZipFile(stream, CompressionMode.Decompress))
      using (System.IO.StreamReader file = new System.IO.StreamReader(stream))
      {
        string line;
        int lineNumber = 0;
        while ((line = file.ReadLine()) != null)
        {
          lineNumber++;
          if (lineNumber > 1)
          {
            try
            {
              char[] delimiters = new char[] { '\t' };
              string[] parts = line.Split(delimiters);
              int i = 0;
              string productId = parts[i++];
              string productName = parts[i++];
              string productUrl = parts[i++];
              string buyUrl = parts[i++];
              string imageUrl = parts[i++];
              string productCategory = parts[i++];
              string productCategoryId = parts[i++];
              string PFXCategory = parts[i++];
              string BriefDesc = parts[i++];
              string ShortDesc = parts[i++];
              string IntermDesc = parts[i++];
              string UPC = "";
              string Price = "";
              if (parts.Length > i)
              {
                string LongDesc = parts[i++];
                string ProductKeyword = parts[i++];
                string Brand = parts[i++];
                string Manufacturer = parts[i++];
                string ManfID = parts[i++];
                string ManufacturerModel = parts[i++];
                UPC = parts[i++];
                string Platform = parts[i++];
                string MediaTypeDesc = parts[i++];
                string MerchandiseType = parts[i++];
                Price = parts[i++];
                string SalePrice = parts[i++];
                string VariableCommission = parts[i++];
                string SubFeedID = parts[i++];
                string InStock = parts[i++];
                string Inventory = parts[i++];
                string RemoveDate = parts[i++];
                string RewPoints = parts[i++];
                string PartnerSpecific = parts[i++];
                string ShipAvail = parts[i++];
                string ShipCost = parts[i++];
                string ShippingIsAbsolut = parts[i++];
                string ShippingWeight = parts[i++];
                string ShipNeeds = parts[i++];
                string ShipPromoText = parts[i++];
                string ProductPromoText = parts[i++];
                string DailySpecialsInd = parts[i++];
                string GiftBoxing = parts[i++];
                string GiftWrapping = parts[i++];
                string GiftMessaging = parts[i++];
                string ProductContainerName = parts[i++];
                string CrossSellRef = parts[i++];
                string AltImagePrompt = parts[i++];
                string AltImageURL = parts[i++];
                string AgeRangeMin = parts[i++];
                string AgeRangeMax = parts[i++];
                string ISBN = parts[i++];
                string Title = parts[i++];
                string Publisher = parts[i++];
                string Author = parts[i++];
                string Genre = parts[i++];
                string Media = parts[i++];
                string Material = parts[i++];
                string PermuColor = parts[i++];
                string PermuSize = parts[i++];
                string PermuWeight = parts[i++];
                string PermuItemPrice = parts[i++];
                string PermuSalePrice = parts[i++];
                string PermuInventorySta = parts[i++];
                string Permutation = parts[i++];
                string PermutationSKU = parts[i++];
                string BaseProductID = parts[i++];
                string Option1 = parts[i++];
                string Option2 = parts[i++];
                string Option3 = parts[i++];
                string Option4 = parts[i++];
                string Option5 = parts[i++];
                string Option6 = parts[i++];
                string Option7 = parts[i++];
                string Option8 = parts[i++];
                string Option9 = parts[i++];
                string Option10 = parts[i++];
                string Option11 = parts[i++];
                string Option12 = parts[i++];
                string Option13 = parts[i++];
                string Option14 = parts[i++];
                string Option15 = parts[i++];
                string Option16 = parts[i++];
                string Option17 = parts[i++];
                string Option18 = parts[i++];
                string Option19 = parts[i++];
                string Option20 = parts[i++];
              }
              double upc;
              double.TryParse(UPC, out upc);
              Decimal price;
              Decimal.TryParse(Price, out price);
              GoogleStoreProduct storeProduct = new GoogleStoreProduct
              {
                StoreName = storeName,
                BuyUrl = buyUrl,
                ShortDescription = BriefDesc,
                ImageUrl = imageUrl,
                Price = price,
                ProductName = productName
              };
              if (priorProgramName != storeName)
              {
                googleStore = new GoogleStore(storeName, session);
                if (m_cjLinkShareStoreSet.TryGetKey(googleStore, ref googleStore))
                {
                  if (googleStore.Indexed) // if indexed then we are reimporting the same store so delete prior store data before proceeding
                  {
                    recycleStore(googleStore, storeName, session);
                  }
                }
                else
                {
                  googleStore = new GoogleStore(storeName, session);
                  session.Persist(googleStore);
                  m_cjLinkShareStoreSet.Add(googleStore);
                }
                storeProductSet = googleStore.ProductSet;
              }
              numberOfImports++;
              session.Persist(storeProduct);
              googleStore.ProductSet.Add(storeProduct);
              //AddCjProductToInvertedIndex(storeProduct, session, cjStore);
              priorProgramName = storeName;
            }
            catch
            {
              numberOfErrors++;
            }
          }
        }
      }
      return numberOfImports;
    }

    void recycleStore(StoreBase cjStore, string programname, SessionBase session)
    {
      var tokenMap = InvertedIndex.Lexicon.TokenMap;
      BTreeSet<StoreBase> tokenLocations = null;
      m_cjLinkShareStoreSet.Remove(cjStore);
      var tokens = cjStore.TokenStoreHit;
      foreach (var token in tokens)
      {
        if (tokenMap.TryGetValue(token.Key, out tokenLocations))
        {
          //--existinToken.GlobalCount;
          tokenLocations.Remove(cjStore);
          if (tokenLocations.Count == 0)
          {
            tokenMap.Remove(token.Key);
            tokenLocations.Unpersist(session);
          }
        }
      }
      Database db = cjStore.GetPage().Database;
      UInt32 dbNum = cjStore.DatabaseNumber;
      string dbName = db.Name;
      //UInt32 dbSize = db.
      session.DeleteDatabase(db);
      session.Commit();
      session.BeginUpdate();
      db = session.NewDatabase(dbNum, 0, dbName);
      cjStore = new CjStore(programname, session);
      session.Persist(cjStore);
      m_cjLinkShareStoreSet.Add(cjStore);
    }

    public int ImportCommisionJunctionStoreProductsFromTextFile(string fileName, SessionBase session, ref int numberOfErrors)
    {
      Decimal tempDecimal;
      DateTime tempDateTime;
      int numberOfImports = 0;
      string priorProgramName = null;
      StoreBase cjStore = null;
      BTreeSet<Product> storeProductSet = null;
      using (FileStream stream = File.OpenRead(fileName))
      using (var zip = new ZipArchive(stream))
      //using (GZipStream decompress = new GZipStream(stream, CompressionMode.Decompress))
      using (System.IO.StreamReader file = new System.IO.StreamReader(zip.Entries[0].Open()))
      {
        int lineNumber = 1;
        CsvReader csvReader = new CsvReader(file, true);
        string[] fileldNames = csvReader.GetFieldHeaders();
        foreach (string[] parts in csvReader)
        {
            try
            {
              lineNumber++;
              int i = 0;
              string t;
              string programname = parts[i++];
              string programurl = parts[i++];
              string catalogname = parts[i++];
              DateTime lastupdated = DateTime.Parse(parts[i++]);
              string id = parts[i++];
              string name = parts[i++];
              // string keywords = parts.Length > i ? parts[i++] : "";
              string description = parts.Length > i ? parts[i++] : "";
              string buyurl = parts.Length > i ? parts[i++] : "";
              string impressionurl = parts.Length > i ? parts[i++] : "";
              string imageurl = parts.Length > i ? parts[i++] : "";
              string mobileLink = parts.Length > i ? parts[i++] : "";
              string additionalImageLink = parts.Length > i ? parts[i++] : "";
              string availability = parts.Length > i ? parts[i++] : "";
              DateTime availableDate;
              DateTime.TryParse(parts[i++], out availableDate);
              DateTime expirationDate;
              DateTime.TryParse(parts[i++], out expirationDate);
              string priceWithCurrency = parts.Length > i ? parts[i++] : "";
              Decimal price;
              Decimal.TryParse(priceWithCurrency.Length > i ? priceWithCurrency.Substring(0, priceWithCurrency.IndexOf(' ')): "", out price);
              string salePriceWithCurrency = parts.Length > i ? parts[i++] : "";
              Decimal salePrice;
              Decimal.TryParse(salePriceWithCurrency.Length > i ? salePriceWithCurrency.Substring(0, salePriceWithCurrency.IndexOf(' ')) : "", out salePrice);
              string currency = "";
              if (salePriceWithCurrency.Length > 0)
                currency = priceWithCurrency.Substring(priceWithCurrency.IndexOf(' ') + 1);
              DateTime salePriceEffectiveDate = DateTime.MaxValue;
              DateTime.TryParse(parts[i++], out salePriceEffectiveDate);
              string unitPricingBaseMeasure = parts.Length > i ? parts[i++] : "";
              string installment = parts.Length > i ? parts[i++] : "";
              string loyaltyPoints = parts.Length > i ? parts[i++] : "";
              int productCategory;
              int.TryParse(parts[i++], out productCategory);
              string productCategoryName = parts.Length > i ? parts[i++] : "";
              string productType = parts.Length > i ? parts[i++] : "";
              string brand = parts.Length > i ? parts[i++] : "";
              string keywords = "";
              string sku = parts.Length > i ? parts[i++] : "";
              string manufacturer = parts.Length > i ? parts[i++] : "";
              string manufacturerid = parts.Length > i ? parts[i++] : "";
              double upc;
              double.TryParse(parts.Length > i ? parts[i++] : "", out upc);
              string isbn = parts.Length > i ? parts[i++] : "";

              t = parts.Length > i ? parts[i++] : "";
              bool fromprice = t == "YES";

              string advertisercategory = parts.Length > i ? parts[i++] : "";
              string thirdpartyid = parts.Length > i ? parts[i++] : "";
              string thirdpartycategory = parts.Length > i ? parts[i++] : "";
              string author = parts.Length > i ? parts[i++] : "";
              string artist = parts.Length > i ? parts[i++] : "";
              string title = parts.Length > i ? parts[i++] : "";
              string publisher = parts.Length > i ? parts[i++] : "";
              string label = parts.Length > i ? parts[i++] : "";
              string format = parts.Length > i ? parts[i++] : "";
              t = parts.Length > i ? parts[i++] : "";
              bool special = t == "YES";
              t = parts.Length > i ? parts[i++] : "";
              bool gift = t == "YES";
              string promotionaltext = parts.Length > i ? parts[i++] : "";
              t = parts.Length > i ? parts[i++] : "";
              DateTime? startdate = null;
              //if (t.Length > 0)
              //{
              //  if (!DateTime.TryParse(t, out tempDateTime))
              //  {
              //    numberOfErrors++;
              //    continue;
              //  }
              //  startdate = tempDateTime;
              //}
              t = parts.Length > i ? parts[i++] : "";
              DateTime? enddate = null;
              //if (t.Length > 0)
              //{
              //  if (!DateTime.TryParse(t, out tempDateTime))
              //  {
              //    numberOfErrors++;
              //    continue;
              //  }
              //  enddate = tempDateTime;
              //}
              t = parts.Length > i ? parts[i++] : "";
              bool offline = t == "YES";
              t = parts.Length > i ? parts[i++] : "";
              bool online = t == "YES";
              t = parts.Length > i ? parts[i++] : "";
              bool instock = t == "YES";
              string condition = parts.Length > i ? parts[i++] : "";
              string warranty = parts.Length > i ? parts[i++] : "";
              Decimal standardshippingcost;
              Decimal.TryParse(parts.Length > i ? parts[i++] : "", out standardshippingcost);
              CommisionJunctionStoreProduct storeProduct = new CommisionJunctionStoreProduct
              {
                StoreName = programname,
                Programurl = programurl,
                Catalogname = catalogname,
                Lastupdated = lastupdated,
                ProductName = name,
                Keywords = keywords.TrimStart('"').TrimEnd('"'),
                LongDescription = description.TrimStart('"').TrimEnd('"'),
                Sku = sku,
                Manufacturer = manufacturer,
                ManufacturerId = manufacturerid,
                Upc = upc,
                Isbn = isbn,
                Currency = currency,
                Saleprice = salePrice,
                Price = price,
                Retailprice = price,
                Fromprice = fromprice,
                BuyUrl = buyurl,
                Impressionurl = impressionurl,
                ImageUrl = imageurl,
                Advertisercategory = advertisercategory,
                Thirdpartyid = thirdpartyid,
                Thirdpartycategory = thirdpartycategory,
                Author = author,
                Artist = artist,
                Title = title,
                Publisher = publisher,
                Label = label,
                Format = format,
                Special = special,
                Gift = gift,
                Promotionaltext = promotionaltext,
                Startdate = startdate,
                ExpireDate = enddate,
                Offline = offline,
                Online = online,
                Instock = instock,
                Condition = condition,
                Warranty = warranty,
                ShippingCost = standardshippingcost
              };
              if (priorProgramName != programname)
              {
                cjStore = new CjStore(programname, session);
                lock (m_cjLinkShareStoreSet)
                {
                  if (m_cjLinkShareStoreSet.TryGetKey(cjStore, ref cjStore))
                  {
                    if (cjStore.Indexed) // if indexed then we are reimporting the same store so delete prior store data before proceeding
                    {
                      recycleStore(cjStore, programname, session);
                    }
                  }
                  else
                  {
                    session.Persist(cjStore);
                    m_cjLinkShareStoreSet.Add(cjStore);
                  }
                }
                storeProductSet = cjStore.ProductSet;
              }
              numberOfImports++;
              //storeProduct.Persist(cjStore.ProductPlacement, session);
              storeProductSet.AddFast(storeProduct);
              //AddCjProductToInvertedIndex(storeProduct, session, cjStore);
              priorProgramName = programname;
            }
            catch (OutOfMemoryException)
            {
              throw;
            }
            catch
            {
              numberOfErrors++;
            }
          }
        }
      return numberOfImports;
    }

    public int ImportLinkShareProductsFromTextFile(string fileName, SessionBase session, ref int numberOfErrors)
    {
      int adveriserId;
      string advertiserName = "";
      DateTime fileCreatedTime;
      int numberOfImports = 0;
      string priorProgramName = null;
      StoreBase linkShareStore = null;
      BTreeSet<Product> storeProductSet = null;
      char[] delimiters = new char[] { '|' };
      using (FileStream stream = File.OpenRead(fileName))
      using (GZipStream decompress = new GZipStream(stream, CompressionMode.Decompress))
      using (System.IO.StreamReader file = new System.IO.StreamReader(decompress))
      {
        string line;
        int lineNumber = 0;
        while ((line = file.ReadLine()) != null)
        {
          string[] parts = line.Split(delimiters);
          int i = 0;
          if (lineNumber++ == 0)
          {
            string header = parts[i++];
            adveriserId = int.Parse(parts[i++]);
            advertiserName = parts[i++];
            if (advertiserName.Contains("Canada"))
              break;
            fileCreatedTime = DateTime.Parse(parts[i]);
          }
          else if (parts.Length > 30)
          {
            try
            {
              string t = parts[i++];;
              UInt64 productId;
              UInt64.TryParse(t, out productId);
              string productName = parts[i++];
              string sku = parts[i++];
              string primaryCategory = parts[i++];
              string secondaryCategories = parts[i++];
              string productUrl = parts[i++];
              string imageUrl = parts[i++];
              string buyurl = parts[i++];
              string shortDescription = parts[i++];
              string longDescription = parts[i++];
              t = parts[i++];
              string discountType = parts[i++];
              t = parts[i++];
              Decimal salesPrice;
              Decimal.TryParse(t, out salesPrice);
              t = parts[i++];
              Decimal price;
              price = Decimal.Parse(t);
              DateTime startTime = DateTime.MaxValue;
              t = parts[i++];
              DateTime.TryParse(t, out startTime);
              DateTime endTime = DateTime.MinValue;
              t = parts[i++];
              DateTime.TryParse(t, out endTime);
              string brand = parts[i++];
              t = parts[i++];
              Decimal shippingCost;
              Decimal.TryParse(t, out shippingCost);
              string keywords = parts[i++];
              string manufacturerPartId = parts[i++];
              string manufacturerName = parts[i++];
              string shippingInfo = parts[i++];
              t = parts[i++]; // availability
              string universalProductCode = parts[i++];
              t = parts[i++];
              UInt64 classID;
              UInt64.TryParse(t, out classID);
              string currency = parts[i++];
              string m1 = parts[i++];
              string pixel = parts[i++];
              string miscellaneousAttribut = parts[i++];
              string attribute2 = parts[i++];
              string attribute3 = parts[i++];
              string attribute4 = parts[i++];
              string attribute5 = parts[i++];
              string attribute6 = parts[i++];
              string attribute7 = parts[i++];
              string attribute8 = parts[i++];
              string attribute9 = parts[i++];
              string attribute10 = parts[i++];
              string modification = parts.Length > i ? parts[i++] : "";
              LinkShareProduct storeProduct = new LinkShareProduct
              {
                ProductId = productId,
                StoreName = advertiserName,
                ProductName = productName,
                LongDescription = longDescription,
                Sku = sku,
                Currency = currency,
                BuyUrl = productUrl,
                Price = price,
                ImageUrl = imageUrl,
                Keywords = keywords.TrimStart('"').TrimEnd('"'),
                Saleprice = salesPrice > 0 ? salesPrice : price
              };
              if (priorProgramName != advertiserName)
              {
                linkShareStore = new LinkShareStore(advertiserName, session);
                lock (m_cjLinkShareStoreSet)
                {
                  if (m_cjLinkShareStoreSet.TryGetKey(linkShareStore, ref linkShareStore))
                  {
                    if (linkShareStore.Indexed) // if indexed then we are reimporting the same store so delete prior store data before proceeding
                    {
                      recycleStore(linkShareStore, advertiserName, session);
                    }
                  }
                  else
                  {
                    session.Persist(linkShareStore);
                    m_cjLinkShareStoreSet.Add(linkShareStore);
                  }
                }
                storeProductSet = linkShareStore.ProductSet;
              }
              if (++numberOfImports % 100000 == 0)
                Console.WriteLine(numberOfImports.ToString() + " products imported from " + fileName + " for advertiser " + advertiserName);
              storeProductSet.AddFast(storeProduct);
              priorProgramName = advertiserName;
            }
            catch (OutOfMemoryException)
            {
              throw;
            }
            catch
            {
              numberOfErrors++;
            }
          }
        }
      }
      return numberOfImports;
    }

    public int ImportCategoryAdFromTextFile(Stream fileStream)
    {
      int numberOfImports = 0;
      using (System.IO.StreamReader file = new System.IO.StreamReader(fileStream))
      {
        string line;
        int lineNumber = 0;
        while ((line = file.ReadLine()) != null)
        {
          lineNumber++;
          char[] delimiters = new char[] { '\t' };
          string[] parts = line.Split(delimiters);
          int i = 0;
          string categoryName = parts[i++];
          string price = parts[i++];
          string text = parts[i++];
          string image = parts[i++];
          string link = parts[i++];
          string expireDate = parts[i++];
          if (lineNumber > 1)
          {
            StoreCategory cat;
            if (!this.m_categorySet.TryGetValue(categoryName, out cat))
              continue;
            CategoryAd catAd = new CategoryAd(categoryName, price, text, image, link, expireDate);
            numberOfImports++;
            cat.adList.Add(catAd);
          }
        }
      }
      return numberOfImports;
    }

    public int ImportProductsFromTextFile(Stream fileStream)
    {
      int numberOfImports = 0;
      using (System.IO.StreamReader file = new System.IO.StreamReader(fileStream))
      {
        string line;
        int lineNumber = 0;
        while ((line = file.ReadLine()) != null)
        {
          lineNumber++;
          char[] delimiters = new char[] { '\t' };
          string[] parts = line.Split(delimiters);
          int i = 0;
          string storeStr = parts[i++];
          string expireDate = parts[i++];
          string name = parts[i++];
          string description = parts[i++];
          string link = parts[i++];
          string image = parts[i++];
          if (lineNumber > 1)
          {
            StoreProducts storeProducts;
            if (!this.m_productSet.TryGetValue(storeStr, out storeProducts))
              continue;
            StoreProduct storeProduct = new StoreProduct(storeStr, expireDate, name, description, link, image);
            numberOfImports++;
            storeProducts.productList.Add(storeProduct);
          }
        }
      }
      return numberOfImports;
    }

    public int ImportCategoriesFromTextFile(Stream fileStream)
    {
      int numberOfImports = 0;
      using (System.IO.StreamReader file = new System.IO.StreamReader(fileStream))
      {
        string line;
        int lineNumber = 0;
        while ((line = file.ReadLine()) != null)
        {
          lineNumber++;
          char[] delimiters = new char[] { '\t' };
          string[] parts = line.Split(delimiters);
          int i = 0;
          string name = parts[i++];
          string url = parts[i++];
          string parent = parts[i++];
          string level = parts[i++];
          if (lineNumber > 1)
          {
            if (this.m_categorySet.ContainsKey(name) == false)
            {
              StoreCategory parentCategory;
              this.m_categorySet.TryGetValue(parent, out parentCategory);
              StoreCategory category = new StoreCategory(name, url, int.Parse(level), parentCategory);
              numberOfImports++;
              if (parentCategory != null)
                parentCategory.categoryList.Add(category);
              this.m_categorySet.Add(name, category);
              this.m_categoryList.Add(category);
            }
          }
        }
      }
      return numberOfImports;
    }

    public int ImportMiscStoresFromTextFile(Stream fileStream, SessionBase session)
    {
      int numberOfImports = 0;
      using (System.IO.StreamReader file = new System.IO.StreamReader(fileStream))
      {
        string line;
        int lineNumber = 0;
        Store priorStore = null;
        while ((line = file.ReadLine()) != null)
        {
          lineNumber++;
          try
          {
            if (lineNumber > 1)
            {
              char[] delimiters = new char[] { '\t' };
              string[] parts = line.Split(delimiters);
              int i = 0;
              string OrganizationName = parts[i++];
              if (OrganizationName != null && OrganizationName.Length > 0)
              {
                string CategoryName = parts[i++];
                string Image = parts[i++];
                string trackingUrl = parts[i++];
                StoreCategory category = m_categorySet[CjCategoryToCategory(CategoryName)];
                Store store;
                if (m_storeSet.TryGetValue(OrganizationName, out store) == false)
                {
                  store = new Store(OrganizationName);
                  session.Persist(store);
                  priorStore = store;
                  numberOfImports++;
                  m_storeSet.Add(OrganizationName, store);
                  StoreInCategory storeInCategory = new StoreInCategory(store, category);
                  session.Persist(storeInCategory);
                  store.m_categoryList.Add(storeInCategory);
                  category.storeInCategoryList.Add(storeInCategory);
                  storeInCategory.Click = trackingUrl;
                  storeInCategory.Image = Image;
                }
              }
            }
          }
          catch (Exception e)
          {
            Console.WriteLine(e);
          }
        }
      }
      return numberOfImports;
    }

    public int ImportShareasaleStoresFromTextFile(Stream fileStream, SessionBase session)
    {
      int numberOfImports = 0;
      using (System.IO.StreamReader file = new System.IO.StreamReader(fileStream))
      {
        string line;
        int lineNumber = 0;
        Store priorStore = null;
        while ((line = file.ReadLine()) != null)
        {
          lineNumber++;
          try
          {
            if (lineNumber > 1)
            {
              char[] delimiters = new char[] { '|' };
              string[] parts = line.Split(delimiters);
              int i = 0;
              string MerchantID = parts[i++];
              if (MerchantID != null && MerchantID.Length > 0)
              {
                string OrganizationName = parts[i++];
                string ContactName = parts[i++];
                string ContactEmail = parts[i++];
                string WWW = parts[i++];
                string DefaultSaleCommission = parts[i++];
                string DefaultLeadCommission = parts[i++];
                string DefaultHitCommission = parts[i++];
                string CategoryStub = parts[i++];
                string CategoryName = parts[i++];
                string StartDateMerchant = parts[i++];
                string DefaultCookieLength = parts[i++];
                string Day7EPC = parts[i++];
                string Day30EPC = parts[i++];
                string Day7ReversalRate = parts[i++];
                string Day30ReversalRate = parts[i++];
                string PowerRank = parts[i++];
                string LastComplianceTestDate = parts[i++];
                string CustomSaleCommission = parts[i++];
                string CustomLeadCommission = parts[i++];
                string CustomHitCommission = parts[i++];
                string CustomCookieLength = parts[i++];
                string AffiliateGroup = parts[i++];
                string GroupSaleCommission = parts[i++];
                string GroupLeadCommission = parts[i++];
                string GroupHitCommission = parts[i++];
                string YourApplicationDate = parts[i++];
                string ListOfHolidays = parts[i++];
                StoreCategory category = m_categorySet[ShareasaleCategoryToCategory(CategoryName)];
                if (m_storeSet.ContainsKey(OrganizationName) == false)
                {
                  Store store = new Store(OrganizationName);
                  session.Persist(store);
                  priorStore = store;
                  numberOfImports++;
                  m_storeSet.Add(OrganizationName, store);
                  StoreInCategory storeInCategory = new StoreInCategory(store, category);
                  session.Persist(storeInCategory);
                  store.m_categoryList.Add(storeInCategory);
                  category.storeInCategoryList.Add(storeInCategory);
                }
              }
            }
          }
          catch (Exception e)
          {
            Console.WriteLine(e);
          }
        }
      }
      return numberOfImports;
    }

    // 	Category	Description	Restrictions	Keywords	Coupon Code	Edit Date
    public int ImportShareasalePromosFromTextFile(Stream fileStream, SessionBase session)
    {
      int numberOfImports = 0;
      using (System.IO.StreamReader file = new System.IO.StreamReader(fileStream))
      {
        string line;
        int lineNumber = 0;
        while ((line = file.ReadLine()) != null)
        {
          lineNumber++;
          char[] delimiters = new char[] { '|' };
          string[] parts = line.Split(delimiters);
          int i = 0;
          string DealId = parts[i++];
          string MerchantId = parts[i++];
          string Merchant = parts[i++];
          string promoStartDate = parts[i++];
          string promoEndDate = parts[i++];
          string PublishDate = parts[i++];
          string Title = parts[i++];
          string ImageBig = parts[i++];
          string trackingUrl = parts[i++];
          string ImageSmall = parts[i++];
          string Category = parts[i++];
          string description = parts[i++];
          string Restrictions = parts[i++];
          string Keywords = parts[i++];
          string CouponCode = parts[i++];
          string EditDate = parts[i++];
          if (lineNumber > 1)
          {
            DateTime endDate = DateTime.MaxValue;
            DateTime startDate = DateTime.MinValue;
            if (promoStartDate.Length > 0 && promoStartDate != "00:00.0")
              if (!DateTime.TryParse(promoStartDate, out startDate))
                continue;
            if (promoEndDate.Length > 0 && promoEndDate != "NEVER")
              if (!DateTime.TryParse(promoEndDate, out endDate))
                continue;
            Store store;
            if (this.m_storeSet.TryGetValue(Merchant, out store))
            {
              numberOfImports++;
              StoreInCategory inCategory = store.m_categoryList[0];
              if (startDate < DateTime.Now && endDate > DateTime.Now)
              {
                if (inCategory.Click == null)
                {
                  inCategory.Click = trackingUrl;
                  inCategory.Image = ImageSmall;
                  store.Description = description;
                }
                StoreCoupons storeCoupons;
                if (this.m_couponSet.TryGetValue(Merchant, out storeCoupons) == false)
                {
                  storeCoupons = new StoreCoupons(store, "", description, ImageSmall, trackingUrl, 0);
                  this.m_couponSet.Add(Merchant, storeCoupons);
                }
                Coupon coupon = new Coupon(startDate, endDate, description.ToLower().Contains("free shipping") ? 1 : 2, description, CouponCode, trackingUrl, null);
                for (i = 0; i < storeCoupons.couponList.Count; i++)
                {
                  Coupon c = storeCoupons.couponList[i];
                  if ((c.Description == description && trackingUrl == c.Link && c.Code == CouponCode) || c.ExpireDate < DateTime.Now)
                  {
                    storeCoupons.couponList.RemoveAt(i);
                    c.Unpersist(session);
                  }
                }
                storeCoupons.couponList.Add(coupon);
              }
            }
          }
        }
      }
      return numberOfImports;
    }

    public int ImportGoogleStoresFromTextFile(Stream fileStream, SessionBase session)
    {
      int numberOfImports = 0;
      using (System.IO.StreamReader file = new System.IO.StreamReader(fileStream))
      {
        string line;
        int lineNumber = 0;
        Store priorStore = null;
        while ((line = file.ReadLine()) != null)
        {
          lineNumber++;
          char[] delimiters = new char[] { ',' };
          string[] parts = line.Split(delimiters);
          int i = 0;
          string advertiserId = parts[i++];
          string name = parts[i++];
          string siteUrl = parts[i++];
          string cjCategory = parts[i++];
          string hasProducts = parts[i++];
          string commisionDuration = parts[i++];
          string epc3month = parts[i++];
          string epc7day = parts[i++];
          string logoUrl = parts[i++];
          string email = parts[i++];
          string phone = parts[i++];
          if (lineNumber > 1)
          {
            StoreCategory category = m_categorySet[GoogleCategoryToCategory(cjCategory)];
            if (this.m_storeSet.ContainsKey(name) == false)
            {
              Store store = new Store(name);
              session.Persist(store);
              priorStore = store;
              numberOfImports++;
              this.m_storeSet.Add(name, store);
              StoreInCategory storeInCategory = new StoreInCategory(store, category);
              session.Persist(storeInCategory);
              store.m_categoryList.Add(storeInCategory);
              category.storeInCategoryList.Add(storeInCategory);
            }
          }
        }
      }
      return numberOfImports;
    }

    public int ImportCjStoresFromTextFile(Stream fileStream, SessionBase session)
    {
      int numberOfImports = 0;
      using (System.IO.StreamReader file = new System.IO.StreamReader(fileStream))
      {
        string line;
        int lineNumber = 0;
        Store priorStore = null;
        while ((line = file.ReadLine()) != null)
        {
          lineNumber++;
          char[] delimiters = new char[] { '\t' };
          string[] parts = line.Split(delimiters);
          int i = 0;
          string advertiserId = parts[i++];
          string name = parts[i++];
          string hasProducts = parts[i++];
          //string currency = parts[i++];
          string cjCategory = parts[i++];
          string country = parts[i++];
          //string dummy = parts[i++];
          if (lineNumber > 1)
          {
            StoreCategory category = m_categorySet[CjCategoryToCategory(cjCategory)];
            if (m_storeSet.ContainsKey(name) == false)
            {
              Store store = new Store(name);
              session.Persist(store);
              priorStore = store;
              numberOfImports++;
              m_storeSet.Add(name, store);
              StoreInCategory storeInCategory = new StoreInCategory(store, category);
              session.Persist(storeInCategory);
              store.m_categoryList.Add(storeInCategory);
              category.storeInCategoryList.Add(storeInCategory);
            }
          }
        }
      }
      return numberOfImports;
    }

    public int ImportCjStoresFromCsvFile(Stream fileStream, SessionBase session)
    {
      int numberOfImports = 0;
      using (System.IO.StreamReader file = new System.IO.StreamReader(fileStream))
      {
        string line;
        int lineNumber = 0;
        Store priorStore = null;
        while ((line = file.ReadLine()) != null)
        {
          if (line.Length == 0)
            continue;
          lineNumber++;
          char[] delimiters = new char[] { ',' };
          string[] parts = line.Split(delimiters);
          int i = 0;
          string advertiserId = parts[i++];
          if (i >= parts.Length)
            continue;
          string name = parts[i++].Trim('\"');
          name = name.Trim('\\');
          name = name.Trim('\"');
          if (i >= parts.Length || name.Length == 0)
            continue;
          string Three_Month_EPC = parts[i++];
          if (i >= parts.Length)
            continue;
          string Seven_Day_EPC = parts[i++];
          if (i >= parts.Length)
            continue;
          string Network_Ranking = parts[i++];
          if (i >= parts.Length)
            continue;
          string Date_Created = parts[i++];
          if (i >= parts.Length)
            continue;
          string hasProducts = parts[i++];
          if (i >= parts.Length)
            continue;
          string cjCategory = parts[i++].Trim('\"');
          if (i >= parts.Length)
            continue;
          string country = parts[i++].Trim('\"');
          //string Date_Created = parts[i++];
          //string currency = parts[i++];
          //string dummy = parts[i++];
          if (lineNumber > 1)
          {
            StoreCategory category = m_categorySet[CjCategoryToCategory(cjCategory)];
            if (m_storeSet.ContainsKey(name) == false)
            {
              Store store = new Store(name);
              session.Persist(store);
              priorStore = store;
              numberOfImports++;
              m_storeSet.Add(name, store);
              StoreInCategory storeInCategory = new StoreInCategory(store, category);
              session.Persist(storeInCategory);
              store.m_categoryList.Add(storeInCategory);
              category.storeInCategoryList.Add(storeInCategory);
            }
          }
        }
      }
      return numberOfImports;
    }

    public int ImportGooglePromosFromTextFile(Stream fileStream, SessionBase session)
    {
      int numberOfImports = 0;
      using (System.IO.StreamReader file = new System.IO.StreamReader(fileStream))
      {
        string line;
        int lineNumber = 0;
        while ((line = file.ReadLine()) != null)
        {
          lineNumber++;
          char[] delimiters = new char[] { '\t' };
          string[] parts = line.Split(delimiters);
          int i = 0;
          string name = parts[i++];
          if (i >= parts.Length)
            continue;
          string linkId = parts[i++];
          string description = parts[i++];
          if (parts.Length > i)
          {
            string MerchandisingText = parts[i++];
            string AltText = parts[i++];
            string promoStartDate = parts[i++];
            string promoEndDate = parts[i++];
            string trackingUrl = parts[i++];
            string imageUrl = parts[i++];
            string imageHeight = parts[i++];
            string imageWidth = parts[i++];
            string linkUrl = parts[i++];
            if (i >= parts.Length)
              continue;
            string promoType = parts[i++];
            string mercahntId = parts[i++];
            if (lineNumber > 1)
            {
              DateTime endDate = DateTime.MaxValue;
              DateTime startDate = DateTime.MinValue;
              if (promoStartDate.Length > 0)
                if (!DateTime.TryParse(promoStartDate, out startDate))
                  continue;
              if (promoEndDate.Length > 0)
                if (!DateTime.TryParse(promoEndDate, out endDate))
                  continue;
              Store store;
              if (this.m_storeSet.TryGetValue(name, out store))
              {
                numberOfImports++;
                StoreInCategory inCategory = store.m_categoryList[0];
                if (startDate < DateTime.Now && endDate > DateTime.Now)
                {
                  if (((inCategory.Click == null || inCategory.Click.Length == 0) && ((imageHeight.Contains("125") && imageWidth.Contains("125")) || imageWidth.Contains("120") || imageUrl == null)) || (imageHeight.Contains("125") && imageWidth.Contains("125")))
                  {
                    inCategory.Click = trackingUrl;
                    inCategory.Image = imageUrl;
                    store.Description = description;
                  }
                  StoreCoupons storeCoupons;
                  if (this.m_couponSet.TryGetValue(name, out storeCoupons) == false)
                  {
                    storeCoupons = new StoreCoupons(store, "", description, imageUrl, trackingUrl, 0);
                    this.m_couponSet.Add(name, storeCoupons);
                  }
                  if (imageUrl == null || imageUrl.Length == 0)
                  {
                    string code = null;
                    int index = description.ToLower().IndexOf("code :");
                    if (index >= 0)
                    {
                      index += 7;
                      code = "";
                      while (description.Length > index && description[index] != ' ' && description[index] != '.')
                        code += description[index++];
                    }
                    else if ((index = description.ToLower().IndexOf("code:")) >= 0)
                    {
                      index += 6;
                      code = "";
                      while (description.Length > index && description[index] != ' ' && description[index] != '.' && description[index] != '\"')
                        code += description[index++];
                    }
                    else if ((index = description.ToLower().IndexOf("code*:")) >= 0)
                    {
                      index += 7;
                      code = "";
                      while (description.Length > index && description[index] != ' ' && description[index] != '.' && description[index] != '\"')
                        code += description[index++];
                    }
                    else if ((index = description.ToLower().IndexOf("coupon:")) >= 0)
                    {
                      index += 7;
                      code = "";
                      while (description.Length > index && description[index] != ' ' && description[index] != '.' && description[index] != '\"')
                        code += description[index++];
                    }
                    else if ((index = description.ToLower().IndexOf("coupon \"")) >= 0)
                    {
                      index += 8;
                      code = "";
                      while (description.Length > index && description[index] != ' ' && description[index] != '.' && description[index] != '\"')
                        code += description[index++];
                    }
                    else
                    {
                      index = description.ToLower().IndexOf("code");
                      if (index >= 0)
                      {
                        index += 5;
                        code = "";
                        while (description.Length > index && description[index] != ' ' && description[index] != '.')
                          code += description[index++];
                      }
                    }
                    Coupon coupon = new Coupon(startDate, endDate, promoType == "Free Shipping" ? 1 : 2, description, code, trackingUrl, null);
                    for (i = 0; i < storeCoupons.couponList.Count; i++)
                    {
                      Coupon c = storeCoupons.couponList[i];
                      if ((c.Description == description && trackingUrl == c.Link && c.Code == code) || c.ExpireDate < DateTime.Now)
                      {
                        storeCoupons.couponList.RemoveAt(i);
                        c.Unpersist(session);
                      }
                    }
                    storeCoupons.couponList.Add(coupon);
                  }
                }
              }
            }
          }
        }
      }
      return numberOfImports;
    }

    public int ImportCjPromosFromTextFile(Stream fileStream, SessionBase session)
    {
      int numberOfImports = 0;
      using (System.IO.StreamReader file = new System.IO.StreamReader(fileStream))
      {
        int lineNumber = 0;
        CsvReader csvReader = new CsvReader(file, true);
        string[] fileldNames = csvReader.GetFieldHeaders();
        foreach (string[] record in csvReader)
        {
          lineNumber++;
          int i = 0;
          string name = record[i++];
          if (record.Length == i)
            continue;
          string programUrl = record[i++];
          if (record.Length == i)
            continue;
          string relation = record[i++];
          string details = record[i++];
          string aid = record[i++];
          string adName = record[i++];
          string description = record[i++];
          if (record.Length == i)
            continue;
          string linkType = record[i++];
          if (record.Length == i)
            continue;
          string trackingUrl = record[i++];
          string imageUrl = record[i++];
          string promoType = record[i++];
          string promoStartDate = record[i++];
          string promoEndDate = record[i++];
          string getLinkHtml = record[i++];
          DateTime endDate = DateTime.MaxValue;
          DateTime startDate = DateTime.MinValue;
          if (promoStartDate.Length > 0)
            if (!DateTime.TryParse(promoStartDate, out startDate))
              continue;
          if (promoEndDate.Length > 0)
            if (!DateTime.TryParse(promoEndDate, out endDate))
              continue;
          Store store;
          if (this.m_storeSet.TryGetValue(name, out store))
          {
            numberOfImports++;
            StoreInCategory inCategory = store.m_categoryList[0];
            if (startDate < DateTime.Now && endDate > DateTime.Now)
            {
              if ((inCategory.Click == null && (adName.Contains("125x125") || adName.Contains("120x90") || adName.Contains("120x60") || linkType == "Text Link")) || adName.Contains("125x125"))
              {
                inCategory.Click = trackingUrl;
                inCategory.Image = imageUrl;
                store.Description = description;
              }
              StoreCoupons storeCoupons;
              if (this.m_couponSet.TryGetValue(name, out storeCoupons) == false)
              {
                storeCoupons = new StoreCoupons(store, "", description, imageUrl, trackingUrl, 0);
                this.m_couponSet.Add(name, storeCoupons);
              }
              if (linkType == "Text Link")
              {
                string code = null;
                int index = description.ToLower().IndexOf("code :");
                if (index >= 0)
                {
                  index += 7;
                  code = "";
                  while (description.Length > index && description[index] != ' ' && description[index] != '.')
                    code += description[index++];
                }
                else if ((index = description.ToLower().IndexOf("code:")) >= 0)
                {
                  index += 6;
                  code = "";
                  while (description.Length > index && description[index] != ' ' && description[index] != '.' && description[index] != '\"')
                    code += description[index++];
                }
                else if ((index = description.ToLower().IndexOf("code*:")) >= 0)
                {
                  index += 7;
                  code = "";
                  while (description.Length > index && description[index] != ' ' && description[index] != '.' && description[index] != '\"')
                    code += description[index++];
                }
                else if ((index = description.ToLower().IndexOf("coupon:")) >= 0)
                {
                  index += 7;
                  code = "";
                  while (description.Length > index && description[index] != ' ' && description[index] != '.' && description[index] != '\"')
                    code += description[index++];
                }
                else if ((index = description.ToLower().IndexOf("coupon \"")) >= 0)
                {
                  index += 8;
                  code = "";
                  while (description.Length > index && description[index] != ' ' && description[index] != '.' && description[index] != '\"')
                    code += description[index++];
                }
                else
                {
                  index = description.ToLower().IndexOf("code");
                  if (index >= 0)
                  {
                    index += 5;
                    code = "";
                    while (description.Length > index && description[index] != ' ' && description[index] != '.')
                      code += description[index++];
                  }
                }
                Coupon coupon = new Coupon(startDate, endDate, promoType == "Free Shipping" ? 1 : 2, description, code, trackingUrl, null);
                for (i = 0; i < storeCoupons.couponList.Count; i++)
                {
                  Coupon c = storeCoupons.couponList[i];
                  if ((c.Description == description && trackingUrl == c.Link && c.Code == code) || c.ExpireDate < DateTime.Now)
                  {
                    storeCoupons.couponList.RemoveAt(i);
                    c.Unpersist(session);
                  }
                }
                storeCoupons.couponList.Add(coupon);
              }
            }
          }
        }
      }
      return numberOfImports;
    }
    public int ImportCjLinksFromCsvFile(Stream fileStream, SessionBase session)
    {
      int numberOfImports = 0;
      using (System.IO.StreamReader file = new System.IO.StreamReader(fileStream))
      {
        int lineNumber = 0;
        CsvReader csvReader = new CsvReader(file, true);
        string[] fileldNames = csvReader.GetFieldHeaders();
        foreach (string[] record in csvReader)
        {
          lineNumber++;
          int i = 0;
          string name = record[i++];
          string countries = record[i++];
          string linkId = record[i++];
          string adName = record[i++];
          string description = record[i++];
          string keywords = record[i++];
          string linkType = record[i++];
          string epc1 = record[i++];
          string epc2 = record[i++];
          string updateDate = record[i++];
          string trackingUrl = record[i++];
          string javaScript = record[i++];
          string clickUrl = record[i++];
          string promoType = record[i++];
          string couponCode = record[i++];
          string promoStartDate = record[i++];
          string promoEndDate = record[i++];
          string category = record[i++];
          string advCid = record[i++];
          string relation = record[i++];
          string language = record[i++];
          string mobileOptimized = record[i++];
          int imageUrlIndex = trackingUrl.IndexOf("<img s");
          string imageUrl = trackingUrl.Remove(0, imageUrlIndex + 10);
          imageUrl = imageUrl.Substring(0, imageUrl.IndexOf('"'));
          trackingUrl = "";
          DateTime endDate = DateTime.MaxValue;
          DateTime startDate = DateTime.MinValue;
          if (promoStartDate.Length > 0)
            if (!DateTime.TryParse(promoStartDate, out startDate))
              continue;
          if (promoEndDate.Length > 0)
            if (!DateTime.TryParse(promoEndDate, out endDate))
              continue;
          Store store;
          if (this.m_storeSet.TryGetValue(name, out store))
          {
            numberOfImports++;
            StoreInCategory inCategory = store.m_categoryList[0];
            if (startDate < DateTime.Now && endDate > DateTime.Now)
            {
              if ((inCategory.Click == null || inCategory.Click.Length == 0) && linkType == "Banner")
              {
                inCategory.Click = clickUrl;
                inCategory.Image = imageUrl;
                store.Description = description;
              }
              StoreCoupons storeCoupons;
              if (this.m_couponSet.TryGetValue(name, out storeCoupons) == false)
              {
                storeCoupons = new StoreCoupons(store, "", description, imageUrl, clickUrl, 0);
                this.m_couponSet.Add(name, storeCoupons);
              }
              if (linkType == "Text Link")
              {
                string code = null;
                int index = description.ToLower().IndexOf("code :");
                if (index >= 0)
                {
                  index += 7;
                  code = "";
                  while (description.Length > index && description[index] != ' ' && description[index] != '.')
                    code += description[index++];
                }
                else if ((index = description.ToLower().IndexOf("code:")) >= 0)
                {
                  index += 6;
                  code = "";
                  while (description.Length > index && description[index] != ' ' && description[index] != '.' && description[index] != '\"')
                    code += description[index++];
                }
                else if ((index = description.ToLower().IndexOf("code*:")) >= 0)
                {
                  index += 7;
                  code = "";
                  while (description.Length > index && description[index] != ' ' && description[index] != '.' && description[index] != '\"')
                    code += description[index++];
                }
                else if ((index = description.ToLower().IndexOf("coupon:")) >= 0)
                {
                  index += 7;
                  code = "";
                  while (description.Length > index && description[index] != ' ' && description[index] != '.' && description[index] != '\"')
                    code += description[index++];
                }
                else if ((index = description.ToLower().IndexOf("coupon \"")) >= 0)
                {
                  index += 8;
                  code = "";
                  while (description.Length > index && description[index] != ' ' && description[index] != '.' && description[index] != '\"')
                    code += description[index++];
                }
                else
                {
                  index = description.ToLower().IndexOf("code");
                  if (index >= 0)
                  {
                    index += 5;
                    code = "";
                    while (description.Length > index && description[index] != ' ' && description[index] != '.')
                      code += description[index++];
                  }
                }
                Coupon coupon = new Coupon(startDate, endDate, promoType == "Free Shipping" ? 1 : 2, description, code, clickUrl, null);
                for (i = 0; i < storeCoupons.couponList.Count; i++)
                {
                  Coupon c = storeCoupons.couponList[i];
                  if ((c.Description == description && clickUrl == c.Link && c.Code == code) || c.ExpireDate < DateTime.Now)
                  {
                    storeCoupons.couponList.RemoveAt(i);
                    c.Unpersist(session);
                  }
                }
                storeCoupons.couponList.Add(coupon);
              }
            }
          }
        }
      }
      return numberOfImports;
    }
    public int ImportStoresFromTextFile(Stream fileStream)
    {
      int numberOfImports = 0;
      using (System.IO.StreamReader file = new System.IO.StreamReader(fileStream))
      {
        string line;
        int lineNumber = 0;
        while ((line = file.ReadLine()) != null)
        {
          lineNumber++;
          char[] delimiters = new char[] { '\t' };
          string[] parts = line.Split(delimiters);
          int i = 0;
          string name = parts[i++];
          string description = "";
          string keyWords = "";
          string rating = "";
          string dateTimeCreated = DateTime.Now.ToString();
          string dateTimeUpdated = DateTime.Now.ToString();
          string isValid = "True";
          if (parts.Length > i)
          {
            description = parts[i++];
            keyWords = parts[i++];
            rating = parts[i++];
            dateTimeCreated = parts[i++];
            dateTimeUpdated = parts[i++];
            isValid = parts[i++];
          }
          if (lineNumber > 1)
          {
            if (this.m_storeSet.ContainsKey(name) == false)
            {
              Store store = new Store(name, description, keyWords, rating, dateTimeCreated, dateTimeUpdated, isValid);
              numberOfImports++;
              this.m_storeSet.Add(name, store);
            }
          }
        }
      }
      return numberOfImports;
    }

    public int ImportInCategoryFromTextFile(Stream fileStream)
    {
      char[] trimStartChars = new char[] { '\"' };
      int numberOfImports = 0;
      using (System.IO.StreamReader file = new System.IO.StreamReader(fileStream))
      {
        string line;
        int lineNumber = 0;
        while ((line = file.ReadLine()) != null)
        {
          lineNumber++;
          char[] delimiters = new char[] { '\t' };
          string[] parts = line.Split(delimiters);
          int i = 0;
          string storeStr = parts[i++];
          storeStr = storeStr.TrimStart(trimStartChars);
          storeStr = storeStr.TrimEnd(trimStartChars);
          string categoryStr = parts[i++];
          string click = parts[i++];
          string image = parts[i++];
          string specialImage = "";
          string script = "";
          string textLink = "";
          string textClick = "";
          string controlUrl = "";
          string createDateStr = null;
          string modifyDateStr = null;
          if (parts.Length > i)
          {
            specialImage = parts[i++];
            script = parts[i++];
            textLink = parts[i++];
            textClick = parts[i++];
            controlUrl = parts[i++];
            createDateStr = parts[i++];
            modifyDateStr = parts[i++];
          }
          if (lineNumber > 1)
          {
            Store store;
            if (!this.m_storeSet.TryGetValue(storeStr, out store))
            {
              store = new Store(storeStr, null, null, 0);
              m_storeSet.Add(storeStr, store);
            }
            StoreCategory category;
            DateTime createDate = createDateStr == null ? DateTime.Now : DateTime.Parse(createDateStr);
            DateTime modifyDate = modifyDateStr == null ? DateTime.Now : DateTime.Parse(modifyDateStr);
            if (!this.m_categorySet.TryGetValue(categoryStr, out category))
              throw new UnexpectedException("failed to find category: " + categoryStr);
            StoreInCategory inCategory = new StoreInCategory(store, click, image, specialImage, script, textLink, textClick, controlUrl, category, createDate, modifyDate);
            if (store.m_categoryList.Contains(inCategory) == false && this.m_inCategorySet.Contains(inCategory) == false)
            {
              numberOfImports++;
              this.m_inCategorySet.Add(inCategory);
              store.m_categoryList.Add(inCategory);
              inCategory.Category.storeInCategoryList.Add(inCategory);
            }
          }
        }
      }
      return numberOfImports;
    }

    public int ImportStoreCouponsFromTextFile(Stream fileStream)
    {
      int numberOfImports = 0;
      using (System.IO.StreamReader file = new System.IO.StreamReader(fileStream))
      {
        string line;
        int lineNumber = 0;
        while ((line = file.ReadLine()) != null)
        {
          lineNumber++;
          char[] delimiters = new char[] { '\t' };
          string[] parts = line.Split(delimiters);
          int i = 0;
          string storeStr = parts[i++];
          string fileName = parts[i++];
          string description = parts[i++];
          string image = parts[i++];
          string link = parts[i++];
          string stars = parts[i++];
          if (lineNumber > 1)
          {
            Store store;
            if (!this.m_storeSet.TryGetValue(storeStr, out store))
              continue;
            int intStars = 0;
            int.TryParse(stars, out intStars);
            StoreCoupons storeCoupons = new StoreCoupons(store, fileName, description, image, link, intStars);
            if (this.m_couponSet.Contains(storeStr) == false)
            {
              numberOfImports++;
              this.m_couponSet.Add(storeStr, storeCoupons);
            }
          }
        }
      }
      return numberOfImports;
    }

    public int ImportCouponsFromTextFile(Stream fileStream)
    {
      int numberOfImports = 0;
      using (System.IO.StreamReader file = new System.IO.StreamReader(fileStream))
      {
        string line;
        int lineNumber = 0;
        while ((line = file.ReadLine()) != null)
        {
          lineNumber++;
          char[] delimiters = new char[] { '\t' };
          string[] parts = line.Split(delimiters);
          int i = 0;
          string storeStr = parts[i++];
          string startDate = parts[i++];
          string exireDate = parts[i++];
          string promotionType = parts[i++];
          string description = parts[i++];
          string code = parts[i++];
          string link = parts[i++];
          string image = parts[i++];
          if (lineNumber > 1)
          {
            StoreCoupons storeCoupons;
            if (!this.m_couponSet.TryGetValue(storeStr, out storeCoupons))
              continue;
            DateTime expire;
            DateTime start;
            DateTime.TryParse(startDate, out start);
            DateTime.TryParse(exireDate, out expire);
            Coupon coupon = new Coupon(start, expire, promotionType, description, code, link, image);
            numberOfImports++;
            storeCoupons.couponList.Add(coupon);
          }
        }
      }
      return numberOfImports;
    }

    public bool HideClickUrls
    {
      get
      {
        return m_hideClickUrls;
      }
      set
      {
        Update();
        m_hideClickUrls = value;
      }
    }

    public bool HideImageUrls
    {
      get
      {
        return m_hideImageUrls;
      }
      set
      {
        Update();
        m_hideImageUrls = value;
      }
    }

    public void Upgrade()
    {
      Session.UpdateClass(this.GetType());
      UpdateTypeVersion();
      //m_invertedIndexId = m_invertedIndex.Id;
    }

    //public override CacheEnum Cache
    //{
    //  get
    //  {
    //    return CacheEnum.Yes;
    //  }
    //}
    public override void InitializeAfterRead(SessionBase session)
    {
      base.InitializeAfterRead(session);
    }
  }
}
#endif