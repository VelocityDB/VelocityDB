using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VelocityDb;
using VelocityDb.Session;
using VelocityDbSchema.Samples.Baseball;

namespace Baseball
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    static readonly string s_systemDir = "Baseball"; // appended to SessionBase.BaseDatabasePath
    static readonly string s_licenseDbFile = "c:/4.odb";

    public MainWindow()
    {
      InitializeComponent();
    }

    private void lahman58csv_GotFocus(object sender, RoutedEventArgs e)
    {
      try
      {
        FolderBrowserDialog chooseFolder = new FolderBrowserDialog();
        DialogResult result = chooseFolder.ShowDialog();
        if (result == System.Windows.Forms.DialogResult.OK)
          lahman58db.Text = chooseFolder.SelectedPath; 
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.ToString());
      }
    }

    private void loadButton_Click(object sender, RoutedEventArgs e)
    {
      errorMessage.Content = null;
      try
      {
        using (SessionNoServer session = new SessionNoServer(s_systemDir))
        {
          Console.WriteLine($"Running with databases in directory: {session.SystemDirectory}");
          session.BeginUpdate();
          File.Copy(s_licenseDbFile, System.IO.Path.Combine(session.SystemDirectory, "4.odb"), true);
          string line;
          using (StreamReader stream = new StreamReader(lahman58db.Text + "\\Allstar.csv", true))
          {
            line = stream.ReadLine(); // heading title line
            while ((line = stream.ReadLine()) != null)
            {
              AllStar allStar = new AllStar(line);
              session.Persist(allStar);
            }
          }          
          using (StreamReader stream = new StreamReader(lahman58db.Text + "\\AllstarFull.csv", true))
          {
            line = stream.ReadLine(); // heading title line
            while ((line = stream.ReadLine()) != null)
            {
              AllStarFull allStar = new AllStarFull(line);
              session.Persist(allStar);
            }
          }
          using (StreamReader stream = new StreamReader(lahman58db.Text + "\\Appearances.csv", true))
          {
            line = stream.ReadLine(); // heading title line
            while ((line = stream.ReadLine()) != null)
            {
              Appearances a = new Appearances(line);
              session.Persist(a);
            }
          }
          using (StreamReader stream = new StreamReader(lahman58db.Text + "\\AwardsManagers.csv", true))
          {
            line = stream.ReadLine(); // heading title line
            while ((line = stream.ReadLine()) != null)
            {
              AwardsManagers a = new AwardsManagers(line);
              session.Persist(a);
            }
          }
          using (StreamReader stream = new StreamReader(lahman58db.Text + "\\AwardsPlayers.csv", true))
          {
            line = stream.ReadLine(); // heading title line
            while ((line = stream.ReadLine()) != null)
            {
              AwardsPlayers a = new AwardsPlayers(line);
              session.Persist(a);
            }
          }
          using (StreamReader stream = new StreamReader(lahman58db.Text + "\\AwardsShareManagers.csv", true))
          {
            line = stream.ReadLine(); // heading title line
            while ((line = stream.ReadLine()) != null)
            {
              AwardsShareManagers a = new AwardsShareManagers(line);
              session.Persist(a);
            }
          }
          using (StreamReader stream = new StreamReader(lahman58db.Text + "\\AwardsSharePlayers.csv", true))
          {
            line = stream.ReadLine(); // heading title line
            while ((line = stream.ReadLine()) != null)
            {
              AwardsSharePlayers a = new AwardsSharePlayers(line);
              session.Persist(a);
            }
          }
          using (StreamReader stream = new StreamReader(lahman58db.Text + "\\Batting.csv", true))
          {
            line = stream.ReadLine(); // heading title line
            while ((line = stream.ReadLine()) != null)
            {
              Batting a = new Batting(line);
              session.Persist(a);
            }
          }
          using (StreamReader stream = new StreamReader(lahman58db.Text + "\\BattingPost.csv", true))
          {
            line = stream.ReadLine(); // heading title line
            while ((line = stream.ReadLine()) != null)
            {
              BattingPost a = new BattingPost(line);
              session.Persist(a);
            }
          }
          using (StreamReader stream = new StreamReader(lahman58db.Text + "\\Fielding.csv", true))
          {
            line = stream.ReadLine(); // heading title line
            while ((line = stream.ReadLine()) != null)
            {
              Fielding a = new Fielding(line);
              session.Persist(a);
            }
          }
          using (StreamReader stream = new StreamReader(lahman58db.Text + "\\FieldingOF.csv", true))
          {
            line = stream.ReadLine(); // heading title line
            while ((line = stream.ReadLine()) != null)
            {
             FieldingOF a = new FieldingOF(line);
             session.Persist(a);
            }
          }
          using (StreamReader stream = new StreamReader(lahman58db.Text + "\\FieldingPost.csv", true))
          {
            line = stream.ReadLine(); // heading title line
            while ((line = stream.ReadLine()) != null)
            {
              FieldingPost a = new FieldingPost(line);
              session.Persist(a);
            }
          }
          using (StreamReader stream = new StreamReader(lahman58db.Text + "\\HallOfFame.csv", true))
          {
            line = stream.ReadLine(); // heading title line
            while ((line = stream.ReadLine()) != null)
            {
              HallOfFame a = new HallOfFame(line);
              session.Persist(a);
            }
          }
          using (StreamReader stream = new StreamReader(lahman58db.Text + "\\HOFold.csv", true))
          {
            line = stream.ReadLine(); // heading title line
            while ((line = stream.ReadLine()) != null)
            {
              HOFold a = new HOFold(line);
              session.Persist(a);
            }
          }
          using (StreamReader stream = new StreamReader(lahman58db.Text + "\\Managers.csv", true))
          {
            line = stream.ReadLine(); // heading title line
            while ((line = stream.ReadLine()) != null)
            {
              Managers a = new Managers(line);
              session.Persist(a);
            }
          }
          using (StreamReader stream = new StreamReader(lahman58db.Text + "\\ManagersHalf.csv", true))
          {
            line = stream.ReadLine(); // heading title line
            while ((line = stream.ReadLine()) != null)
            {
              ManagersHalf a = new ManagersHalf(line);
              session.Persist(a);
            }
          }
          using (StreamReader stream = new StreamReader(lahman58db.Text + "\\Master.csv", true))
          {
            line = stream.ReadLine(); // heading title line
            while ((line = stream.ReadLine()) != null)
            {
              Master a = new Master(line);
              session.Persist(a);
            }
          }
          using (StreamReader stream = new StreamReader(lahman58db.Text + "\\Pitching.csv", true))
          {
            line = stream.ReadLine(); // heading title line
            while ((line = stream.ReadLine()) != null)
            {
              Pitching a = new Pitching(line);
              session.Persist(a);
            }
          }
          using (StreamReader stream = new StreamReader(lahman58db.Text + "\\PitchingPost.csv", true))
          {
            line = stream.ReadLine(); // heading title line
            while ((line = stream.ReadLine()) != null)
            {
              PitchingPost a = new PitchingPost(line);
              session.Persist(a);
            }
          }
          using (StreamReader stream = new StreamReader(lahman58db.Text + "\\Salaries.csv", true))
          {
            line = stream.ReadLine(); // heading title line
            while ((line = stream.ReadLine()) != null)
            {
              Salaries a = new Salaries(line);
              session.Persist(a);
            }
          }
          using (StreamReader stream = new StreamReader(lahman58db.Text + "\\Schools.csv", true))
          {
            line = stream.ReadLine(); // heading title line
            while ((line = stream.ReadLine()) != null)
            {
              Schools a = new Schools(line);
              session.Persist(a);
            }
          }
          using (StreamReader stream = new StreamReader(lahman58db.Text + "\\SchoolsPlayers.csv", true))
          {
            line = stream.ReadLine(); // heading title line
            while ((line = stream.ReadLine()) != null)
            {
              SchoolsPlayers a = new SchoolsPlayers(line);
              session.Persist(a);
            }
          }
          using (StreamReader stream = new StreamReader(lahman58db.Text + "\\SeriesPost.csv", true))
          {
            line = stream.ReadLine(); // heading title line
            while ((line = stream.ReadLine()) != null)
            {
              SeriesPost a = new SeriesPost(line);
              session.Persist(a);
            }
          }
          using (StreamReader stream = new StreamReader(lahman58db.Text + "\\Teams.csv", true))
          {
            line = stream.ReadLine(); // heading title line
            while ((line = stream.ReadLine()) != null)
            {
              Teams a = new Teams(line);
              session.Persist(a);
            }
          }
          using (StreamReader stream = new StreamReader(lahman58db.Text + "\\TeamsFranchises.csv", true))
          {
            line = stream.ReadLine(); // heading title line
            while ((line = stream.ReadLine()) != null)
            {
              TeamsFranchises a = new TeamsFranchises(line);
              session.Persist(a);
            }
          }
          using (StreamReader stream = new StreamReader(lahman58db.Text + "\\TeamsHalf.csv", true))
          {
            line = stream.ReadLine(); // heading title line
            while ((line = stream.ReadLine()) != null)
            {
              TeamsHalf a = new TeamsHalf(line);
              session.Persist(a);
            }
          }
          using (StreamReader stream = new StreamReader(lahman58db.Text + "\\Xref_Stats.csv", true))
          {
            line = stream.ReadLine(); // heading title line
            while ((line = stream.ReadLine()) != null)
            {
              Xref_Stats a = new Xref_Stats(line);
              session.Persist(a);
            }
          }
          session.Commit();      
          errorMessage.Content = "Done, databases located in: " + session.SystemDirectory + " View objects using VelocityDBBrowser";
        }
      }
      catch (Exception ex)
      {
        errorMessage.Content = ex.Message == null ? ex.ToString() : ex.Message;
      }
    }
  }
}
