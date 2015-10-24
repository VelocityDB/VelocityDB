using DatabaseManager.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DatabaseManager
{
  /// <summary>
  /// Interaction logic for RestoreDialog.xaml
  /// </summary>
  public partial class RestoreDialog : Window
  {
    DatabaseLocationMutable m_newLocation;
    public RestoreDialog(DatabaseLocationMutable newLocation)
    {
      InitializeComponent();
      m_newLocation = newLocation;
    }

    void BrowseLocationDir(object pSender, RoutedEventArgs pEvents)
    {
      var lDialog = new System.Windows.Forms.FolderBrowserDialog()
      {
        Description = "Choose DatabaseLocation Folder",
      };
      if (lDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
      {
        DirectoryTextBox.Text = lDialog.SelectedPath;
      }
    }

    void BtnOkClick(object pSender, RoutedEventArgs pEvents)
    {
      m_newLocation.HostName = HostTextBox.Text;
      m_newLocation.DirectoryPath = DirectoryTextBox.Text;
      DialogResult = true;
    }
  }
}
