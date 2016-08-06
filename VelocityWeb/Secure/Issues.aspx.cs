using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using VelocityDb;
using VelocityDb.Collection;
using VelocityDb.Session;
using VelocityDbSchema.VelocityWeb;
using VelocityDbSchema.Tracker;
using System.Web.UI.HtmlControls;

namespace VelocityWeb.Secure
{
  public partial class Issues : System.Web.UI.Page
  {
    static readonly string s_dataPath = HttpContext.Current.Server.MapPath("~/IssuesDatabase");
    static SessionPool s_sessionPool = new SessionPool(1, () => new SessionNoServer(s_dataPath));
    static SessionNoServerShared s_sharedReadOnlySession = new SessionNoServerShared(s_dataPath);
    private string viewString;
    protected void Page_Load(object sender, EventArgs e)
    {
      if (!IsPostBack)
      {
        Response.Cache.SetCacheability(HttpCacheability.NoCache);
        Page.Header.Title = "VelocityWeb - Issue Tracking";
        HtmlGenericControl menu = (HtmlGenericControl)Master.FindControl("liIssues");
        menu.Attributes.Add("class", "active");
        IssueTracker bugTracker = null;
        User user = null;
        int sessionId = -1;
        SessionBase session = null;
        try
        {
          session = s_sessionPool.GetSession(out sessionId);
          using (var transaction = session.BeginUpdate())
          {
            bugTracker = session.AllObjects<IssueTracker>(false).FirstOrDefault();
            if (bugTracker == null)
            {
              bugTracker = new IssueTracker(10, session);
              session.Persist(bugTracker);
              user = lookupUser(bugTracker, session);
              PermissionScheme permissions = new PermissionScheme(user);
              bugTracker.Permissions = permissions;
              createInitialObjects(bugTracker, user, session);
            }
            else
              user = lookupUser(bugTracker, session);

            transaction.Commit();
            s_sharedReadOnlySession.ForceDatabaseCacheValidation();
          }
        }
        catch (Exception ex)
        {
          Console.Out.WriteLine(ex.StackTrace);
        }
        finally
        {
          s_sessionPool.FreeSession(sessionId, session);
        }
        if (Session["filterById"] == null)
        {
          Session["filterById"] = false;
          Session["priorityFilter"] = new List<int>();
          Session["statusFilter"] = new List<int>();
          Session["categoryFilter"] = new List<int>();
        }
        viewString = Request.QueryString["view"];
        int view = 1;
        if (viewString != null)
          view = int.Parse(viewString);
        switch (view)
        {
          case 0:
            Session["UpdatedAttachments"] = null;
            IssueDetailsView.ChangeMode(DetailsViewMode.Insert);
            IssuesGridView.SelectedIndex = -1;
            IssueDetailsView.InsertItem(false);
            IssueDetailsView.Visible = true;
            IssueDetailsView.DataBind();
            MultiView1.SetActiveView(IssueEdit);
            break;
          case 1:
            Session["filterOn"] = false;
            MultiView1.SetActiveView(IssuesView);
            break;
          case 2:
            bindFilterRanges(bugTracker);
            object sessionState = Session["filterById"];
            bool filterById = (bool)sessionState;
            TreeNode root = IssueFilterTree.Nodes[0];
            TreeNode id = root.ChildNodes[0];
            id.Checked = filterById;
            MultiView1.SetActiveView(IssuesFilter);
            break;
          case 3:
            if (bugTracker.Permissions.AdminSet.Contains(user) == false)
              MultiView1.SetActiveView(IssuesView);
            else
              MultiView1.SetActiveView(ProjectsView);
            break;
          case 4:
            if (bugTracker.Permissions.AdminSet.Contains(user) == false)
            {
              errorLabel.Text = "Sorry, you don't have permission to create/edit/delete components";
              MultiView1.SetActiveView(IssuesView);
            }
            else
              MultiView1.SetActiveView(ComponentsView);
            break;
          case 5:
            if (bugTracker.Permissions.AdminSet.Contains(user) == false)
            {
              errorLabel.Text = "Sorry, you don't have permission to create/edit/delete users";
              MultiView1.SetActiveView(IssuesView);
            }
            else
              MultiView1.SetActiveView(UsersView);
            break;
          case 6:
            if (bugTracker.Permissions.AdminSet.Contains(user) == false)
            {
              errorLabel.Text = "Sorry, you don't have permission to create/edit/delete versions";
              MultiView1.SetActiveView(IssuesView);
            }
            else
              MultiView1.SetActiveView(VersionsView);
            break;
          case 7:
            if (bugTracker.Permissions.AdminSet.Contains(user) == false)
            {
              errorLabel.Text = "Sorry, you don't have permission to create/edit/delete permissions";
              MultiView1.SetActiveView(IssuesView);
            }
            else
            {
              AdminUsers.DataSource = bugTracker.Permissions.AdminSet.Keys;
              DeveloperUsers.DataSource = bugTracker.Permissions.DeveloperSet.Keys;
              RegularUsers.DataSource = bugTracker.UserSet.Keys;
              AdminUsers.DataBind();
              DeveloperUsers.DataBind();
              RegularUsers.DataBind();
              MultiView1.SetActiveView(PermissionsView);
            }
            break;
        }
      }
    }

    void createInitialObjects(IssueTracker issueTracker, User user, SessionBase session)
    {
      Project project = new Project(user, "VelocityDB", "Object Database Management System");
      session.Persist(project);
      issueTracker.ProjectSet.Add(project);

      Component webSite = new Component(user, "Web Site", "VelocityDB.com", project);
      session.Persist(webSite);
      issueTracker.ComponentSet.Add(webSite);

      Component samples = new Component(user, "Samples", "Samples applications provided", project);
      session.Persist(samples);
      issueTracker.ComponentSet.Add(samples);

      Component collections = new Component(user, "Collections", "Any of the collection classes", project);
      session.Persist(collections);
      issueTracker.ComponentSet.Add(collections);

      Component performance = new Component(user, "Performance", "Any performance issue", project);
      session.Persist(performance);
      issueTracker.ComponentSet.Add(performance);

      ProductVersion version = new ProductVersion(user, "5.0.16", "First initial version", new DateTime(2015, 11, 29));
      session.Persist(version);
      issueTracker.VersionSet.Add(version);
      version = new ProductVersion(user, "4.7", "June 13 version", new DateTime(2015, 06, 13));
      session.Persist(version);
      issueTracker.VersionSet.Add(version);
    }

    void bindFilterRanges(IssueTracker bugTracker)
    {
      var issueQuery = from issue in bugTracker.IssueSetById.Keys orderby issue.Id select issue;
      int ct = issueQuery.Count();
      IdFromDropDownList.DataSource = issueQuery;
      IdToDropDownList.DataSource = issueQuery;
      IdToDropDownList.SelectedIndex = ct - 1;
      IdFromDropDownList.DataBind();
      IdToDropDownList.DataBind();
      Session["idFrom"] = IdFromDropDownList.SelectedValue;
      Session["idTo"] = IdToDropDownList.SelectedValue;

      var issueQuery2 = from issue in bugTracker.IssueSetById.Keys orderby issue.DateTimeCreated select issue.DateTimeCreated;
      int numberOfDateTimeCreatedInUse = issueQuery2.Count(p => p != null);
      issueQuery2 = issueQuery2.Distinct();
      DateTimeCreatedFromDropDownList.DataSource = issueQuery2;
      DateTimeCreatedToDropDownList.DataSource = issueQuery2;
      DateTimeCreatedToDropDownList.SelectedIndex = numberOfDateTimeCreatedInUse - 1;
      DateTimeCreatedFromDropDownList.DataBind();
      DateTimeCreatedToDropDownList.DataBind();

      var issueQuery3 = from issue in bugTracker.IssueSetById.Keys orderby issue.ReportedBy select issue.ReportedBy;
      issueQuery3 = issueQuery3.Distinct();
      int numberOfReportedByInUse = issueQuery3.Count(p => p != null);
      ReportedByFromDropDownList.DataSource = issueQuery3;
      ReportedByToDropDownList.DataSource = issueQuery3;
      ReportedByToDropDownList.SelectedIndex = numberOfReportedByInUse - 1;
      ReportedByFromDropDownList.DataBind();
      ReportedByToDropDownList.DataBind();

      var issueQuery4 = from issue in bugTracker.IssueSetById.Keys orderby issue.DateTimeLastUpdated select issue.DateTimeLastUpdated;
      issueQuery4 = issueQuery4.Distinct();
      int numberOfDateTimeLastUpdatedInUse = issueQuery4.Count(p => p != null);
      DateTimeLastUpdatedFromDropDownList.DataSource = issueQuery4;
      DateTimeLastUpdatedToDropDownList.DataSource = issueQuery4;
      DateTimeLastUpdatedToDropDownList.SelectedIndex = numberOfDateTimeLastUpdatedInUse - 1;
      DateTimeLastUpdatedFromDropDownList.DataBind();
      DateTimeLastUpdatedToDropDownList.DataBind();

      var issueQuery5 = from issue in bugTracker.IssueSetById.Keys orderby issue.LastUpdatedBy select issue.LastUpdatedBy;
      issueQuery5 = issueQuery5.Distinct();
      int numberOfLastUpdatedByInUse = issueQuery5.Count(p => p != null);
      LastUpdatedByFromDropDownList.DataSource = bugTracker.UserSet.Keys;
      LastUpdatedByToDropDownList.DataSource = issueQuery5;
      LastUpdatedByToDropDownList.SelectedIndex = numberOfLastUpdatedByInUse - 1;
      LastUpdatedByFromDropDownList.DataBind();
      LastUpdatedByToDropDownList.DataBind();

      var issueQuery6 = (from issue in bugTracker.IssueSetById.Keys orderby issue.AssignedTo select issue).ToList();
      CompareAssignedTo compareAssignedTo = new CompareAssignedTo();
      issueQuery6 = issueQuery6.Distinct(compareAssignedTo).ToList();
      int numberOfAssignedToInUse = issueQuery6.Count(p => p != null);
      AssignedToFromDropDownList.DataSource = issueQuery6;
      AssignedToToDropDownList.DataSource = issueQuery6;
      AssignedToToDropDownList.SelectedIndex = numberOfAssignedToInUse - 1;
      AssignedToToDropDownList.DataBind();
      AssignedToFromDropDownList.DataBind();

      var issueQuery7 = from issue in bugTracker.IssueSetById.Keys orderby issue.Project select issue.Project;
      issueQuery7 = issueQuery7.Distinct();
      int numberOfProjectsInUse = issueQuery7.Count(p => p != null);
      ProjectFromDropDownList.DataSource = issueQuery7;
      ProjectToDropDownList.DataSource = issueQuery7;
      ProjectToDropDownList.SelectedIndex = numberOfProjectsInUse - 1;
      ProjectFromDropDownList.DataBind();
      ProjectToDropDownList.DataBind();

      var issueQuery8 = from issue in bugTracker.IssueSetById.Keys orderby issue.Project, issue.Component select issue.Component;
      issueQuery8 = issueQuery8.Distinct();
      int numberOfComponentsInUse = issueQuery8.Count(p => p != null);
      ComponentFromDropDownList.DataSource = issueQuery8;
      ComponentToDropDownList.DataSource = issueQuery8;
      ComponentToDropDownList.SelectedIndex = numberOfComponentsInUse - 1;
      ComponentFromDropDownList.DataBind();
      ComponentToDropDownList.DataBind();

      var issueQuery9 = from issue in bugTracker.IssueSetById.Keys where issue.Version != null orderby issue.Version select issue.Version;
      issueQuery9 = issueQuery9.Distinct();
      int numberOfVersionsInUse = issueQuery9.Count(p => p != null);
      VersionFromDropDownList.DataSource = issueQuery9;
      VersionToDropDownList.DataSource = issueQuery9;
      VersionToDropDownList.SelectedIndex = numberOfVersionsInUse - 1;
      VersionFromDropDownList.DataBind();
      VersionToDropDownList.DataBind();
    }

