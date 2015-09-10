using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using VelocityDb;
using VelocityDb.Session;
using VelocityDBExtensions;

namespace WebApi.Controllers
{
  public class ObjectController : ApiController
  {
    public string Get(string path, UInt64 id)
    {
      using (SessionNoServer session = new SessionNoServer(path))
      {
        session.BeginRead();
        object obj = session.Open(id);
        JsonSerializerSettings jsonSettings = new JsonSerializerSettings();
        jsonSettings.TypeNameAssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Full;
        jsonSettings.TypeNameHandling = TypeNameHandling.All;
        jsonSettings.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
        jsonSettings.ContractResolver = new FieldsOnlyContractResolver();
        string json = JsonConvert.SerializeObject(obj, jsonSettings);
        session.Commit();
        return json;
      }
    }

    // POST api/object
    public void Post([FromBody]string value)
    {
    }

    // PUT api/object/5
    public void Put(int id, [FromBody]string value)
    {
    }

    // DELETE api/object/5
    public void Delete(int id)
    {
    }
  }
}
