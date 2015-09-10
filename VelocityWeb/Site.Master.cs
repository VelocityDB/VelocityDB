using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace VelocityWeb
{
  public partial class Site : System.Web.UI.MasterPage
  {
    protected void Page_Load(object sender, EventArgs e)
    {
      if (HttpContext.Current.Request.Url.ToString().ToLower().Contains("graph"))
      {
        VelocityLogo.ImageUrl = "~/images/VelocityGraph.png";
        SiteName.Text = "VelocityGraph";
        description.Text = "<meta name=\"description\" content=\"VelocityGraph is a fast, scalable and easy to use NoSQL hybrid Graph Database and Object Database\">";
        keywords.Text = "<meta name=\"keywords\" content=\"C# graph database,.net database,linq,velocity,db,database,object persistence,object database,database management,data management,prism,distributed data,graph database,relationship analytics,simulation,noSQL,social network analysis,embedded database,real-time database, key value store, BTree, BTreeMap\">";
      }
      else
      {
        description.Text = "<meta name=\"description\" content=\"VelocityDB is an easy to use, extremely high performance with indexes, scalable (Yottabytes and trillions of objects), embeddable and distributable object database system for C# .NET applications with a small footprint (~ 400KB)\">";
        keywords.Text = "<meta name=\"keywords\" content=\"C# database,.net database,linq,velocity,db,database,object persistence,object database,prism,database management,data management,distributed data,graph database,relationship analytics,simulation,noSQL,social network analysis,embedded database,real-time database, key value store, BTree, BTreeMap\">";
      }
    }
  }
}