    public System.Array AllResolution()
    {
      return Enum.GetValues(typeof(Issue.Resolution));
    }

    public System.Array AllStatus()
    {
      return Enum.GetValues(typeof(Issue.StatusEnum));
    }

    public System.Array AllCategory()
    {
      return Enum.GetValues(typeof(Issue.CategoryEnum));
    }

    public System.Array AllPriority()
    {
      return Enum.GetValues(typeof(Issue.PriorityEnum));
    }

    public IList<User> AllUsers(string sortExpression = "")
    {
      SortedSetAny<User> userSet;
      try
      {
        var bugTracker = s_sharedReadOnlySession.AllObjects<IssueTracker>(false).FirstOrDefault();
        if (bugTracker == null)
          return null;
        userSet = bugTracker.UserSet;
      }
      catch (System.Exception ex)
      {
        Console.WriteLine(ex.ToString());
        return null;
      }
      if (sortExpression.Length == 0)
        return userSet.Keys;
      else
      {
        string[] selectAndDir = sortExpression.Split(' ');
        int sorting = int.Parse(selectAndDir[0]);
        bool reverse = selectAndDir.Length > 1;
        IList<User> users = userSet.Keys;
        switch (sorting)
        {
          case 1: users = (from user in users orderby user.FirstName select user).ToList();
            break;
          case 2: users = (from user in users orderby user.LastName select user).ToList();
            break;
          case 3: users = (from user in users orderby user.Email select user).ToList();
            break;
          case 4: users = (from user in users orderby user.UserName select user).ToList();
            break;
        }
        if (reverse)
        {
          List<User> tempList = new List<User>(users.Count);
          IEnumerable<User> temp = users.Reverse<User>();
          foreach (User user in temp)
          {
            tempList.Add(user);
          }
          return tempList;
        }
        return users;
      }
    }

    public IList<Issue> AllIssues()
    {
      try
      {
        IssueTracker bugTracker = s_sharedReadOnlySession.AllObjects<IssueTracker>(false).FirstOrDefault();
        IList<Issue> issueSet = bugTracker.IssueSetById.Keys;
        return issueSet;
      }
      catch (System.Exception ex)
      {
        this.errorLabel.Text = ex.ToString();
      }
      return null;
    }

    void setSortArrow(int sorting, bool reverse)
    {
      for (int i = 0; i < IssuesGridView.Columns.Count; i++)
      {
        IssuesGridView.Columns[1].HeaderStyle.CssClass = "unsorted";
      }
      if (reverse)
        IssuesGridView.Columns[sorting].HeaderStyle.CssClass = "sortedASC";
      else
        IssuesGridView.Columns[sorting].HeaderStyle.CssClass = "sortedDESC";
    }

    public ICollection<Issue> AllIssues(string sortExpression)
    {
      try
      {
        IssueTracker bugTracker = s_sharedReadOnlySession.AllObjects<IssueTracker>(false).FirstOrDefault();
        if (bugTracker != null)
        {
          ICollection<Issue> issueSet = bugTracker.IssueSetById.Keys;
          if (sortExpression.Length > 0)
          {
            string[] selectAndDir = sortExpression.Split(' ');
            int sorting = int.Parse(selectAndDir[0]);
            bool reverse = selectAndDir.Length > 1;
            switch (sorting)
            {
              case 1: issueSet = bugTracker.IssueSetByPriority.Keys;
                break;
              case 2: issueSet = bugTracker.IssueSetByDateTimeCreated.Keys;
                break;
              case 3: issueSet = bugTracker.IssueSetByStatus.Keys;
                break; // do sorting by using LINQ since we did not create a sorted collection sorted by FixResolution
              case 4: issueSet = (from issue in issueSet orderby issue.FixResolution select issue).ToList();
                break;
              case 5: issueSet = bugTracker.IssueSetBySummary.Keys;
                break;
              case 6: // do sorting by using LINQ since we did not create a sorted collection sorted by component
                issueSet = (from issue in issueSet orderby issue.Project, issue.Component select issue).ToList();
                break;
              case 7: issueSet = bugTracker.IssueSetByVersion.Keys;
                break;
              case 8: issueSet = bugTracker.IssueSetByCategory.Keys;
                break;
              case 9: issueSet = bugTracker.IssueSetByReportedBy.Keys;
                break;
              case 10: issueSet = bugTracker.IssueSetByLastUpdatedBy.Keys;
                break;
              case 11: issueSet = bugTracker.IssueSetByLastUpdatedBy.Keys;
                break;
              case 12:
                if (bugTracker.IssueSetByAssignedTo.Keys.Count > 0)
                  issueSet = bugTracker.IssueSetByAssignedTo.Keys;
                break;
              case 13: issueSet = bugTracker.IssueSetByDueDate.Keys;
                break;

            }
            if (reverse)
            {
              List<Issue> tempList = new List<Issue>(issueSet.Count);
              IEnumerable<Issue> temp = issueSet.Reverse<Issue>();
              foreach (Issue issue in temp)
              {
                tempList.Add(issue);
              }
              issueSet = tempList;
            }
            //setSortArrow(sorting, reverse);
          }
          //object viewState = ViewState["filterById"];
          object filterOnObject = Session["filterOn"];
          if (filterOnObject != null && (bool)filterOnObject)
          {
            object sessionState = Session["filterById"];
            bool filterById = (bool)sessionState;
            if (filterById)
            {
              string idFrom = (string)Session["idFrom"];
              if (idFrom.Length > 0)
              {
                UInt64 startId = UInt64.Parse(idFrom);
                UInt64 endId = UInt64.Parse((string)Session["idTo"]);
                issueSet = (from issue in issueSet where issue.Id >= startId && issue.Id <= endId select issue).ToList();
              }
            }
            List<int> priorityNumList = (List<int>)Session["priorityFilter"];
            if (priorityNumList != null && priorityNumList.Count > 0)
            {
              issueSet = (from issue in issueSet where priorityNumList.Contains((int)issue.Priority) select issue).ToList();
            }
            List<int> statusNumList = (List<int>)Session["statusFilter"];
            if (statusNumList != null && statusNumList.Count > 0)
            {
              issueSet = (from issue in issueSet where statusNumList.Contains((int)issue.Status) select issue).ToList();
            }
            List<int> categoryNumList = (List<int>)Session["categoryFilter"];
            if (categoryNumList.Count > 0)
            {
              issueSet = (from issue in issueSet where categoryNumList.Contains((int)issue.Category) select issue).ToList();
            }
          }
          return issueSet;
        }
      }
      catch (System.Exception ex)
      {
        this.errorLabel.Text = ex.ToString();
      }
      return null;
    }

