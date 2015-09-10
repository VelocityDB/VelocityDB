using System;
using System.Data;
using System.Configuration;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

namespace VelocityWeb
{
  [System.ComponentModel.DataObject(true)]
  public class EditedVersion
  {
    public EditedVersion() { }

    public UInt64 Id { get; set; }
    public string Oid { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime ReleaseDate { get; set; }
  }
}