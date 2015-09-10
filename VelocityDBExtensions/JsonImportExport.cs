using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using VelocityDb;
using VelocityDb.Session;

namespace VelocityDBExtensions
{
  public static class JsonImportExport
  {
    static public T ImportJson<T>(this SessionBase session, string json)
    {
      return JsonConvert.DeserializeObject<T>(json);
    }

    static public string ExportToJson<T>(this SessionBase session, Oid oid)
    {
      return ExportToJson<T>(session, oid.Id);
    }

    static public string ExportToJson<T>(this SessionBase session, UInt64 id)
    {
      object obj = session.Open(id);
      JsonSerializerSettings jsonSettings = new JsonSerializerSettings();
      jsonSettings.TypeNameAssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Full;
      jsonSettings.TypeNameHandling = TypeNameHandling.All;
      jsonSettings.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
      jsonSettings.ContractResolver = new FieldsOnlyContractResolver();
      return JsonConvert.SerializeObject(obj, jsonSettings);
    }

    static public IEnumerable<string> ExportToJson<T>(this SessionBase session, bool includeSubclasses = false, bool databasePerType = true)
    {
      JsonSerializerSettings jsonSettings = new JsonSerializerSettings();
      jsonSettings.TypeNameAssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Full;
      jsonSettings.TypeNameHandling = TypeNameHandling.All;
      jsonSettings.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
      jsonSettings.ContractResolver = new FieldsOnlyContractResolver();
      var e = session.AllObjects<T>(includeSubclasses, databasePerType);
      foreach (T t in e)
      {
        yield return JsonConvert.SerializeObject(t, jsonSettings);
      }
    }
  }

  public class FieldsOnlyContractResolver : Newtonsoft.Json.Serialization.DefaultContractResolver
  {
    protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
    {
      var props = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
        .Select(f => base.CreateProperty(f, memberSerialization)).ToList();
      props.ForEach(p => { p.Writable = true; p.Readable = true; });
      return props;
    }
  }
}