    public ICollection<Project> AllProjects(string sortExpression = "")
    {
      try
      {
        IssueTracker bugTracker = s_sharedReadOnlySession.AllObjects<IssueTracker>(false).FirstOrDefault();
        if (bugTracker != null)
        {
          ICollection<Project> projectSet = bugTracker.ProjectSet.Keys;
          if (sortExpression.Length > 0)
          {
            string[] selectAndDir = sortExpression.Split(' ');
            int sorting = int.Parse(selectAndDir[0]);
            bool reverse = selectAndDir.Length > 1;
            switch (sorting)
            {
              case 1: projectSet = (from project in projectSet orderby project.Name select project).ToList();
                break;
              case 2: projectSet = (from project in projectSet orderby project.Description select project).ToList();
                break;
              case 3: projectSet = (from project in projectSet orderby project.CreatedBy select project).ToList();
                break;
            }
            if (reverse)
            {
              List<Project> tempList = new List<Project>(projectSet.Count);
              IEnumerable<Project> temp = projectSet.Reverse<Project>();
              foreach (Project issue in temp)
              {
                tempList.Add(issue);
              }
              projectSet = tempList;
            }
          }
          Session["Projects"] = projectSet;
          return projectSet;
        }
      }
      catch (System.Exception ex)
      {
        this.errorLabel.Text = ex.ToString();
      }
      return null;
    }

    public int ResolutionIndex(object obj)
    {
      if (obj != null)
      {
        Array enumStatus = AllResolution();
        int index = 0;
        foreach (object enumObj in enumStatus)
          if (enumObj.Equals(obj))
            return index;
          else
            ++index;
      }
      return -1;
    }

    public int StatusIndex(object obj)
    {
      if (obj != null)
      {
        Array enumStatus = AllStatus();
        int index = 0;
        foreach (object enumObj in enumStatus)
          if (enumObj.Equals(obj))
            return index;
          else
            ++index;
      }
      return -1;
    }

    public int CategoryIndex(object obj)
    {
      if (obj != null)
      {
        Array enumCategory = AllCategory();
        int index = 0;
        foreach (object enumObj in enumCategory)
          if (enumObj.Equals(obj))
            return index;
          else
            ++index;
      }
      return -1;
    }

    public int PriorityIndex(object obj)
    {
      if (obj != null)
      {
        Array enumPriority = AllPriority();
        int index = 0;
        foreach (object enumObj in enumPriority)
          if (enumObj.Equals(obj))
            return index;
          else
            ++index;
      }
      return -1;
    }

    public int ProjectIndex(object obj)
    {
      if (obj != null)
      {
        Project project = (Project)obj;
        try
        {
          IssueTracker bugTracker = s_sharedReadOnlySession.AllObjects<IssueTracker>(false).FirstOrDefault();
          SortedSetAny<Project> projectSet = bugTracker.ProjectSet;
          int index = projectSet.IndexOf(project);
          return index;
        }
        catch (System.Exception ex)
        {
          this.errorLabel.Text = ex.ToString();
          Console.WriteLine(ex.ToString());
        }
      }
      return -1;
    }

    public int ComponentIndex(object obj)
    {
      if (obj != null)
      {
        Component component = (Component)obj;
        try
        {
          IssueTracker bugTracker = s_sharedReadOnlySession.AllObjects<IssueTracker>(false).FirstOrDefault();
          SortedSetAny<Component> componentSet = bugTracker.ComponentSet;
          int index = componentSet.IndexOf(component);
          return index;
        }
        catch (System.Exception ex)
        {
          this.errorLabel.Text = ex.ToString();
          Console.WriteLine(ex.ToString());
        }
      }
      return -1;
    }

    public int VersionIndex(object obj)
    {
      if (obj != null)
      {
        ProductVersion version = (ProductVersion)obj;
        try
        {
          IssueTracker bugTracker = s_sharedReadOnlySession.AllObjects<IssueTracker>(false).FirstOrDefault();
          SortedSetAny<ProductVersion> versionSet = bugTracker.VersionSet;
          int index = versionSet.IndexOf(version);
          return index;
        }
        catch (System.Exception ex)
        {
          this.errorLabel.Text = ex.ToString();
          Console.WriteLine(ex.ToString());
        }
      }
      return -1;
    }

    public int AssignedToIndex(object obj)
    {
      if (obj != null)
      {
        User user = (User)obj;
        try
        {
          IssueTracker bugTracker = s_sharedReadOnlySession.AllObjects<IssueTracker>(false).FirstOrDefault();
          SortedSetAny<User> userSet = bugTracker.UserSet;
          int index = userSet.IndexOf(user);
          return index;
        }
        catch (System.Exception ex)
        {
          this.errorLabel.Text = ex.ToString();
          Console.WriteLine(ex.ToString());
        }
      }
      return -1;
    }

    public ICollection<Component> AllComponents(string sortExpression)
    {
      try
      {
        IssueTracker bugTracker = s_sharedReadOnlySession.AllObjects<IssueTracker>(false).FirstOrDefault();
        ICollection<Component> componentSet = bugTracker.ComponentSet.Keys;
        if (sortExpression.Length > 0)
        {
          string[] selectAndDir = sortExpression.Split(' ');
          int sorting = int.Parse(selectAndDir[0]);
          bool reverse = selectAndDir.Length > 1;
          switch (sorting)
          {
            case 1: componentSet = (from component in componentSet orderby component.Name select component).ToList();
              break;
            case 2: componentSet = (from component in componentSet orderby component.Description select component).ToList();
              break;
            case 3: componentSet = (from component in componentSet orderby component.Project select component).ToList();
              break;
          }
          if (reverse)
          {
            List<Component> tempList = new List<Component>(componentSet.Count);
            IEnumerable<Component> temp = componentSet.Reverse<Component>();
            foreach (Component issue in temp)
            {
              tempList.Add(issue);
            }
            componentSet = tempList;
          }
        }
        Session["Components"] = componentSet;
        return componentSet;
      }
      catch (System.Exception ex)
      {
        this.errorLabel.Text = ex.ToString();
      }
      return null;
    }

    public ICollection<ProductVersion> AllVersions(string sortExpression)
    {
      try
      {
        IssueTracker bugTracker = s_sharedReadOnlySession.AllObjects<IssueTracker>(false).FirstOrDefault();
        ICollection<ProductVersion> versionSet = bugTracker.VersionSet.Keys;
        if (sortExpression.Length > 0)
        {
          string[] selectAndDir = sortExpression.Split(' ');
          int sorting = int.Parse(selectAndDir[0]);
          bool reverse = selectAndDir.Length > 1;
          switch (sorting)
          {
            case 1: versionSet = (from version in versionSet orderby version.Name select version).ToList();
              break;
            case 2: versionSet = (from version in versionSet orderby version.Description select version).ToList();
              break;
            case 3: versionSet = (from version in versionSet orderby version.ReleaseDate select version).ToList();
              break;
            case 4: versionSet = (from version in versionSet orderby version.CreatedBy select version).ToList();
              break;
          }
          if (reverse)
          {
            List<ProductVersion> tempList = new List<ProductVersion>(versionSet.Count);
            IEnumerable<ProductVersion> temp = versionSet.Reverse<ProductVersion>();
            foreach (ProductVersion issue in temp)
            {
              tempList.Add(issue);
            }
            versionSet = tempList;
          }
        }
        return versionSet;
      }
      catch (System.Exception ex)
      {
        this.errorLabel.Text = ex.ToString();
      }
      return null;
    }

    [System.ComponentModel.DataObjectMethod(System.ComponentModel.DataObjectMethodType.Select)]
    public IEnumerable<Issue> SelectIssue(UInt64 Id)
    {
      try
      {
        Issue issue;
        if (Id > 0)
        {
          issue = (Issue)s_sharedReadOnlySession.Open(Id);
        }
        else
          issue = new Issue();
        List<Issue> list = new List<Issue>(1);
        list.Add(issue);
        return list;
      }
      catch (System.Exception ex)
      {
        this.errorLabel.Text = ex.ToString();
      }
      return null;
    }

    [System.ComponentModel.DataObjectMethod(System.ComponentModel.DataObjectMethodType.Select)]
    public IEnumerable<Project> SelectProject(UInt64 Id)
    {
      try
      {
        Project project;
        if (Id > 0)
        {
          project = (Project)s_sharedReadOnlySession.Open(Id);
        }
        else
          project = new Project();
        List<Project> list = new List<Project>(1);
        list.Add(project);
        return list;
      }
      catch (System.Exception ex)
      {
        this.errorLabel.Text = ex.ToString();
      }
      return null;
    }

    [System.ComponentModel.DataObjectMethod(System.ComponentModel.DataObjectMethodType.Select)]
    public IEnumerable<Component> SelectComponent(UInt64 Id)
    {
      try
      {
        Component component;
        if (Id > 0)
        {
          component = (Component)s_sharedReadOnlySession.Open(Id);
        }
        else
          component = new Component();
        List<Component> list = new List<Component>(1);
        list.Add(component);
        return list;
      }
      catch (System.Exception ex)
      {
        this.errorLabel.Text = ex.ToString();
      }
      return null;
    }

    [System.ComponentModel.DataObjectMethod(System.ComponentModel.DataObjectMethodType.Select)]
    public IEnumerable<ProductVersion> SelectVersion(UInt64 Id)
    {
      try
      {
        ProductVersion version;
        if (Id > 0)
        {
          version = (ProductVersion)s_sharedReadOnlySession.Open(Id);
        }
        else
          version = new ProductVersion(null, null, null, DateTime.MaxValue);
        List<ProductVersion> list = new List<ProductVersion>(1);
        list.Add(version);
        return list;
      }
      catch (System.Exception ex)
      {
        this.errorLabel.Text = ex.ToString();
      }
      return null;
    }

    [System.ComponentModel.DataObjectMethod(System.ComponentModel.DataObjectMethodType.Select)]
    public IEnumerable<User> SelectUser(UInt64 Id)
    {
      try
      {
        User user;
        if (Id > 0)
        {
          user = (User)s_sharedReadOnlySession.Open(Id);
        }
        else
          user = new User();
        List<User> list = new List<User>(1);
        list.Add(user);
        return list;
      }
      catch (System.Exception ex)
      {
        this.errorLabel.Text = ex.ToString();
      }
      return null;
    }

