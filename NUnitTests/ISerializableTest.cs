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
  public class ISerializableTest
  {
    static readonly string s_windrive = System.IO.Path.GetPathRoot(Environment.SystemDirectory);
    static readonly string s_systemDir = System.IO.Path.Combine(s_windrive, "NUnitTestDbs");
    [Test]
    public void ModelInfo()
    {
      ModelInfo model = new ModelInfo();
      BomTable bt = new BomTable()
      {
        BomName = Guid.NewGuid().ToString(),
        Header = new List<string>() { Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString() },
        TotalCols = 2,
        TotalRow = 10,
        //Rows = new List<BomTableRow>()
      };

      //for (int i = 0; i < 10; i++)
      //{
      //    BomTableRow row = new BomTableRow();

      //    //for (int j = 0; j < 10; j++)
      //    //{
      //    //    row.Cells.Add("Value " + j.ToString());
      //    //}

      //    bt.Rows.Add(row);

      //}
      model.BomTables = new List<BomTable>();
      model.BomTables.Add(bt);

      try
      {
        using (SessionNoServerShared session = new SessionNoServerShared(s_systemDir))
        {
          session.BeginUpdate();
          //session.Persist(model.BomTables);
          session.Persist(model);
          session.Commit();
        }
      }
      catch (Exception ex)
      {
        System.Diagnostics.Trace.WriteLine(ex.ToString());
      }

      try
      {
        using (SessionNoServerShared session = new SessionNoServerShared(s_systemDir))
        {
          session.BeginRead();
          //var models1 = session.AllObjects<List<BomTable>>().ToList();
          var models = session.AllObjects<ModelInfo>().ToList();
          session.Commit();
        }
      }
      catch (Exception ex)
      {
        System.Diagnostics.Trace.WriteLine(ex.ToString());
      }

    }

  }
}
