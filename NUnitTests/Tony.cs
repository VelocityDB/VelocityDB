using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VelocityDb.Session;
using VelocityDbSchema.NUnit;

namespace NUnitTests
{
  [TestFixture]
  public class Tony
  {
    public const string systemDir = "c:/NUnitTestDbs";

    UInt64 _id;
    [Test]
    public void ListOfStrings()
    {
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        var l1 = new ValidNameList();
        l1.AddValidName(new ValidName("Kinga"));
        var names = new ValidNames("Mats", l1, new ValidNameList(), new ValidNameList());
        _id = session.Persist(names);
        session.Commit();
      }
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginRead();
        var names = session.Open<ValidNames>(_id);
        Assert.NotNull(names);
        session.Commit();
      }
    }
  }

  public class ValidNames : CommonBaseClass
  {
    private readonly string _nameOrigin;
    private readonly ValidNameList _validMaleNames;
    private readonly ValidNameList _validFemaleNames;
    private readonly ValidNameList _validFamilyNames;

    public ValidNames(string nameOrigin, ValidNameList validMaleNames, ValidNameList validFemaleNames, ValidNameList validFamilyNames)
    {
      _nameOrigin = nameOrigin;
      _validMaleNames = validMaleNames;
      _validFemaleNames = validFemaleNames;
      _validFamilyNames = validFamilyNames;
    }

    private static Random rnd = new Random();

    public string NameOrigin() => _nameOrigin;
    public ValidNameList ValidMaleNames() => _validMaleNames;

    //public ValidName SelectGivenName(GenderEnum gender)
    //{
    //  if (gender == GenderEnum.Male)
    //    return _validMaleNames.ValidNames().RandomSubset(1, rnd).First();
    //  else
    //    return _validFemaleNames.ValidNames().RandomSubset(1, rnd).First();
    //}


    //public ValidName SelectFamilyName()
    //{
    //  return _validFamilyNames.ValidNames().RandomSubset(1, rnd).First();
    //}
  }

  public class ValidNameList
  {

    private readonly List<ValidName> _validNames;


    public ValidNameList()
    {
      _validNames = new List<ValidName>();
    }

    public void AddValidName(ValidName validName)
    {
      _validNames.Add(validName);
    }

    public IEnumerable<ValidName> ValidNames() => _validNames;
  }

  public class ValidName
  {
    private readonly string _name;
    private readonly List<string> _nameAbbreviations;
    private readonly List<string> _nameHomonyms;

    public ValidName(string name)
    {
      _name = name;

      _nameAbbreviations = new List<string>();
      //     _nameHomonyms = new List<string>();
    }

    public override string ToString() => _name;
    public string Name() => _name;
    public IEnumerable<string> NameAbbreviations() => _nameAbbreviations;
    public IEnumerable<string> NameHomonyms() => _nameHomonyms;

    public void AddAbbreviation(string abbreviation)
    {
      _nameAbbreviations.Add(abbreviation);
    }

    public void AddHomonym(string homonym)
    {
      _nameHomonyms.Add(homonym);
    }
  }
}