    void updateFilters()
    {
      TreeNode root = IssueFilterTree.Nodes[0];
      TreeNode id = root.ChildNodes[0];
      TreeNode priority = IssuePriorityTree.Nodes[0];
      //ViewState["filterById"] = id.Checked; // not working for unknown reason (data is null when refreshing datagrid)
      Session["filterById"] = id.Checked;
      Session["filterOn"] = FilterOnOrOff.SelectedIndex == 0;
      List<int> priorityList = new List<int>();
      foreach (TreeNode p in priority.ChildNodes)
        if (p.Checked)
          priorityList.Add(int.Parse(p.Value));
      Session["priorityFilter"] = priorityList;

      TreeNode status = IssueStatusTree.Nodes[0];
      List<int> statusList = new List<int>();
      foreach (TreeNode p in status.ChildNodes)
        if (p.Checked)
          statusList.Add(int.Parse(p.Value));
      Session["statusFilter"] = statusList;

      TreeNode category = IssueCategoryTree.Nodes[0];
      List<int> categoryList = new List<int>();
      foreach (TreeNode p in category.ChildNodes)
        if (p.Checked)
          categoryList.Add(int.Parse(p.Value));
      Session["categoryFilter"] = categoryList;
    }

    protected void FilterOnOrOff_SelectedIndexChanged(object sender, EventArgs e)
    {
      updateFilters();
    }

    protected void SetFiltersButton_Click(object sender, EventArgs args)
    {
      updateFilters();
      MultiView1.SetActiveView(IssuesView);
    }

    protected void IdFromDropDownList_SelectedIndexChanged(object sender, EventArgs e)
    {
      string idStr = IdFromDropDownList.SelectedValue;
      Session["idFrom"] = idStr;
      UInt64 fromId = UInt64.Parse(idStr);
      idStr = IdToDropDownList.SelectedValue;
      UInt64 toId = UInt64.Parse(idStr);
      ICollection<Issue> issues = AllIssues();
      var issueQuery = from issue in issues where issue.Id >= fromId orderby issue.Id select issue;
      int ct = IdToDropDownList.Items.Count;
      int selection = IdToDropDownList.SelectedIndex;
      IdToDropDownList.DataSource = issueQuery;
      int diff = issueQuery.Count() - ct;
      IdToDropDownList.DataBind();
      if (selection + diff > 0)
        IdToDropDownList.SelectedIndex = selection + diff;
    }

    public static string GetCategoryImage(object obj)
    {
      Issue issue = (Issue)obj;
      string imageFile = "~/images/";
      switch (issue.Category)
      {
        case Issue.CategoryEnum.Bug:
          return imageFile + "bug32.png";
        case Issue.CategoryEnum.CustomIssue:
          return imageFile + "genericissue32.png";
        case Issue.CategoryEnum.Improvement:
          return imageFile + "improvement32.png";
        case Issue.CategoryEnum.NewFeature:
          return imageFile + "newfeature32.png";
        case Issue.CategoryEnum.Task:
          return imageFile + "task32.png";
      }
      return imageFile;
    }

    public static string GetPriorityImage(object obj)
    {
      Issue issue = (Issue)obj;
      string imageFile = "~/images/";
      switch (issue.Priority)
      {
        case Issue.PriorityEnum.Blocker:
          return imageFile + "block32.png";
        case Issue.PriorityEnum.Critical:
          return imageFile + "critical32.jpg";
        case Issue.PriorityEnum.Major:
          return imageFile + "priorityMajor32.jpg";
        case Issue.PriorityEnum.Minor:
          return imageFile + "minor32.jpg";
        case Issue.PriorityEnum.Trivial:
          return imageFile + "trivial32.jpg";
      }
      return imageFile;
    }

    public static string GetResolutionImage(object obj)
    {
      Issue issue = (Issue)obj;
      string imageFile = "~/images/";
      switch (issue.FixResolution)
      {
        case Issue.Resolution.Incomplete:
          return imageFile + "incomplete32.jpg";
        case Issue.Resolution.CannotReproduce:
          return imageFile + "notReproducible32.jpg";
        case Issue.Resolution.Dublicate:
          return imageFile + "dublicate32.jpg";
        case Issue.Resolution.Fixed:
          return imageFile + "fixed32.jpg";
        case Issue.Resolution.WontFix:
          return imageFile + "wontFix32.jpg";
      }
      return imageFile;
    }

    public static string GetStatusImage(object obj)
    {
      Issue issue = (Issue)obj;
      string imageFile = "~/images/";
      switch (issue.Status)
      {
        case Issue.StatusEnum.Open:
          return imageFile + "open32.jpg";
        case Issue.StatusEnum.InProgress:
          return imageFile + "inprogress32.jpg";
        case Issue.StatusEnum.Resolved:
          return imageFile + "resolved32.jpg";
        case Issue.StatusEnum.Reopened:
          return imageFile + "reopen32.png";
        case Issue.StatusEnum.Closed:
          return imageFile + "closed32.jpg";
      }
      return imageFile;
    }

    protected void IdToDropDownList_SelectedIndexChanged(object sender, EventArgs e)
    {
      Session["idTo"] = IdToDropDownList.SelectedValue;
      //if ((bool)Session["filterById"])
      //  IssuesGridView.DataBind();
    }

    // really connected with "Edit" LinkButton in GridView
    protected void IssuesGridView_SelectedIndexChanged(object sender, EventArgs e)
    {
      IssueDetailsView.DataBind();
      MultiView1.SetActiveView(IssueEdit);
    }

    // really connected with "Edit" LinkButton in GridView
    protected void ProjectsGridView_SelectedIndexChanged(object sender, EventArgs e)
    {
      ProjectDetailsView.Visible = true;
      ProjectDetailsView.DataBind();
      ProjectUpdatePanelDetail.Update();
      ProjectModalPopup.Show();
    }

    // really connected with "Edit" LinkButton in GridView
    protected void ComponentsGridView_SelectedIndexChanged(object sender, EventArgs e)
    {
      ComponentDetailsView.Visible = true;
      ComponentDetailsView.DataBind();
      ComponentUpdatePanelDetail.Update();
      ComponentModalPopup.Show();
    }

    // really connected with "Edit" LinkButton in GridView
    protected void VersionsGridView_SelectedIndexChanged(object sender, EventArgs e)
    {
      //  set it to true so it will render
      this.VersionDetailsView.Visible = true;
      //  force databinding
      this.VersionDetailsView.DataBind();
      //  update the contents in the detail panel
      this.VersionUpdatePanelDetail.Update();
      //  show the modal popup
      this.VersionModalPopup.Show();
    }

    // really connected with "Edit" LinkButton in GridView
    protected void UsersGridView_SelectedIndexChanged(object sender, EventArgs e)
    {
      //  set it to true so it will render
      this.UserDetailsView.Visible = true;
      //  force databinding
      this.UserDetailsView.DataBind();
      //  update the contents in the detail panel
      this.UserUpdatePanelDetail.Update();
      //  show the modal popup
      this.UserModalPopup.Show();
    }

    protected void VelocityDbDataSourceIssue_Selecting(object sender, ObjectDataSourceSelectingEventArgs e)
    {
      Session["ProjectSelected"] = null;
      Session["ComponentSelected"] = null;
      Session["AssignedTo"] = null;
      Session["StatusSelected"] = null;
      Session["CategorySelected"] = null;
      Session["PrioritySelected"] = null;
      Session["ResolutionSelected"] = null;
      //  set the input parameter to the value of the selected index
      if (IssuesGridView.SelectedIndex < 0 || IssuesGridView.DataKeys.Count == 0) // why is this happening (IssuesGridView.DataKeys.Count == 0) ???
      {
        e.InputParameters["id"] = "0";
      }
      else
        e.InputParameters["id"] = Convert.ToString(IssuesGridView.DataKeys[IssuesGridView.SelectedIndex].Value);
    }

    protected void VelocityDbDataSourceProject_Selecting(object sender, ObjectDataSourceSelectingEventArgs e)
    {
      //  set the input parameter to the value of the selected index
      if (ProjectsGridView.SelectedIndex < 0 || ProjectsGridView.DataKeys.Count == 0) // why is this happening (ProjectsGridView.DataKeys.Count == 0) ???
      {
        e.InputParameters["id"] = "0";
      }
      else
        e.InputParameters["id"] = Convert.ToString(ProjectsGridView.DataKeys[ProjectsGridView.SelectedIndex].Value);
    }

    protected void VelocityDbDataSourceComponent_Selecting(object sender, ObjectDataSourceSelectingEventArgs e)
    {
      Session["ProjectSelected"] = null;
      //  set the input parameter to the value of the selected index
      if (this.ComponentsGridView.SelectedIndex < 0 || ComponentsGridView.DataKeys.Count == 0) // why is this happening (ComponentsGridView.DataKeys.Count == 0) ???
      {
        e.InputParameters["id"] = "0";
      }
      else
        e.InputParameters["id"] = Convert.ToString(ComponentsGridView.DataKeys[ComponentsGridView.SelectedIndex].Value);
    }

    protected void VelocityDbDataSourceUser_Selecting(object sender, ObjectDataSourceSelectingEventArgs e)
    {
      //  set the input parameter to the value of the selected index
      if (UsersGridView.SelectedIndex < 0 || UsersGridView.DataKeys.Count == 0) // ???
      {
        e.InputParameters["id"] = "0";
      }
      else
        e.InputParameters["id"] = Convert.ToString(UsersGridView.DataKeys[UsersGridView.SelectedIndex].Value);
    }

