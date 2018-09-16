using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using VelocityDb.Session;

namespace VelocityDBCoreServer.Controllers
{
  public class VelocityDBControllerBaser : ControllerBase
  {
    protected JsonSerializerSettings _jsonSettings;
    public VelocityDBControllerBaser()
    {
      _jsonSettings = new JsonSerializerSettings();
      _jsonSettings.TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple;
      _jsonSettings.TypeNameHandling = TypeNameHandling.All;
      _jsonSettings.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
      _jsonSettings.ContractResolver = new FieldsOnlyContractResolver();
    }
    static Dictionary<string, SessionPool> SessionPools { get; } = new Dictionary<string, SessionPool>();

    public static SessionPool GetSessionPool(string path)
    {
      lock (SessionPools)
      {
        SessionPool pool;
        if (SessionPools.TryGetValue(path, out pool))
          return pool;
        pool = new SessionPool(1, () => new ServerClientSession(path, null, 2000, true, false, false, VelocityDb.CacheEnum.No));
        SessionPools.Add(path, pool);
        return pool;
      }
    }

    public static void UpdateFields(object to, object from)
    {
      Type t = to.GetType();
      var fields = t.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
      foreach (var field in fields)
      {
        if (!field.IsNotSerialized)
        {
          var newValue = field.GetValue(from);
          var oldValue = field.GetValue(to);
          if (newValue != oldValue)
          {
            field.SetValue(to, newValue);
          }
        }
      }
    }
  }
}
