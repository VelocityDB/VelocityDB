using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace VelocityWeb
{
  public partial class About : System.Web.UI.Page
  {
    protected void Page_Load(object sender, EventArgs e)
    {
      month.Text = DateTime.Now.ToLongDateString();
      if (!IsPostBack)
      {
        Page.Header.Title = "VelocityWeb - About";
        Menu mainMenu = (Menu)Master.FindControl("NavigationMenu");
        HtmlGenericControl menu = (HtmlGenericControl)Master.FindControl("liAbout");
        menu.Attributes.Add("class", "active");
      }
    }
  }
}