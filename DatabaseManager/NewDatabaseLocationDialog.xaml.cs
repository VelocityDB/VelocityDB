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
using VelocityDb;

namespace DatabaseManager
{
  /// <summary>
  /// Interaction logic for NewDatabaseLocationDialog.xaml
  /// </summary>
  public partial class NewDatabaseLocationDialog : Window
  {
    DatabaseLocation m_existingLocation;
    DatabaseLocationMutable m_newLocation;
    public NewDatabaseLocationDialog(DatabaseLocationMutable newLocation, DatabaseLocation existingLocation)
    {
      InitializeComponent();
      m_existingLocation = existingLocation;
      if (m_existingLocation != null)
        Title = "Edit " + m_existingLocation.ToString();
      m_newLocation = newLocation;
      if (m_newLocation.IsBackupLocation)
      {
        BackupOfLocationLabel.Content = "Backup";
        BackupOfLocationBox.IsReadOnly = true;
      }
      base.DataContext = this;
    }

    public CollectionView DatabaseLocations
    {
      get
      {
        var locationsList = m_newLocation.Session.DatabaseLocations.ToList();
        if (m_existingLocation != null)
          locationsList.Remove(m_existingLocation);
        locationsList = locationsList.Where(loc => loc.IsBackupLocation == false).ToList();
        return new CollectionView(locationsList);
      }
    }
    public CollectionView Compression
    {
      get
      {
        return new CollectionView(Enum.GetValues(typeof(PageInfo.compressionKind)));
      }
    }

    public CollectionView Encryption
    {
      get
      {
        return new CollectionView(Enum.GetValues(typeof(PageInfo.encryptionKind)));
      }
    }

    public string Directory
    {
      get
      {
        return m_newLocation.DirectoryPath;
      }
      set
      {
        m_newLocation.DirectoryPath = value;
      }
    }

    public string Host
    {
      get
      {
        return m_newLocation.HostName;
      }
      set
      {
        m_newLocation.HostName = value;
      }
    }

    public UInt32 StartDatabaseNumber
    {
      get
      {
        return m_newLocation.StartDatabaseNumber;
      }
      set
      {
        m_newLocation.StartDatabaseNumber = value;
      }
    }
    public UInt32 EndDatabaseNumber
    {
      get
      {
        return m_newLocation.EndDatabaseNumber;
      }
      set
      {
        m_newLocation.EndDatabaseNumber = value;
      }
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

    public bool IsBackupLocation
    {
      get
      {
        return m_newLocation.IsBackupLocation;
      }
      set
      {
        m_newLocation.IsBackupLocation = value;
      }
    }

    void BtnOkClick(object pSender, RoutedEventArgs pEvents)
    {
      if (m_newLocation.IsBackupLocation)
      {
        DatabaseLocation backedUpLocation = (DatabaseLocation)BackupOfLocationBox.SelectedValue;
        m_newLocation.BackupOfOrForLocation = backedUpLocation;
      }
      m_newLocation.CompressPages = (PageInfo.compressionKind)CompressionBox.SelectedValue;
      m_newLocation.PageEncryption = (PageInfo.encryptionKind)EncryptionBox.SelectedValue;
      m_newLocation.HostName = HostTextBox.Text;
      m_newLocation.DirectoryPath = DirectoryTextBox.Text;
      DialogResult = true;
    }
  }
}
