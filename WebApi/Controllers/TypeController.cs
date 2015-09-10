using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using VelocityDb.TypeInfo;
using VelocityDb.Session;
using VelocityDBExtensions;
using VelocityDb;
using Newtonsoft.Json;

namespace WebApi.Controllers
{
  /// <summary>
  /// Handles messages with url "api/type"
  /// </summary>
  public class TypeController : ApiController
  {
    /// <summary>
    /// Get the names of all persitent types used.
    /// </summary>
    /// <param name="path">Path to database directory on server relativer to server setting <see cref="SessionBase.BaseDatabasePath"/></param>
    /// <returns>All type names registered in the database schema</returns>
    public IEnumerable<string> Get(string path)
    {
      using (SessionNoServer session = new SessionNoServer(path))
      {
        session.BeginRead();
        Database db = session.OpenDatabase(1);
        var e = db.AllObjects<VelocityDbType>(false);
        var types = session.ExportToJson<VelocityDbType>(false, false);
        List<string> stringList = new List<String>();
        foreach (VelocityDbType t in e)
          yield return t.Type.ToGenericTypeString();
        session.Commit();
      }
    }

    // POST api/type
    public void Post([FromBody]string value)
    {
    }

    // PUT api/type/5
    public void Put(int id, [FromBody]string value)
    {
    }

    // DELETE api/type/5
    public void Delete(int id)
    {
    }
  }
}
