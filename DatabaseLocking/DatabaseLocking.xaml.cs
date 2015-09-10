using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VelocityDb;
using VelocityDb.Session;
using VelocityDbSchema.Samples.DatabaseLocking;

namespace DatabaseLocking
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    const int defaultWaitTime = 9999;
    const int numberOfSessions = 4;
    static readonly string systemDir =
  System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
  "VelocityDB" + System.IO.Path.DirectorySeparatorChar + "Databases" + System.IO.Path.DirectorySeparatorChar + "DatabaseLocking");
    SessionBase[] session;
    Thread[] thread;

    public MainWindow()
    {
      InitializeComponent();
      if (Directory.Exists(systemDir))
        Directory.Delete(systemDir, true); // remove systemDir from prior runs and all its databases.
      try
      {
        session = new SessionBase[4];
        thread = new Thread[4];
        session[0] = new SessionNoServer(systemDir, int.Parse(session1LockTimeout.Text));
        session[1] = new SessionNoServer(systemDir, int.Parse(session2LockTimeout.Text));
        session[2] = new SessionNoServer(systemDir, int.Parse(session3LockTimeout.Text));
        session[3] = new SessionNoServer(systemDir, int.Parse(session4LockTimeout.Text));
        session[0].BeginUpdate();
        Placement place = new Placement(10);
        Number number = new Number();
        session[0].Persist(place, number);
        number = new Number(2);
        place = new Placement(11);
        session[0].Persist(place, number);
        number = new Number(3);
        place = new Placement(12);
        session[0].Persist(place, number);
        session[0].Commit();
      }
      catch (Exception ex)
      {
        session1messages.Content = ex.Message;
      }
    }

    private void session1locking(object sender, RoutedEventArgs e)
    {
      if (session != null)
      try
      {
        if (session[0].InTransaction == false)
          if (session1server.IsChecked.Value)
            session[0] = new ServerClientSession(systemDir, null, int.Parse(session1LockTimeout.Text), session1optimistic.IsChecked.Value);
          else
            session[0] = new SessionNoServer(systemDir, int.Parse(session1LockTimeout.Text), session1optimistic.IsChecked.Value);
        else
          session1messages.Content = "Can only set OptimisticLocking attribute while not in a transaction";
      }
      catch (Exception ex)
      {
        session1messages.Content = ex.Message;
      }
    }

    private void session2locking(object sender, RoutedEventArgs e)
    {
      if (session != null)
      try
      {
        if (session[1].InTransaction == false)
          if (session2server.IsChecked.Value)
            session[1] = new ServerClientSession(systemDir, null, int.Parse(session2LockTimeout.Text), session2optimistic.IsChecked.Value);
          else
            session[1] = new SessionNoServer(systemDir, int.Parse(session2LockTimeout.Text), session2optimistic.IsChecked.Value);
        else
          session2messages.Content = "Can only set OptimisticLocking attribute while not in a transaction";
      }
      catch (Exception ex)
      {
        session2messages.Content = ex.Message;
      }
    }

    private void session3locking(object sender, RoutedEventArgs e)
    {
      if (session != null)
      try
      {
        if (session[2].InTransaction == false)
          if (session3server.IsChecked.Value)
            session[2] = new ServerClientSession(systemDir, null, int.Parse(session3LockTimeout.Text), session3optimistic.IsChecked.Value);
          else
            session[2] = new SessionNoServer(systemDir, int.Parse(session3LockTimeout.Text), session3optimistic.IsChecked.Value);
        else
          session3messages.Content = "Can only set OptimisticLocking attribute while not in a transaction";
      }
      catch (Exception ex)
      {
        session3messages.Content = ex.Message;
      }
    }

    private void session4locking(object sender, RoutedEventArgs e)
    {
      if (session != null)
      try
      {
        if (session[3].InTransaction == false)
          if (session4server.IsChecked.Value)
            session[3] = new ServerClientSession(systemDir, null, int.Parse(session4LockTimeout.Text), session4optimistic.IsChecked.Value);
          else
            session[3] = new SessionNoServer(systemDir, int.Parse(session4LockTimeout.Text), session4optimistic.IsChecked.Value);
        else
          session4messages.Content = "Can only set OptimisticLocking attribute while not in a transaction";
      }
      catch (Exception ex)
      {
        session4messages.Content = ex.Message;
      }
    }

    private void beginReadSession1_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        int waitTime;
        if (int.TryParse(session1LockTimeout.Text, out waitTime))
          session[0].WaitForLockMilliseconds = waitTime;
        else
          session[0].WaitForLockMilliseconds = defaultWaitTime;
        session[0].BeginRead();
        session1status.Content = "In Read Only Transaction";
        session1messages.Content = "";
      }
      catch (Exception ex)
      {
        session1messages.Content = ex.Message;
      }
    }

    private void beginReadSession2_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        session[1].BeginRead();
        session2status.Content = "In Read Only Transaction";
        session2messages.Content = "";
      }
      catch (Exception ex)
      {
        session2messages.Content = ex.Message;
      }
    }

    private void beginReadSession3_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        session[2].BeginRead();
        session3status.Content = "In Read Only Transaction";
        session3messages.Content = "";
      }
      catch (Exception ex)
      {
        session3messages.Content = ex.Message;
      }
    }

    private void beginReadSession4_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        session[3].BeginRead();
        session4status.Content = "In Read Only Transaction";
        session4messages.Content = "";
      }
      catch (Exception ex)
      {
        session4messages.Content = ex.Message;
      }
    }

    private void beginUpdateSession1_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        session[0].BeginUpdate();
        session1status.Content = "In Update Transaction";
        session1messages.Content = "";
      }
      catch (Exception ex)
      {
        session1messages.Content = ex.Message;
      }
    }

    private void beginUpdateSession2_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        session[1].BeginUpdate();
        session2status.Content = "In Update Transaction";
        session2messages.Content = "";
      }
      catch (Exception ex)
      {
        session2messages.Content = ex.Message;
      }
    }

    private void beginUpdateSession3_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        session[2].BeginUpdate();
        session3status.Content = "In Update Transaction";
        session3messages.Content = "";
      }
      catch (Exception ex)
      {
        session3messages.Content = ex.Message;
      }
    }

    private void beginUpdateSession4_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        session[3].BeginUpdate();
        session4status.Content = "In Update Transaction";
        session4messages.Content = "";
      }
      catch (Exception ex)
      {
        session4messages.Content = ex.Message;
      }
    }

    private void commitSession1_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        session[0].Commit();
        session1status.Content = "Not In Transaction";
        session1messages.Content = "";
      }
      catch (Exception ex)
      {
        session1messages.Content = ex.Message;
      }
    }

    private void commitSession2_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        session[1].Commit();
        session2status.Content = "Not In Transaction";
        session2messages.Content = "";
      }
      catch (Exception ex)
      {
        session2messages.Content = ex.Message;
      }
    }

    private void commitSession3_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        session[2].Commit();
        session3status.Content = "Not In Transaction";
        session3messages.Content = "";
      }
      catch (Exception ex)
      {
        session3messages.Content = ex.Message;
      }
    }
    private void commitSession4_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        session[3].Commit();
        session4status.Content = "Not In Transaction";
        session4messages.Content = "";
      }
      catch (Exception ex)
      {
        session4messages.Content = ex.Message;
      }
    }

    private void abortSession1_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        session[0].Abort();
        session1status.Content = "Not In Transaction";
        session1messages.Content = "";
      }
      catch (Exception ex)
      {
        session1messages.Content = ex.Message;
      }
    }

    private void abortSession2_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        session[1].Abort();
        session2status.Content = "Not In Transaction";
        session2messages.Content = "";
      }
      catch (Exception ex)
      {
        session2messages.Content = ex.Message;
      }
    }

    private void abortSession3_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        session[2].Abort();
        session3status.Content = "Not In Transaction";
        session3messages.Content = "";
      }
      catch (Exception ex)
      {
        session3messages.Content = ex.Message;
      }
    }
    private void abortSession4_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        session[3].Abort();
        session4status.Content = "Not In Transaction";
        session4messages.Content = "";
      }
      catch (Exception ex)
      {
        session4messages.Content = ex.Message;
      }
    }

    private void add1db1Session1_Click(object sender, RoutedEventArgs e)
    {
      if (thread[0] != null)
        thread[0].Join();
      thread[0] = new Thread(() =>
      {
        try
        {
          Number pObj = (Number)session[0].Open(Oid.Encode(10, 1, 1), true, null, false, 0, 1);
          pObj.MyInt = pObj.MyInt + 1;
          Application.Current.Dispatcher.Invoke(new Action(() => { session1objectDb1.Content = pObj.ToString();
          session1messages.Content = ""; }));
        }
        catch (Exception ex)
        {
          Application.Current.Dispatcher.Invoke(new Action(() => { session1messages.Content = ex.Message; }));
        }
      });
      thread[0].Start();
    }

    private void add1db2Session1_Click(object sender, RoutedEventArgs e)
    {
       if (thread[0] != null)
        thread[0].Join();
       thread[0] = new Thread(() =>
       {
         try
         {
           Number pObj = (Number)session[0].Open(Oid.Encode(11, 1, 1), true, null, false, 0, 1);
           pObj.MyInt = pObj.MyInt + 1;
           Application.Current.Dispatcher.Invoke(new Action(() =>
           {
             session1objectDb2.Content = pObj.ToString();
             session1messages.Content = "";
           }));
         }
         catch (Exception ex)
         {
           Application.Current.Dispatcher.Invoke(new Action(() => { session1messages.Content = ex.Message; }));
         }
       });
      thread[0].Start();
    }

    private void add1db3Session1_Click(object sender, RoutedEventArgs e)
    {
       if (thread[0] != null)
        thread[0].Join();
       thread[0] = new Thread(() =>
       {
      try
      {
        Number pObj = (Number)session[0].Open(Oid.Encode(12, 1, 1), true, null, false, 0, 1);
        pObj.MyInt = pObj.MyInt + 1;
        Application.Current.Dispatcher.Invoke(new Action(() =>
        {
          session1objectDb3.Content = pObj.ToString();
          session1messages.Content = "";
        }));
      }
      catch (Exception ex)
      {
        Application.Current.Dispatcher.Invoke(new Action(() => { session1messages.Content = ex.Message; }));
      }
       });
       thread[0].Start();
    }

    private void add1db1Session2_Click(object sender, RoutedEventArgs e)
    {
      if (thread[1] != null && thread[1].IsAlive)
        thread[1].Join();
       thread[1] = new Thread(() =>
       {
      try
      {
        Number pObj = (Number)session[1].Open(Oid.Encode(10, 1, 1), true, null, false, 0, 1);
        pObj.MyInt = pObj.MyInt + 1;
        Application.Current.Dispatcher.Invoke(new Action(() =>
        {
          session2objectDb1.Content = pObj.ToString();
          session2messages.Content = "";
        }));
      }
      catch (Exception ex)
      {
        Application.Current.Dispatcher.Invoke(new Action(() => { session2messages.Content = ex.Message; }));
      }
       });
       thread[1].Start();
    }

    private void add1db2Session2_Click(object sender, RoutedEventArgs e)
    {
       if (thread[1] != null && thread[1].IsAlive)
        thread[1].Join();
       thread[1] = new Thread(() =>
       {
      try
      {
        Number pObj = (Number)session[1].Open(Oid.Encode(11, 1, 1), true, null, false, 0, 1);
        pObj.MyInt = pObj.MyInt + 1;
        Application.Current.Dispatcher.Invoke(new Action(() =>
         {
           session2objectDb2.Content = pObj.ToString();
           session2messages.Content = "";
         }));
      }
      catch (Exception ex)
      {
        Application.Current.Dispatcher.Invoke(new Action(() => { session2messages.Content = ex.Message; }));
      }
       });
       thread[1].Start();
    }

    private void add1db3Session2_Click(object sender, RoutedEventArgs e)
    {
      if (thread[1] != null && thread[1].IsAlive)
        thread[1].Join();
       thread[1] = new Thread(() =>
       {
      try
      {
        Number pObj = (Number)session[1].Open(Oid.Encode(12, 1, 1), true, null, false, 0, 1);
        pObj.MyInt = pObj.MyInt + 1;
        Application.Current.Dispatcher.Invoke(new Action(() =>
        {
          session2objectDb3.Content = pObj.ToString();
          session2messages.Content = "";
        }));
      }
      catch (Exception ex)
      {
        Application.Current.Dispatcher.Invoke(new Action(() => { session2messages.Content = ex.Message; }));
      }
       });
       thread[1].Start();
    }

    private void add1db1Session3_Click(object sender, RoutedEventArgs e)
    {
      if (thread[2] != null && thread[2].IsAlive)
        thread[2].Join();
      thread[2] = new Thread(() =>
      {
        try
        {
          Number pObj = (Number)session[2].Open(Oid.Encode(10, 1, 1), true, null, false, 0, 1);
          pObj.MyInt = pObj.MyInt + 1;
          Application.Current.Dispatcher.Invoke(new Action(() =>
          {
            session3objectDb1.Content = pObj.ToString();
            session3messages.Content = "";
          }));
        }
        catch (Exception ex)
        {
          Application.Current.Dispatcher.Invoke(new Action(() => { session3messages.Content = ex.Message; }));
        }
      });
      thread[2].Start();
    }

    private void add1db2Session3_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        Number pObj = (Number)session[2].Open(Oid.Encode(11, 1, 1), true, null, false, 0, 1);
        pObj.MyInt = pObj.MyInt + 1;
        session3objectDb2.Content = pObj.ToString();
        session3messages.Content = "";
      }
      catch (Exception ex)
      {
        session3messages.Content = ex.Message;
      }
    }

    private void add1db3Session3_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        Number pObj = (Number)session[2].Open(Oid.Encode(12, 1, 1), true, null, false, 0, 1);
        pObj.MyInt = pObj.MyInt + 1;
        session3objectDb3.Content = pObj.ToString();
        session3messages.Content = "";
      }
      catch (Exception ex)
      {
        session3messages.Content = ex.Message;
      }
    }

    private void add1db1Session4_Click(object sender, RoutedEventArgs e)
    {
      if (thread[3] != null && thread[3].IsAlive)
        thread[3].Join();
      thread[3] = new Thread(() =>
      {
        try
        {
          Number pObj = (Number)session[3].Open(Oid.Encode(10, 1, 1), true, null, false, 0, 1);
          pObj.MyInt = pObj.MyInt + 1;
          Application.Current.Dispatcher.Invoke(new Action(() =>
          {
            session4objectDb1.Content = pObj.ToString();
            session4messages.Content = "";
          }));
        }
        catch (Exception ex)
        {
          Application.Current.Dispatcher.Invoke(new Action(() => { session4messages.Content = ex.Message; }));
        }
      });
      thread[3].Start();
    }

    private void add1db2Session4_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        Number pObj = (Number)session[3].Open(Oid.Encode(11, 1, 1), true, null, false, 0, 1);
        pObj.MyInt = pObj.MyInt + 1;
        session4objectDb2.Content = pObj.ToString();
        session4messages.Content = "";
      }
      catch (Exception ex)
      {
        session4messages.Content = ex.Message;
      }
    }

    private void add1db3Session4_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        Number pObj = (Number)session[3].Open(Oid.Encode(12, 1, 1), true, null, false, 0, 1);
        pObj.MyInt = pObj.MyInt + 1;
        session4objectDb3.Content = pObj.ToString();
        session4messages.Content = "";
      }
      catch (Exception ex)
      {
        session4messages.Content = ex.Message;
      }
    }

    private void db1ReadSession1_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        Number pObj = (Number)session[0].Open(Oid.Encode(10, 1, 1), false, null, false, 0, 1);
        session1objectDb1.Content = pObj.ToString();
        session1messages.Content = "";
      }
      catch (Exception ex)
      {
        session1messages.Content = ex.Message;
      }
    }

    private void db2ReadSession1_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        Number pObj = (Number)session[0].Open(Oid.Encode(11, 1, 1), false, null, false, 0, 1);
        session1objectDb2.Content = pObj.ToString();
        session1messages.Content = "";
      }
      catch (Exception ex)
      {
        session1messages.Content = ex.Message;
      }
    }

    private void db3ReadSession1_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        Number pObj = (Number)session[0].Open(Oid.Encode(12, 1, 1), false, null, false, 0, 1);
        session1objectDb3.Content = pObj.ToString();
        session1messages.Content = "";
      }
      catch (Exception ex)
      {
        session1messages.Content = ex.Message;
      }
    }


    private void db1ReadSession2_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        Number pObj = (Number)session[1].Open(Oid.Encode(10, 1, 1), false, null, false, 0, 1);
        session2objectDb1.Content = pObj.ToString();
        session2messages.Content = "";
      }
      catch (Exception ex)
      {
        session2messages.Content = ex.Message;
      }
    }

    private void db2ReadSession2_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        Number pObj = (Number)session[1].Open(Oid.Encode(11, 1, 1), false, null, false, 0, 1);
        session2objectDb2.Content = pObj.ToString();
        session2messages.Content = "";
      }
      catch (Exception ex)
      {
        session2messages.Content = ex.Message;
      }
    }

    private void db3ReadSession2_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        Number pObj = (Number)session[1].Open(Oid.Encode(12, 1, 1), false, null, false, 0, 1);
        session2objectDb3.Content = pObj.ToString();
        session2messages.Content = "";
      }
      catch (Exception ex)
      {
        session2messages.Content = ex.Message;
      }
    }

    private void db1ReadSession3_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        Number pObj = (Number)session[2].Open(Oid.Encode(10, 1, 1), false, null, false, 0, 1);
        session3objectDb1.Content = pObj.ToString();
        session3messages.Content = "";
      }
      catch (Exception ex)
      {
        session3messages.Content = ex.Message;
      }
    }

    private void db2ReadSession3_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        Number pObj = (Number)session[2].Open(Oid.Encode(11, 1, 1), false, null, false, 0, 1);
        session3objectDb2.Content = pObj.ToString();
        session3messages.Content = "";
      }
      catch (Exception ex)
      {
        session3messages.Content = ex.Message;
      }
    }

    private void db3ReadSession3_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        Number pObj = (Number)session[2].Open(Oid.Encode(12, 1, 1), false, null, false, 0, 1);
        session3objectDb3.Content = pObj.ToString();
        session3messages.Content = "";
      }
      catch (Exception ex)
      {
        session3messages.Content = ex.Message;
      }
    }


    private void db1ReadSession4_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        Number pObj = (Number)session[3].Open(Oid.Encode(10, 1, 1), false, null, false, 0, 1);
        session4objectDb1.Content = pObj.ToString();
        session4messages.Content = "";
      }
      catch (Exception ex)
      {
        session4messages.Content = ex.Message;
      }
    }

    private void db2ReadSession4_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        Number pObj = (Number)session[3].Open(Oid.Encode(11, 1, 1), false, null, false, 0, 1);
        session4objectDb2.Content = pObj.ToString();
        session4messages.Content = "";
      }
      catch (Exception ex)
      {
        session4messages.Content = ex.Message;
      }
    }

    private void db3ReadSession4_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        Number pObj = (Number)session[3].Open(Oid.Encode(12, 1, 1), false, null, false, 0, 1);
        session4objectDb3.Content = pObj.ToString();
        session4messages.Content = "";
      }
      catch (Exception ex)
      {
        session4messages.Content = ex.Message;
      }
    }

    private void db1UpdateSession1_Click(object sender, RoutedEventArgs e)
    {
      if (thread[0] != null && thread[0].IsAlive)
        thread[0].Join();
      thread[0] = new Thread(() =>
      {
        try
        {
          Number pObj = (Number)session[0].Open(Oid.Encode(10, 1, 1), true, null, false, 0, 1);
          pObj.Update();
          Application.Current.Dispatcher.Invoke(new Action(() =>
          {
            session1objectDb1.Content = pObj.ToString();
            session1messages.Content = "";
          }));
        }
        catch (Exception ex)
        {
          Application.Current.Dispatcher.Invoke(new Action(() => { session1messages.Content = ex.Message; }));
        }
      });
      thread[0].Start();
    }

    private void db2UpdateSession1_Click(object sender, RoutedEventArgs e)
    {
      if (thread[0] != null && thread[0].IsAlive)
        thread[0].Join();
      thread[0] = new Thread(() =>
      {
        try
        {
          Number pObj = (Number)session[0].Open(Oid.Encode(11, 1, 1), true, null, false, 0, 1);
          pObj.Update();
          Application.Current.Dispatcher.Invoke(new Action(() =>
          {
            session1objectDb1.Content = pObj.ToString();
            session1messages.Content = "";
          }));
        }
        catch (Exception ex)
        {
          Application.Current.Dispatcher.Invoke(new Action(() => { session1messages.Content = ex.Message; }));
        }
      });
      thread[0].Start();
    }

    private void db3UpdateSession1_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        Number pObj = (Number)session[0].Open(Oid.Encode(12, 1, 1), true, null, false, 0, 1);
        session1objectDb3.Content = pObj.ToString();
        session1messages.Content = "";
      }
      catch (Exception ex)
      {
        session1messages.Content = ex.Message;
      }
    }


    private void db1UpdateSession2_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        Number pObj = (Number)session[1].Open(Oid.Encode(10, 1, 1), true, null, false, 0, 1);
        session2objectDb1.Content = pObj.ToString();
        session2messages.Content = "";
      }
      catch (Exception ex)
      {
        session2messages.Content = ex.Message;
      }
    }

    private void db2UpdateSession2_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        Number pObj = (Number)session[1].Open(Oid.Encode(11, 1, 1), true, null, false, 0, 1);
        session2objectDb2.Content = pObj.ToString();
        session2messages.Content = "";
      }
      catch (Exception ex)
      {
        session2messages.Content = ex.Message;
      }
    }

    private void db3UpdateSession2_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        Number pObj = (Number)session[1].Open(Oid.Encode(12, 1, 1), true, null, false, 0, 1);
        session2objectDb3.Content = pObj.ToString();
        session2messages.Content = "";
      }
      catch (Exception ex)
      {
        session2messages.Content = ex.Message;
      }
    }

    private void db1UpdateSession3_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        Number pObj = (Number)session[2].Open(Oid.Encode(10, 1, 1), true, null, false, 0, 1);
        session3objectDb1.Content = pObj.ToString();
        session3messages.Content = "";
      }
      catch (Exception ex)
      {
        session3messages.Content = ex.Message;
      }
    }

    private void db2UpdateSession3_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        Number pObj = (Number)session[2].Open(Oid.Encode(11, 1, 1), true, null, false, 0, 1);
        session3objectDb2.Content = pObj.ToString();
        session3messages.Content = "";
      }
      catch (Exception ex)
      {
        session3messages.Content = ex.Message;
      }
    }

    private void db3UpdateSession3_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        Number pObj = (Number)session[2].Open(Oid.Encode(12, 1, 1), true, null, false, 0, 1);
        session3objectDb3.Content = pObj.ToString();
        session3messages.Content = "";
      }
      catch (Exception ex)
      {
        session3messages.Content = ex.Message;
      }
    }


    private void db1UpdateSession4_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        Number pObj = (Number)session[3].Open(Oid.Encode(10, 1, 1), true, null, false, 0, 1);
        session4objectDb1.Content = pObj.ToString();
        session4messages.Content = "";
      }
      catch (Exception ex)
      {
        session4messages.Content = ex.Message;
      }
    }

    private void db2UpdateSession4_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        Number pObj = (Number)session[3].Open(Oid.Encode(11, 1, 1), true, null, false, 0, 1);
        session4objectDb2.Content = pObj.ToString();
        session4messages.Content = "";
      }
      catch (Exception ex)
      {
        session4messages.Content = ex.Message;
      }
    }

    private void db3UpdateSession4_Click(object sender, RoutedEventArgs e)
    {
      try
      {
        Number pObj = (Number)session[3].Open(Oid.Encode(12, 1, 1), true, null, false, 0, 1);
        session4objectDb3.Content = pObj.ToString();
        session4messages.Content = "";
      }
      catch (Exception ex)
      {
        session4messages.Content = ex.Message;
      }
    }
  }
}