    protected void VelocityDbDataSourceVersion_Selecting(object sender, ObjectDataSourceSelectingEventArgs e)
    {
      //  set the input parameter to the value of the selected index
      if (VersionsGridView.SelectedIndex < 0 || VersionsGridView.DataKeys.Count == 0) // ???
      {
        e.InputParameters["id"] = "0";
      }
      else
        e.InputParameters["id"] = Convert.ToString(this.VersionsGridView.DataKeys[this.VersionsGridView.SelectedIndex].Value);
    }

    protected void CloseIssueLinkButton_Click(object sender, EventArgs args)
    {
      MultiView1.SetActiveView(IssuesView);
    }

    protected void SaveIssueLinkButton_Click(object sender, EventArgs args)
    {
      if (this.Page.IsValid)
      {
        //  move the data back to the data object
        IssueDetailsView.UpdateItem(false);
        MultiView1.SetActiveView(IssuesView);
        //  refresh the grid so we can see our changed
        IssuesGridView.DataBind();
      }
    }

    protected void SaveProjectLinkButton_Click(object sender, EventArgs args)
    {
      if (this.Page.IsValid)
      {
        //  move the data back to the data object
        ProjectDetailsView.UpdateItem(false);
        ProjectDetailsView.Visible = false;

        //  hide the modal popup
        ProjectModalPopup.Hide();

        //  add the css class for our yellow fade
        ScriptManager.GetCurrent(this).RegisterDataItem(ProjectsGridView, ProjectsGridView.SelectedIndex.ToString());

        //  refresh the grid so we can see our changed
        ProjectsGridView.DataBind();
        ProjectUpdatePanel.Update();
      }
    }

    protected void SaveComponentLinkButton_Click(object sender, EventArgs args)
    {
      if (this.Page.IsValid)
      {
        //  move the data back to the data object
        ComponentDetailsView.UpdateItem(false);
        ComponentDetailsView.Visible = false;

        //  hide the modal popup
        ComponentModalPopup.Hide();

        //  add the css class for our yellow fade
        ScriptManager.GetCurrent(this).RegisterDataItem(
            this.ComponentsGridView,
            this.ComponentsGridView.SelectedIndex.ToString()
        );

        //  refresh the grid so we can see our changed
        ComponentsGridView.DataBind();
        ComponentUpdatePanel.Update();
      }
    }

    protected void SaveVersionLinkButton_Click(object sender, EventArgs args)
    {
      if (this.Page.IsValid)
      {
        //  move the data back to the data object
        this.VersionDetailsView.UpdateItem(false);
        this.VersionDetailsView.Visible = false;

        //  hide the modal popup
        this.VersionModalPopup.Hide();

        //  add the css class for our yellow fade
        ScriptManager.GetCurrent(this).RegisterDataItem(
          // The control I want to send data to
            this.VersionsGridView,
          //  The data I want to send it (the row that was edited)
            this.VersionsGridView.SelectedIndex.ToString()
        );

        //  refresh the grid so we can see our changed
        this.VersionsGridView.DataBind();
        VersionUpdatePanel.Update();
      }
    }

    protected void SaveUserLinkButton_Click(object sender, EventArgs args)
    {
      if (this.Page.IsValid)
      {
        //  move the data back to the data object
        this.UserDetailsView.UpdateItem(false);
        this.UserDetailsView.Visible = false;

        //  hide the modal popup
        this.UserModalPopup.Hide();

        //  add the css class for our yellow fade
        ScriptManager.GetCurrent(this).RegisterDataItem(
          // The control I want to send data to
            this.UsersGridView,
          //  The data I want to send it (the row that was edited)
            this.UsersGridView.SelectedIndex.ToString()
        );

        //  refresh the grid so we can see our changed
        this.UsersGridView.DataBind();
        UsersUpdatePanel.Update();
      }
    }

    public void DeleteIssue(UInt64 id)
    {
      if (id > 0)
      {
        int sessionId = -1;
        SessionBase session = null;
        try
        {
          session = s_sessionPool.GetSession(out sessionId);
          using (var transaction = session.BeginUpdate())
          {
            IssueTracker issueTracker = session.AllObjects<IssueTracker>(false).FirstOrDefault();
            Issue existingIssue = (Issue)session.Open(id);
            if (issueTracker.Remove(existingIssue))
              existingIssue.Unpersist(session);
            transaction.Commit();
            s_sharedReadOnlySession.ForceDatabaseCacheValidation();
          }
        }
        catch (Exception ex)
        {
          this.errorLabel.Text = ex.ToString();
          Console.Out.WriteLine(ex.StackTrace);
        }
        finally
        {
          s_sessionPool.FreeSession(sessionId, session);
        }
      }
    }

    public void DeleteProject(UInt64 id)
    {
      try
      {
        if (id > 0)
        {
          int sessionId = -1;
          SessionBase session = null;
          try
          {
            session = s_sessionPool.GetSession(out sessionId);
            using (var transaction = session.BeginUpdate())
            {
              IssueTracker issueTracker = session.AllObjects<IssueTracker>(false).FirstOrDefault();
              Project existingProject = (Project)session.Open(id);
              if (issueTracker.ProjectSet.Remove(existingProject))
              {
                existingProject.Unpersist(session);
                session.Commit();
                s_sharedReadOnlySession.ForceDatabaseCacheValidation();
              }
            }
          }
          catch (Exception ex)
          {
            Console.Out.WriteLine(ex.StackTrace);
          }
          finally
          {
            s_sessionPool.FreeSession(sessionId, session);
          }
        }
      }
      catch (System.Exception ex)
      {
        this.errorLabel.Text = ex.ToString();
      }
    }

    public void DeleteComponent(UInt64 id)
    {
      try
      {
        if (id > 0)
        {
          int sessionId = -1;
          SessionBase session = null;
          try
          {
            session = s_sessionPool.GetSession(out sessionId);
            using (var transaction = session.BeginUpdate())
            {
              IssueTracker issueTracker = session.AllObjects<IssueTracker>(false).FirstOrDefault();
              Component existingComponent = (Component)session.Open(id);
              if (issueTracker.ComponentSet.Remove(existingComponent))
              {
                existingComponent.Unpersist(session);
                session.Commit();
                s_sharedReadOnlySession.ForceDatabaseCacheValidation();
              }
            }
          }
          catch (Exception ex)
          {
            Console.Out.WriteLine(ex.StackTrace);
          }
          finally
          {
            s_sessionPool.FreeSession(sessionId, session);
          }
        }
      }
      catch (System.Exception ex)
      {
        this.errorLabel.Text = ex.ToString();
      }
    }

    public void DeleteVersion(UInt64 id)
    {
      try
      {
        if (id > 0)
        {
          int sessionId = -1;
          SessionBase session = null;
          try
          {
            session = s_sessionPool.GetSession(out sessionId);
            using (var transaction = session.BeginUpdate())
            {
              IssueTracker issueTracker = session.AllObjects<IssueTracker>(false).FirstOrDefault();
              ProductVersion existingVersion = (ProductVersion)session.Open(id);
              if (issueTracker.VersionSet.Remove(existingVersion))
              {
                existingVersion.Unpersist(session);
                session.Commit();
                s_sharedReadOnlySession.ForceDatabaseCacheValidation();
              }
            }
          }
          catch (Exception ex)
          {
            Console.Out.WriteLine(ex.StackTrace);
          }
          finally
          {
            s_sessionPool.FreeSession(sessionId, session);
          }
        }
      }
      catch (System.Exception ex)
      {
        this.errorLabel.Text = ex.ToString();
      }
    }

    public void DeleteUser(UInt64 id)
    {
      try
      {
        if (id > 0)
        {
          int sessionId = -1;
          SessionBase session = null;
          try
          {
            session = s_sessionPool.GetSession(out sessionId);
            using (var transaction = session.BeginUpdate())
            {
              IssueTracker issueTracker = session.AllObjects<IssueTracker>(false).FirstOrDefault();
              User existingUser = (User)session.Open(id);
              if (issueTracker.UserSet.Remove(existingUser))
              {
                existingUser.Unpersist(session);
                session.Commit();
                s_sharedReadOnlySession.ForceDatabaseCacheValidation();
              }
            }
          }
          catch (Exception ex)
          {
            Console.Out.WriteLine(ex.StackTrace);
          }
          finally
          {
            s_sessionPool.FreeSession(sessionId, session);
          }
        }
      }
      catch (System.Exception ex)
      {
        this.errorLabel.Text = ex.ToString();
      }
    }

    User lookupUser(IssueTracker issueTracker, SessionBase session)
    {
      string userEmail = this.User.Identity.Name;
      User user = new User(userEmail);
      string dataPath = HttpContext.Current.Server.MapPath("~/Database");
      if (!issueTracker.UserSet.TryGetValue(user, ref user))
      {
        CustomerContact existingCustomer = null;
        string firstName = null;
        string lastName = null;
        string userName = null;
        try
        {
          using (SessionNoServer session2 = new SessionNoServer(dataPath, 2000, true, true))
          {
            session2.BeginRead();
            Root velocityDbroot = session2.AllObjects<Root>(false).FirstOrDefault();
            CustomerContact lookup = new CustomerContact(userEmail, null);
            velocityDbroot.customersByEmail.TryGetKey(lookup, ref existingCustomer);
            session2.Commit();
            firstName = existingCustomer.FirstName;
            lastName = existingCustomer.LastName;
            userName = existingCustomer.UserName;
          }
        }
        catch (System.Exception ex)
        {
          this.errorLabel.Text = ex.ToString();
        }
        user = new User(null, userEmail, firstName, lastName, userName);
        session.Persist(user);
        issueTracker.UserSet.Add(user);
      }
      return user;
    }

