using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using VelocityDb;
using VelocityDb.Session;
using VelocityDb.Collection;
using VelocityDbSchema;
using VelocityDbSchema.Tracker;
using NUnit.Framework;

namespace NUnitTests
{
  [TestFixture]
  public class TrackerTests
  {
    public const string systemDir = "C:\\VelocityDb\\VelocityDb.com\\tracker";
    static Random rand = new Random(5);

    [Test]
    public void Create1Root()
    {
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        Placement placementRoot = new Placement(IssueTracker.PlaceInDatabase);
        IssueTracker issueTracker = new IssueTracker(10, session);
        User user = new User(null, "mats.persson@gmail.com", "Mats", "Persson", "matspca");
        user.Persist(session, user);
        PermissionScheme permissions = new PermissionScheme(user);
        issueTracker.Permissions = permissions;
        issueTracker.Persist(placementRoot, session);
        session.Commit();
      }
    }

    [TestCase(80)]
    public void Create2Users(int numberOfUsers)
    {
      User user = null;
      User priorUser = null;
      for (int i = 0; i < numberOfUsers; i++)
        using (SessionNoServer session = new SessionNoServer(systemDir))
        {
          session.BeginUpdate();
          IssueTracker issueTracker = (IssueTracker)session.Open(IssueTracker.PlaceInDatabase, 1, 1, false);
          string email = i.ToString() + "@gmail.com";
          string first = "first" + i.ToString();
          string last = "last" + i.ToString();
          string userName = "username" + i.ToString();
          user = new User(user, email, first, last, userName);
          user.Persist(session, priorUser ?? user);
          issueTracker.UserSet.Add(user);
          priorUser = user;
          session.Commit();
        }
    }

    [TestCase(50)]
    public void Create3Versions(int numberOfVersions)
    {
      ProductVersion version = null;
      ProductVersion priorVersion = null;
      for (int i = 0; i < numberOfVersions; i++)
        using (SessionNoServer session = new SessionNoServer(systemDir))
        {
          session.BeginUpdate();
          IssueTracker issueTracker = (IssueTracker)session.Open(IssueTracker.PlaceInDatabase, 1, 1, false);
          User user = issueTracker.UserSet.Keys[rand.Next(issueTracker.UserSet.Keys.Count - 1)];
          string v = "version" + i.ToString();
          string d = "vdescription" + i.ToString();
          version = new ProductVersion(user, v, d, null);
          version.Persist(session, priorVersion ?? version);
          issueTracker.VersionSet.Add(version);
          priorVersion = version;
          session.Commit();
        }
    }

    [TestCase(50)]
    public void Create4Projects(int numberOfProjects)
    {
      Project project = null;
      Project priorProject = null;
      for (int i = 0; i < numberOfProjects; i++)
        using (SessionNoServer session = new SessionNoServer(systemDir))
        {
          session.BeginUpdate();
          IssueTracker issueTracker = (IssueTracker)session.Open(IssueTracker.PlaceInDatabase, 1, 1, false);
          User user = issueTracker.UserSet.Keys[rand.Next(issueTracker.UserSet.Keys.Count - 1)];
          string p = "project" + i.ToString();
          string d = "pdescription" + i.ToString();
          project = new Project(user, p, d);
          project.Persist(session, priorProject ?? project);
          priorProject = project;
          issueTracker.ProjectSet.Add(project);
          session.Commit();
        }
    }

    [TestCase(60)]
    public void Create5Components(int numberOfComponents)
    {
      Component component = null;
      Component priorComponent = null;
      for (int i = 0; i < numberOfComponents; i++)
        using (SessionNoServer session = new SessionNoServer(systemDir))
        {
          session.BeginUpdate();
          IssueTracker issueTracker = (IssueTracker)session.Open(IssueTracker.PlaceInDatabase, 1, 1, false);
          User user = issueTracker.UserSet.Keys[rand.Next(issueTracker.UserSet.Keys.Count - 1)];
          Project project = issueTracker.ProjectSet.Keys[rand.Next(issueTracker.ProjectSet.Keys.Count - 1)];
          string c = "component" + i.ToString();
          string d = "cdescription" + i.ToString();
          component = new Component(user, c, d, project);
          component.Persist(session, priorComponent ?? component);
          issueTracker.ComponentSet.Add(component);
          priorComponent = component;
          session.Commit();
        }
    }

    [TestCase(250)]
    public void Create6Issues(int numberOfIssues)
    {
      Issue issue = null;
      Issue priorIssue = null;
      using (SessionNoServer session = new SessionNoServer(systemDir))
      {
        session.BeginUpdate();
        for (int i = 0; i < numberOfIssues; i++)
        {         
          IssueTracker issueTracker = (IssueTracker)session.Open(IssueTracker.PlaceInDatabase, 1, 1, false);
          User user = issueTracker.UserSet.Keys[rand.Next(issueTracker.UserSet.Count)];
          User assignedTo = issueTracker.UserSet.Keys[rand.Next(issueTracker.UserSet.Count)];
          Project project = issueTracker.ProjectSet.Keys[rand.Next(issueTracker.ProjectSet.Count)];
          Component component = issueTracker.ComponentSet.Keys[rand.Next(issueTracker.ComponentSet.Count)];
          ProductVersion version = issueTracker.VersionSet.Keys[rand.Next(issueTracker.VersionSet.Count)];
          Issue.CategoryEnum category = (Issue.CategoryEnum)rand.Next(5);
          Issue.Resolution resolution = (Issue.Resolution)rand.Next(5);
          Issue.PriorityEnum priority = (Issue.PriorityEnum)rand.Next(5);
          Issue.StatusEnum status = (Issue.StatusEnum)rand.Next(5);
          if (status == Issue.StatusEnum.Open || status == Issue.StatusEnum.InProgress || status == Issue.StatusEnum.Reopened)
            resolution = Issue.Resolution.Incomplete; // the other states does not make sense
          DateTime dueDate = new DateTime(rand.Next());
          string c = "project" + i.ToString();
          string s = "summary" + i.ToString();
          string e = "environment" + i.ToString();
          string d = "idescription" + i.ToString();
          issue = new Issue(user, priority, project, category, component, version, resolution, s, d, e, assignedTo, dueDate, null, status);
          issue.Persist(session, priorIssue == null ? issue : priorIssue);
          priorIssue = issue;
          issueTracker.Add(issue);       
        }
        session.Commit();
      }
    }

    [Test]
    public void VerifyOpenIssueTracker()
    {
      using (SessionNoServer session = new SessionNoServer(systemDir, 2000, true, true))
      {
        session.BeginRead();
        IssueTracker issueTracker = (IssueTracker)session.Open(IssueTracker.PlaceInDatabase, 1, 1, false);
        session.Commit();
      }
    }

    [Test]
    public void VerifyOpenIssueTrackerCacheOff()
    {
      using (SessionNoServer session = new SessionNoServer(systemDir, 2000, true, false))
      {
        session.BeginUpdate();
        IssueTracker issueTracker = (IssueTracker)session.Open(IssueTracker.PlaceInDatabase, 1, 1, false);
        session.Commit();
      }
    }
  }
}
