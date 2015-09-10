using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using VelocityDb;
using VelocityDb.Session;

namespace WebApi.Controllers
{
  public class DatabaseController : ApiController
  {
    /// <summary>
    /// Get a list of all <see cref="Database"/> used.
    /// </summary>
    /// <param name="path">Path to database directory on server relative to server setting <see cref="SessionBase.BaseDatabasePath"/></param>
    /// <returns>All databases in use</returns>
    public IEnumerable<string> Get(string path)
    {
      using (SessionNoServer session = new SessionNoServer(path))
      {
        session.BeginRead();
        List<Database> dbList = session.OpenAllDatabases();
        foreach (Database db in dbList)
          yield return db.ToString();
        session.Commit();
      }
    }

    // GET api/database/suppliertracking/15
    public string Get(string path, UInt32 id)
    {
      using (SessionNoServer session = new SessionNoServer(path))
      {
        session.BeginRead();
        Database db = session.OpenDatabase(id);
        string dbName = db.ToString();
        session.Commit();
        return dbName;
      }
    }

    // POST api/database
    public void Post([FromBody]string value)
    {
    }

    // PUT api/database/5
    public void Put(int id, [FromBody]string value)
    {
    }

    // DELETE api/database/5
    public void Delete(int id)
    {
    }
  }
}