    public void UpdateIssue(EditedIssue newValues)
    {
      try
      {
        object projectIndexObj = Session["ProjectSelected"];
        int projectIndex = projectIndexObj == null ? 0 : (int)projectIndexObj;
        object componentIndexObj = Session["ComponentSelected"];
        int componentIndex = componentIndexObj == null ? 0 : (int)componentIndexObj;
        object versionIndexObj = Session["VersionSelected"];
        int versionIndex = versionIndexObj == null ? 0 : (int)versionIndexObj;
        object assignedToIndexObj = Session["AssignedToSelected"];
        int assignedToIndex = assignedToIndexObj == null ? 0 : (int)assignedToIndexObj;
        object statusIndexObj = Session["StatusSelected"];
        int statusIndex = statusIndexObj == null ? 0 : (int)statusIndexObj;
        object resolutionIndexObj = Session["ResolutionSelected"];
        int resolutionIndex = resolutionIndexObj == null ? 0 : (int)resolutionIndexObj;
        object categoryIndexObj = Session["CategorySelected"];
        int categoryIndex = categoryIndexObj == null ? 0 : (int)categoryIndexObj;
        object priorityIndexObj = Session["PrioritySelected"];
        int priorityIndex = priorityIndexObj == null ? 0 : (int)priorityIndexObj;
        object updatedAttachementsObj = Session["UpdatedAttachments"];
        SortedSetAny<Attachment> updatedAttachments = null;
        if (updatedAttachementsObj != null)
          updatedAttachments = (SortedSetAny<Attachment>)Session["UpdatedAttachments"];
        Session["UpdatedAttachments"] = null;
        int sessionId = -1;
        SessionBase session = null;
        try
        {
          session = s_sessionPool.GetSession(out sessionId);
          session.BeginUpdate();
          IssueTracker issueTracker = session.AllObjects<IssueTracker>(false).FirstOrDefault();
          Project project = issueTracker.ProjectSet.Keys[projectIndex];
          Component component = issueTracker.ComponentSet.Keys[componentIndex];
          ProductVersion version = issueTracker.VersionSet.Count > 0 ? issueTracker.VersionSet.Keys[versionIndex] : null;
          Issue.StatusEnum status = (Issue.StatusEnum)AllStatus().GetValue(statusIndex);
          Issue.CategoryEnum category = (Issue.CategoryEnum)AllCategory().GetValue(categoryIndex);
          Issue.PriorityEnum priority = (Issue.PriorityEnum)AllPriority().GetValue(priorityIndex);
          Issue.Resolution resolution = (Issue.Resolution)AllResolution().GetValue(resolutionIndex);
          User assignedTo = issueTracker.UserSet.Keys[assignedToIndex];
          if (newValues.Id == 0)
          {
            if (newValues.Summary == null)
              Console.WriteLine("Issue null summary detected in Update method");
            else
            {
              User user = lookupUser(issueTracker, session);
              Issue issue = new Issue(user, priority, project, category, component, version, resolution, newValues.Summary, newValues.Description,
                newValues.Environment, assignedTo, newValues.DueDate, updatedAttachments);
              if (updatedAttachments != null)
                foreach (Attachment attachment in updatedAttachments)
                  attachment.IssueAttachedTo = new WeakIOptimizedPersistableReference<Issue>(issue);
              session.Persist(issue);
              issueTracker.Add(issue);
            }
          }
          else
          {
            Issue existingIssue = (Issue)session.Open(newValues.Id);
            User user = lookupUser(issueTracker, session);
            if (existingIssue.Summary != newValues.Summary)
            {
              issueTracker.IssueSetBySummary.Remove(existingIssue);
              existingIssue.Summary = newValues.Summary;
              issueTracker.IssueSetBySummary.Add(existingIssue);
            }
            existingIssue.Description = newValues.Description;
            existingIssue.FixMessage = newValues.FixMessage;
            if (existingIssue.DueDate != newValues.DueDate)
            {
              issueTracker.IssueSetByDueDate.Remove(existingIssue);
              existingIssue.DueDate = newValues.DueDate;
              issueTracker.IssueSetByDueDate.Add(existingIssue);
            }
            if (existingIssue.LastUpdatedBy != user)
            {
              issueTracker.IssueSetByLastUpdatedBy.Remove(existingIssue);
              existingIssue.LastUpdatedBy = user;
              issueTracker.IssueSetByLastUpdatedBy.Add(existingIssue);
            }
            issueTracker.IssueSetByDateTimeUpdated.Remove(existingIssue);
            existingIssue.DateTimeLastUpdated = DateTime.Now;
            issueTracker.IssueSetByDateTimeUpdated.Add(existingIssue);

            if (priorityIndexObj != null && existingIssue.Priority != priority)
            {
              issueTracker.IssueSetByPriority.Remove(existingIssue);
              existingIssue.Priority = priority;
              issueTracker.IssueSetByPriority.Add(existingIssue);
            }
            if (categoryIndexObj != null && existingIssue.Category != category)
            {
              issueTracker.IssueSetByCategory.Remove(existingIssue);
              existingIssue.Category = category;
              issueTracker.IssueSetByCategory.Add(existingIssue);
            }
            if (componentIndexObj != null && existingIssue.Component != component)
            {
              existingIssue.Component = component;
              project = component.Project;
            }
            if (versionIndexObj != null && existingIssue.Version != version)
            {
              issueTracker.IssueSetByVersion.Remove(existingIssue);
              existingIssue.Version = version;
              issueTracker.IssueSetByVersion.Add(existingIssue);
            }
            existingIssue.Environment = newValues.Environment;
            if (projectIndexObj != null && existingIssue.Project != project)
            {
              issueTracker.IssueSetByProject.Remove(existingIssue);
              existingIssue.Project = project;
              issueTracker.IssueSetByProject.Add(existingIssue);
            }
            if (assignedToIndexObj != null && existingIssue.AssignedTo != assignedTo)
            {
              issueTracker.IssueSetByAssignedTo.Remove(existingIssue);
              existingIssue.AssignedTo = assignedTo;
              issueTracker.IssueSetByAssignedTo.Add(existingIssue);
            }
            if (statusIndexObj != null && existingIssue.Status != status)
            {
              issueTracker.IssueSetByStatus.Remove(existingIssue);
              existingIssue.Status = status;
              issueTracker.IssueSetByStatus.Add(existingIssue);
            }
            if (resolutionIndexObj != null && existingIssue.FixResolution != resolution)
            {
              //issueTracker.IssueSetByStatus.Remove(existingIssue);
              existingIssue.FixResolution = resolution;
              //issueTracker.IssueSetByStatus.Add(existingIssue);
            }
            if (updatedAttachments != null)
              foreach (Attachment attachment in updatedAttachments)
              {
                attachment.IssueAttachedTo = new WeakIOptimizedPersistableReference<Issue>(existingIssue);
                existingIssue.Attachments.Add(attachment);
              }
          }
          session.Commit();
          s_sharedReadOnlySession.ForceDatabaseCacheValidation();
        }
        catch (Exception ex)
        {
          session?.Abort();
          Console.Out.WriteLine(ex.StackTrace);
        }
        finally
        {
          s_sessionPool.FreeSession(sessionId, session);
        }
      }
      catch (System.Exception ex)
      {
        Console.WriteLine(ex.ToString());
        this.errorLabel.Text = ex.ToString();
      }
    }

    public void UpdateProject(EditedProject newValues)
    {
      try
      {
        string dataPath = HttpContext.Current.Server.MapPath("~/tracker");
        using (SessionNoServer session = new SessionNoServer(dataPath, 2000, true, true))
        {
          session.BeginUpdate();
          if (newValues.Id == 0)
          {
            if (newValues.Name == null)
              Console.WriteLine("Project null storeName detected in Update method");
            else
            {
              IssueTracker issueTracker = session.AllObjects<IssueTracker>(false).FirstOrDefault();
              User user = lookupUser(issueTracker, session);
              Project newProject = new Project(user, newValues.Name, newValues.Description);
              session.Persist(newProject);
              issueTracker.ProjectSet.Add(newProject);
            }
          }
          else
          {
            Project existingProject = (Project)session.Open(newValues.Id);
            existingProject.Name = newValues.Name;
            existingProject.Description = newValues.Description;
          }
          session.Commit();
          s_sharedReadOnlySession.ForceDatabaseCacheValidation();
        }
      }
      catch (System.Exception ex)
      {
        this.errorLabel.Text = ex.ToString();
      }
    }

    public void UpdateComponent(EditedComponent newValues)
    {
      try
      {
        object projectIndexObj = Session["ProjectSelected"];
        int projectIndex = projectIndexObj == null ? 0 : (int)projectIndexObj;
        int sessionId = -1;
        SessionBase session = null;
        try
        {
          session = s_sessionPool.GetSession(out sessionId);
          using (var transaction = session.BeginUpdate())
          {

            IssueTracker issueTracker = session.AllObjects<IssueTracker>(false).FirstOrDefault();
            Project project = issueTracker.ProjectSet.Keys[projectIndex];
            if (newValues.Id == 0)
            {
              if (newValues.Name == null)
                Console.WriteLine("Component null storeName detected in Update method");
              else
              {
                User user = lookupUser(issueTracker, session);
                Component newComponent = new Component(user, newValues.Name, newValues.Description, project);
                session.Persist(newComponent);
                issueTracker.ComponentSet.Add(newComponent);
              }
            }
            else
            {
              Component existingComponent = (Component)session.Open(newValues.Id);
              existingComponent.Name = newValues.Name;
              existingComponent.Description = newValues.Description;
              existingComponent.Project = project;
            }
            session.Commit();
            s_sharedReadOnlySession.ForceDatabaseCacheValidation();
          }
        }
        catch (Exception ex)
        {
          Console.Out.WriteLine(ex.StackTrace);
        }
        finally
        {
          s_sessionPool.FreeSession(sessionId, session);
        }
      }
      catch (System.Exception ex)
      {
        this.errorLabel.Text = ex.ToString();
      }
    }

