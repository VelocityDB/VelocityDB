using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using VelocityDb.Session;
using VelocityGraph;

namespace WebApi.Controllers
{
  public class GraphController : ApiController
  {
    public string Get(string path, int id = 0)
    {
      using (SessionNoServer session = new SessionNoServer(path))
      {
        session.BeginRead();
        Graph graph = Graph.Open(session, id);
        using (MemoryStream ms = new MemoryStream())
        {
          graph.ExportToGraphJson(ms);
          session.Commit();
          return Encoding.UTF8.GetString(ms.ToArray());
        }
      }
    }

    // GET api/graph/5
    public string Get(int id)
    {
      return "value";
    }

    // POST api/graph
    public void Post(string path, int id, [FromBody]string json)
    {
      using (SessionNoServer session = new SessionNoServer(path))
      {
        session.BeginUpdate();
        Graph graph = Graph.Open(session, id);
        if (graph == null)
          graph = new Graph(session);
        else
          graph.Clear();
        using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
        {
          graph.ImportGraphJson(ms);
          session.Commit();
        }
      }
    }

    // PUT api/graph/5
    public void Put(int id, [FromBody]string value)
    {
    }

    // DELETE api/graph/5
    public void Delete(int id)
    {
    }
  }
}
