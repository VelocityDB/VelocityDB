using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VelocityDB.Server;
namespace VelocityDBCoreServer.Controllers
{
  //[Authorize]
  [Route("[controller]")]
  [ApiController]
  public class ActiveController : VelocityDBControllerBaser
  {
    [AllowAnonymous]
    public ActionResult<IEnumerable<string>> Active()
    {
      return new ActionResult<IEnumerable<string>>(ServerStats.ActiveServerPaths);
    }
  }
}