    public void UpdateUser(EditedUser newValues)
    {
      try
      {
        int sessionId = -1;
        SessionBase session = null;
        try
        {
          session = s_sessionPool.GetSession(out sessionId);
          using (var transaction = session.BeginUpdate())
          {
            if (newValues.Id == 0)
            {
              IssueTracker issueTracker = session.AllObjects<IssueTracker>(false).FirstOrDefault();
              User user = lookupUser(issueTracker, session);
              User newUser = new User(user, newValues.Email, newValues.FirstName, newValues.LastName, newValues.UserName);
              session.Persist(newUser);
              issueTracker.UserSet.Add(newUser);
            }
            else
            {
              User existingUser = (User)session.Open(newValues.Id);
              existingUser.FirstName = newValues.FirstName;
              existingUser.LastName = newValues.LastName;
              existingUser.Email = newValues.Email;
              existingUser.UserName = newValues.UserName;
            }
            session.Commit();
            s_sharedReadOnlySession.ForceDatabaseCacheValidation();
          }
        }
        catch (Exception ex)
        {
          Console.Out.WriteLine(ex.StackTrace);
        }
        finally
        {
          s_sessionPool.FreeSession(sessionId, session);
        }
      }
      catch (System.Exception ex)
      {
        this.errorLabel.Text = ex.ToString();
      }
    }

    public void UpdateVersion(EditedVersion newValues)
    {
      try
      {
        int sessionId = -1;
        SessionBase session = null;
        try
        {
          session = s_sessionPool.GetSession(out sessionId);
          using (var transaction = session.BeginUpdate())
          {
            if (newValues.Id == 0)
            {
              if (newValues.Name == null)
                Console.WriteLine("ProductVersion null storeName detected in Update method");
              else
              {
                IssueTracker issueTracker = session.AllObjects<IssueTracker>(false).FirstOrDefault();
                User user = lookupUser(issueTracker, session);
                ProductVersion newVersion = new ProductVersion(user, newValues.Name, newValues.Description, newValues.ReleaseDate);
                session.Persist(newVersion);
                issueTracker.VersionSet.Add(newVersion);
              }
            }
            else
            {
              ProductVersion existingVersion = (ProductVersion)session.Open(newValues.Id);
              existingVersion.Name = newValues.Name;
              existingVersion.Description = newValues.Description;
              existingVersion.ReleaseDate = newValues.ReleaseDate;
            }
            session.Commit();
            s_sharedReadOnlySession.ForceDatabaseCacheValidation();
          }
        }
        catch (Exception ex)
        {
          Console.Out.WriteLine(ex.StackTrace);
        }
        finally
        {
          s_sessionPool.FreeSession(sessionId, session);
        }
      }
      catch (System.Exception ex)
      {
        this.errorLabel.Text = ex.ToString();
      }
    }

    public void InsertIssue(EditedIssue newValues)
    {
      newValues.Id = 0;
      newValues.Oid = "";
      newValues.Summary = "";
      newValues.Description = "";
      newValues.DueDate = DateTime.MaxValue;
    }

    public void InsertProject(EditedProject newValues)
    {
      newValues.Id = 0;
      newValues.Oid = "";
      newValues.Name = "";
      newValues.Description = "";
    }

    public void InsertComponent(EditedComponent newValues)
    {
      newValues.Id = 0;
      newValues.Oid = "";
      newValues.Name = "";
      newValues.Description = "";
    }

    public void InsertVersion(EditedVersion newValues)
    {
      newValues.Id = 0;
      newValues.Oid = "";
      newValues.Name = "";
      newValues.Description = "";
      newValues.ReleaseDate = DateTime.MaxValue;
    }

    public void InsertUser(EditedUser newValues)
    {
      newValues.CreatedBy = "";
      newValues.FirstName = "";
      newValues.LastName = "";
      newValues.UserName = "";
      newValues.Email = "";
    }

    protected void NewIssueLinkButton_Click(object sender, EventArgs args)
    {
      if (this.Page.IsValid)
      {
        Session["UpdatedAttachments"] = null;
        IssueDetailsView.ChangeMode(DetailsViewMode.Insert);
        IssuesGridView.SelectedIndex = -1;
        IssueDetailsView.InsertItem(false);
        IssueDetailsView.Visible = true;
        IssueDetailsView.DataBind();
        //IssueUpdatePanelDetail.Update();
        MultiView1.SetActiveView(IssueEdit);
        //IssueModalPopup.Show();
      }
    }

    protected void IssueViewDetails_Click(object sender, EventArgs args)
    {
      if (this.Page.IsValid)
      {
        Session["UpdatedAttachments"] = null;
        IssueDetailsView.ChangeMode(DetailsViewMode.Edit);
        IssueDetailsView.UpdateItem(false);
        IssueDetailsView.Visible = true;
        IssueDetailsView.DataBind();
        //IssueUpdatePanelDetail.Update();
        MultiView1.SetActiveView(IssueEdit);
        //IssueModalPopup.Show();
      }
    }

    protected void NewProjectLinkButton_Click(object sender, EventArgs args)
    {
      if (this.Page.IsValid)
      {
        ProjectDetailsView.ChangeMode(DetailsViewMode.Insert);
        ProjectsGridView.SelectedIndex = -1;
        ProjectDetailsView.InsertItem(false);
        ProjectDetailsView.Visible = true;
        ProjectDetailsView.DataBind();
        ProjectUpdatePanelDetail.Update();
        ProjectModalPopup.Show();
      }
    }

    protected void NewComponentLinkButton_Click(object sender, EventArgs args)
    {
      if (this.Page.IsValid)
      {
        ComponentDetailsView.ChangeMode(DetailsViewMode.Insert);
        ComponentsGridView.SelectedIndex = -1;
        ComponentDetailsView.InsertItem(false);
        ComponentDetailsView.Visible = true;
        ComponentDetailsView.DataBind();
        ComponentUpdatePanelDetail.Update();
        ComponentModalPopup.Show();
      }
    }

    protected void newVersionLinkButton_Click(object sender, EventArgs args)
    {
      if (Page.IsValid)
      {
        VersionDetailsView.ChangeMode(DetailsViewMode.Insert);
        VersionsGridView.SelectedIndex = -1; // so we don't select an existing item
        VersionDetailsView.InsertItem(false);
        //  set it to true so it will render
        VersionDetailsView.Visible = true;
        //  force databinding
        VersionDetailsView.DataBind();
        //  update the contents in the detail panel
        VersionUpdatePanelDetail.Update();
        //  show the modal popup
        VersionModalPopup.Show();
      }
    }

    protected void newUserLinkButton_Click(object sender, EventArgs args)
    {
      if (Page.IsValid)
      {
        UserDetailsView.ChangeMode(DetailsViewMode.Insert);
        UsersGridView.SelectedIndex = -1; // so we don't select an existing item
        UserDetailsView.InsertItem(false);
        //  set it to true so it will render
        UserDetailsView.Visible = true;
        //  force databinding
        UserDetailsView.DataBind();
        //  update the contents in the detail panel
        UserUpdatePanelDetail.Update();
        //  show the modal popup
        UserModalPopup.Show();
      }
    }

    protected void ProjectSelected(object sender, EventArgs e)
    {
      DropDownList dropDownList = (DropDownList)sender;
      Session["ProjectSelected"] = dropDownList.SelectedIndex;
    }

    protected void ComponentSelected(object sender, EventArgs e)
    {
      DropDownList dropDownList = (DropDownList)sender;
      Session["ComponentSelected"] = dropDownList.SelectedIndex;
    }

    protected void VersionSelected(object sender, EventArgs e)
    {
      DropDownList dropDownList = (DropDownList)sender;
      Session["VersionSelected"] = dropDownList.SelectedIndex;
    }

    protected void AssignedToSelected(object sender, EventArgs e)
    {
      DropDownList dropDownList = (DropDownList)sender;
      Session["AssignedToSelected"] = dropDownList.SelectedIndex;
    }

    protected void StatusSelected(object sender, EventArgs e)
    {
      DropDownList dropDownList = (DropDownList)sender;
      Session["StatusSelected"] = dropDownList.SelectedIndex;
    }

    protected void ResolutionSelected(object sender, EventArgs e)
    {
      DropDownList dropDownList = (DropDownList)sender;
      Session["ResolutionSelected"] = dropDownList.SelectedIndex;
    }

    protected void CategorySelected(object sender, EventArgs e)
    {
      DropDownList dropDownList = (DropDownList)sender;
      Session["CategorySelected"] = dropDownList.SelectedIndex;
    }

    protected void PrioritySelected(object sender, EventArgs e)
    {
      DropDownList dropDownList = (DropDownList)sender;
      Session["PrioritySelected"] = dropDownList.SelectedIndex;
    }

