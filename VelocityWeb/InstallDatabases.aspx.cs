using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using VelocityDb;
using VelocityDb.Collection;
using VelocityDb.Session;
using VelocityDbSchema.VelocityWeb;

namespace VelocityWeb
{
  public partial class InstallDatabases : System.Web.UI.Page
  {
    protected void Page_Load(object sender, EventArgs e)
    {

    }

    protected void DoItClick(object sender, EventArgs e)
    {
      if (DoUpdatePassword != null && DoUpdatePassword.Text == "mysecret")
      {
        int step = 0;
        try
        {
          string updatedHostName = Dns.GetHostName().ToLower();
          if (HostName.Text != null && HostName.Text.Length > 0)
            updatedHostName = HostName.Text.ToLower();
          using (SessionNoServer session = new SessionNoServer(MapPath("~/Database").ToLower(), 2000, false, false))
          {
            session.BeginUpdate(false);
            DatabaseLocation bootLocation = session.DatabaseLocations.LocationForDb(0);
            DatabaseLocation locationNew = new DatabaseLocation(updatedHostName, MapPath("~/Database").ToLower(), bootLocation.StartDatabaseNumber, bootLocation.EndDatabaseNumber, session,
                bootLocation.CompressPages, bootLocation.PageEncryption, bootLocation.IsBackupLocation, bootLocation.BackupOfOrForLocation);
            bootLocation = session.NewLocation(locationNew);
            session.Commit(false);
            step++;
          }
          using (SessionNoServer session = new SessionNoServer(MapPath("~/IssuesDatabase").ToLower(), 2000, false, false))
          {
            session.BeginUpdate(false);
            DatabaseLocation bootLocation = session.DatabaseLocations.LocationForDb(0);
            DatabaseLocation locationNew = new DatabaseLocation(updatedHostName, MapPath("~/IssuesDatabase").ToLower(), bootLocation.StartDatabaseNumber, bootLocation.EndDatabaseNumber, session,
                bootLocation.CompressPages, bootLocation.PageEncryption, bootLocation.IsBackupLocation, bootLocation.BackupOfOrForLocation);
            bootLocation = session.NewLocation(locationNew);
            session.Commit(false);
            step++;
            Results.Text = locationNew.HostName + "@" + locationNew.DirectoryPath;
          }
        }
        catch (System.Exception ex)
        {
          Results.Text = "In step: " + step + " " + ex.ToString();
        }
      }
    }
  }
}