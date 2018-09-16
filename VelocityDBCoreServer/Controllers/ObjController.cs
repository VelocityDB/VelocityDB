using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using VelocityDb;
using VelocityDb.Session;
namespace VelocityDBCoreServer.Controllers
{
  //[Authorize]
  [Route("[controller]")]
  [ApiController]
  public class ObjController : VelocityDBControllerBaser
  {
    [AllowAnonymous]
    [HttpGet]
    public object Get([BindRequired, FromQuery] string path, [BindRequired, FromQuery] string id)
    {
      if (path == null)
        return "Path no set";
      if (id == null)
        return "object Id not specified (as string)";

      try
      {
        var pool = GetSessionPool(path);
        int sessionId = -1;
        SessionBase session = null;
        try
        {
          session = pool.GetSession(out sessionId);
          UInt64 Id;
          if (id.Contains('-'))
            Id = Oid.IdFromString(id);
          else
            UInt64.TryParse(id, out Id);
          if (Id != 0)
          {
            session.BeginRead();
            var obj = session.Open<IOptimizedPersistable>(Id);
            session.Commit();
            if (obj == null)
              return $"object with id {id} does not exist";
            return new TypePlusObj(obj);
          }
          return null;
        }
        finally
        {
          if (session != null)
            pool.FreeSession(sessionId, session);
        }
      }
      catch (Exception ex)
      {
        return ex.Message;
      }
    }

    [AllowAnonymous]
    [HttpPost]
    public ActionResult Put([BindRequired, FromQuery] string path, [FromBody] TypePlusObj obj)
    {
      if (path == null)
        return BadRequest("path not set");

      try
      {
        var pool = GetSessionPool(path);
        int sessionId = -1;
        SessionBase session = null;
        try
        {
          session = pool.GetSession(out sessionId);
          if (session.InTransaction)
            session.Abort();
          session.BeginUpdate();
          var id = Oid.IdFromString(obj.Id);
          OptimizedPersistable pObj = null;
          if (id != 0)
            pObj = session.Open<OptimizedPersistable>(id);
          string objAsString = obj.Obj.ToString();
          var pObj2 = JsonConvert.DeserializeObject(objAsString, obj.Type, _jsonSettings);
          if (pObj != null)
          {
            pObj.Update();
            UpdateFields(pObj, pObj2);
            session.Commit();
            return Ok($"{obj.Id} updated");
          }
          else if (pObj2 != null)
          {
            var pId = session.Persist(pObj2);
            session.Commit();
            return Ok(new Oid(pId).ToString());
          }
          session.Abort();
          return BadRequest($"Failed to deserialize json to object of type: {obj.Type}");
        }
        catch(Exception ex)
        {
          session.Abort();
          return BadRequest(ex.Message);
        }
        finally
        {
          if (session != null)
            pool.FreeSession(sessionId, session);
        }
      }
      catch (Exception ex)
      {
        return BadRequest(ex.Message);
      }
    }
  }

  public class TypePlusObj
  {
    public Type Type;
    public string Id;
    public dynamic Obj;
    public TypePlusObj() { }
    public TypePlusObj(IOptimizedPersistable o)
    {
      Type = o.GetType();
      Id = new Oid(o.Id).ToString();
      Obj = o;
    }
  }
}
