using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace DatabaseManager
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application
  {
    void AppStartup(object sender, StartupEventArgs e)
    {
      //Thread.Sleep(10000);
      string dbFilePath = null;
      if (e.Args.Length > 0)
        dbFilePath = e.Args[0];
      MainWindow mainWindow = new MainWindow(dbFilePath);
      mainWindow.Show();
    }
  }
}
