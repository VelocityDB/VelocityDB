using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace VelocityDBCoreServer
{
  public class FieldsOnlyContractResolver : DefaultContractResolver
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