    protected void UploadButton_Click(object sender, EventArgs e)
    {
      FileUpload file = (FileUpload)IssueDetailsView.FindControl("AttachmentFileUpload");
      if (file.HasFile)
      {
        HttpPostedFile postedFile = file.PostedFile;
        TextBox fileInfoTextBox = (TextBox)IssueDetailsView.FindControl("UploadTextBox");
        ListBox listBox = (ListBox)IssueDetailsView.FindControl("UploadsListBox");
        Attachment attachment = new Attachment(postedFile.FileName, file.FileName, fileInfoTextBox.Text, file.FileBytes, postedFile.ContentType, null);
        listBox.Items.Add(new ListItem(attachment.ToString(), attachment.FileName));
        SortedSetAny<Attachment> attachments;
        object updatedAttachments = Session["UpdatedAttachments"];
        if (updatedAttachments == null)
        {
          attachments = new SortedSetAny<Attachment>();
          Session["UpdatedAttachments"] = attachments;
        }
        else
          attachments = (SortedSetAny<Attachment>)updatedAttachments;
        attachments.Add(attachment);
        updateImage(0, attachment.FileName);
      }
    }

    protected void DeleteAttachmentLinkButton_Click(object sender, EventArgs e)
    {
      ListBox listBox = (ListBox)IssueDetailsView.FindControl("UploadsListBox");
      if (listBox.SelectedValue != null && listBox.SelectedValue.Length > 1)
      {
        UInt64 id = 0;
        UInt64.TryParse(listBox.SelectedValue, out id);
        if (id != 0)
        {
          try
          {
            int sessionId = -1;
            SessionBase session = null;
            try
            {
              session = s_sessionPool.GetSession(out sessionId);
              session.BeginUpdate();
              Attachment attachment = (Attachment)session.Open(id);
              if (attachment.IssueAttachedTo != null)
                attachment.IssueAttachedTo.GetTarget(false, session).Attachments.Remove(attachment);
              attachment.Unpersist(session);
              session.Commit();
              s_sharedReadOnlySession.ForceDatabaseCacheValidation();
            }
            catch (Exception ex)
            {
              session?.Abort();
              Console.Out.WriteLine(ex.StackTrace);
            }
            finally
            {
              s_sessionPool.FreeSession(sessionId, session);
            }
          }
          catch (System.Exception ex)
          {
            Console.WriteLine(ex.ToString());
          }
        }
        listBox.Items.RemoveAt(listBox.SelectedIndex);
      }
    }

    protected void OpenAttachmentLinkButton_Click(object sender, EventArgs e)
    {
      ListBox listBox = (ListBox)IssueDetailsView.FindControl("UploadsListBox");
      if (listBox.SelectedValue != null && listBox.SelectedValue.Length > 1)
      {
        UInt64 id = 0;
        UInt64.TryParse(listBox.SelectedValue, out id);
        if (id == 0)
        {
          object updatedAttachments = Session["UpdatedAttachments"];
          SortedSetAny<Attachment> attachments = (SortedSetAny<Attachment>)updatedAttachments;
          Attachment attachment = new Attachment(null, listBox.SelectedItem.Value, null, null, null, null);
          if (attachments.TryGetValue(attachment, ref attachment))
          {
            Response.AddHeader("content-disposition", "attachment;filename=" + attachment.FileName);
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.ContentType = attachment.ContentType;
            Response.BinaryWrite(attachment.FileContent);
          }
        }
        else
        {
          try
          {
            int sessionId = -1;
            SessionBase session = null;
            try
            {
              session = s_sessionPool.GetSession(out sessionId);
              session.BeginUpdate();
              Attachment attachment = (Attachment)session.Open(id);
              session.Commit();
              Response.AddHeader("content-disposition", "attachment;filename=" + attachment.FileName);
              Response.Cache.SetCacheability(HttpCacheability.NoCache);
              Response.ContentType = attachment.ContentType;
              Response.BinaryWrite(attachment.FileContent);
              s_sharedReadOnlySession.ForceDatabaseCacheValidation();
            }
            catch (Exception ex)
            {
              session?.Abort();
              Console.Out.WriteLine(ex.StackTrace);
            }
            finally
            {
              s_sessionPool.FreeSession(sessionId, session);
            }
          }
          catch (System.Exception ex)
          {
            Console.WriteLine(ex.ToString());
          }
        }
        listBox.Items.RemoveAt(listBox.SelectedIndex);
      }
    }

    protected void RefreshButton_Click(object sender, EventArgs e)
    {
      IssuesGridView.DataBind();
    }

    protected void updateImage(UInt64 id, string fileName)
    {
      Image image = (Image)IssueDetailsView.FindControl("UploadImage");
      TextBox uploadTextBox = (TextBox)IssueDetailsView.FindControl("UploadTextBox");
      if (id > 0)
      {
        try
        {
          Attachment attachment = (Attachment)s_sharedReadOnlySession.Open(id);
          image.Visible = attachment.ContentType.Contains("image");
          uploadTextBox.Text = attachment.Comment;
        }
        catch (System.Exception ex)
        {
          Console.WriteLine(ex.ToString());
        }
        image.ImageUrl = "~/image.ashx?a=" + id.ToString() + "&t=120";
      }
      else
      {
        object updatedAttachments = Session["UpdatedAttachments"];
        SortedSetAny<Attachment> attachments = (SortedSetAny<Attachment>)updatedAttachments;

        foreach (Attachment attachment in attachments)
          if (attachment.FileName == fileName)
          {
            uploadTextBox.Text = attachment.Comment;
            Session["Image"] = attachment.FileContent;
            if (attachment.ContentType.Contains("image"))
            {
              image.ImageUrl = "~/image.aspx?t=120";
              image.Visible = true;
            }
            else
              image.Visible = false;
            break;
          }
      }
      image.DataBind();
    }

    protected void UploadsListBox_SelectedIndexChanged(object sender, EventArgs e)
    {
      ListBox listBox = (ListBox)sender;
      if (listBox.SelectedValue != null && listBox.SelectedValue.Length > 1)
      {
        UInt64 id = 0;
        UInt64.TryParse(listBox.SelectedValue, out id);
        updateImage(id, listBox.SelectedItem.Value);
      }
    }
    protected void UseIcons_CheckedChange(object sender, EventArgs e)
    {
      foreach (GridViewRow row in IssuesGridView.Rows)
      {
        Image priorityImage = (Image)row.FindControl("PriorityImage");
        Label priorityLabel = (Label)row.FindControl("PriorityLabel");
        Image statusImage = (Image)row.FindControl("StatusImage");
        Label statusLabel = (Label)row.FindControl("StatusLabel");
        Image categoryImage = (Image)row.FindControl("CategoryImage");
        Label categoryLabel = (Label)row.FindControl("CategoryLabel");
        Image resolutionImage = (Image)row.FindControl("ResolutionImage");
        Label resolutionLabel = (Label)row.FindControl("ResolutionLabel");
        priorityImage.Visible = UseIcons.Checked;
        statusImage.Visible = UseIcons.Checked;
        categoryImage.Visible = UseIcons.Checked;
        resolutionImage.Visible = UseIcons.Checked;
        priorityLabel.Visible = !UseIcons.Checked;
        statusLabel.Visible = !UseIcons.Checked;
        categoryLabel.Visible = !UseIcons.Checked;
        resolutionLabel.Visible = !UseIcons.Checked;
      }
    }

    protected void SetRegularPermissions_Click(object sender, EventArgs e)
    {

    }

    protected void SetDevelopersPermissions_Click(object sender, EventArgs e)
    {

    }

    protected void SetAdminPermissions_Click(object sender, EventArgs e)
    {

    }

    protected void AddAdminUsers_Click(object sender, EventArgs e)
    {
      int sessionId = -1;
      SessionBase session = null;
      try
      {
        session = s_sessionPool.GetSession(out sessionId);
        session.BeginUpdate();
        IssueTracker bugTracker = session.AllObjects<IssueTracker>(false).FirstOrDefault();
        int index = 0;
        foreach (ListItem item in RegularUsers.Items)
        {
          if (item.Selected)
          {
            if (bugTracker.UserSet.Keys[index].UserName == item.Value && bugTracker.Permissions.AdminSet.Contains(bugTracker.UserSet.Keys[index]) == false)
              bugTracker.Permissions.AdminSet.Add(bugTracker.UserSet.Keys[index]);
          }
          index++;
        }
        session.Commit();
        AdminUsers.DataSource = bugTracker.Permissions.AdminSet.Keys;
        AdminUsers.DataBind();
        s_sharedReadOnlySession.ForceDatabaseCacheValidation();
      }
      catch (Exception ex)
      {
        session?.Abort();
        Console.Out.WriteLine(ex.StackTrace);
      }
      finally
      {
        s_sessionPool.FreeSession(sessionId, session);
      }
    }

    protected void AddDeveloperUsers_Click(object sender, EventArgs e)
    {

    }


    protected void RemoveDeveloperUsers_Click(object sender, EventArgs e)
    {

    }

    protected void RemoveAdminUsers_Click(object sender, EventArgs e)
    {
      int sessionId = -1;
      SessionBase session = null;
      try
      {
        session = s_sessionPool.GetSession(out sessionId);
        session.BeginUpdate();
        IssueTracker bugTracker = session.AllObjects<IssueTracker>(false).FirstOrDefault();
        int index = 0;
        foreach (ListItem item in AdminUsers.Items)
        {
          if (item.Selected)
          {
            User user = bugTracker.Permissions.AdminSet.Keys[index];
            if (user.UserName == item.Value) // make sure no other user updated list since we build ours
              bugTracker.Permissions.AdminSet.Remove(user);
          }
          index++;
        }
        session.Commit();
        AdminUsers.DataSource = bugTracker.Permissions.AdminSet.Keys;
        AdminUsers.DataBind();
        s_sharedReadOnlySession.ForceDatabaseCacheValidation();
      }
      catch (Exception ex)
      {
        session?.Abort();
        Console.Out.WriteLine(ex.StackTrace);
      }
      finally
      {
        s_sessionPool.FreeSession(sessionId, session);
      }
    }
  }
}