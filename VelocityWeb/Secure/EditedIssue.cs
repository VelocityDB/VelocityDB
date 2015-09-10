using System;
using System.Data;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using VelocityDbSchema.Tracker;
using VelocityDb.Collection;

namespace VelocityWeb
{
  [System.ComponentModel.DataObject(true)]
  public class EditedIssue
  {
    public EditedIssue() {}
    public User AssignedTo { get; set; }
    public Attachment[] AttachmentArray { get; set; }
    public DateTime DateTimeCreated { get; set; }
    public DateTime DateTimeLastUpdated { get; set; }
    public string Description { get; set; }
    public DateTime DueDate { get; set; }
    public string Environment { get; set; }
    public string FixMessage { get; set; }
    public Issue.Resolution FixResolution { get; set; } 
    public UInt64 Id { get; set; }
    public User LastUpdatedBy { get; set; }
    public string Oid { get; set; }
    public User ReportedBy { get; set; }
    public Issue.StatusEnum Status { get; set; }
    public string Summary { get; set; }
  }
}
