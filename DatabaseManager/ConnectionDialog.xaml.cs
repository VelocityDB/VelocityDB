using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Globalization;
using DatabaseManager.Model;

namespace DatabaseManager
{
  /// <summary>
  /// Interaction logic for ConnectDialog.xaml
  /// </summary>
  public partial class ConnectionDialog : Window
  {
    FederationInfo m_federationInfo;

    void AddClass(object pSender, RoutedEventArgs pEvents)
    {
      var lDialog = new Microsoft.Win32.OpenFileDialog()
      {
        Title = "Choose classes Assembly",
        CheckFileExists = true,
        Filter = "Assemblies|*.dll;*.exe;*.winmd|All Files|*.*",
        Multiselect = true
      };
      if (lDialog.ShowDialog() == true)
      {
        // Check if the assembly is already on the list.
        foreach (string lName in lDialog.FileNames)
        {
          var lSameLib = (
              from ListViewItem lItem in AssemblyList.Items
              where lItem.Content.Equals(lName)
              select lItem);
          if (lSameLib.Count() == 0)
          {
            ListViewItem lEntry = new ListViewItem();
            lEntry.Content = lName;
            AssemblyList.Items.Add(lEntry);
          }
        }
      }
    }

    void AddDependency(object pSender, RoutedEventArgs pEvents)
    {
      var lDialog = new Microsoft.Win32.OpenFileDialog()
      {
        Title = "Choose dependency Assembly",
        CheckFileExists = true,
        Filter = "Assemblies|*.dll;*.exe;*.winmd|All Files|*.*",
        Multiselect = true
      };
      if (lDialog.ShowDialog() == true)
      {
        foreach (string lName in lDialog.FileNames)
        {
          // Check if the dependency is already on the list.
          var lSameLib = (
              from ListViewItem lItem in DependencyList.Items
              where lItem.Content.Equals(lName)
              select lItem);
          if (lSameLib.Count() == 0)
          {
            ListViewItem lEntry = new ListViewItem();
            lEntry.Content = lName;
            DependencyList.Items.Add(lEntry);
          }
        }
      }
    }
    private void AssemblySelection(Object sender, SelectionChangedEventArgs e)
    {
      BtnRemoveAssembly.IsEnabled = AssemblyList.SelectedItems.Count > 0;
    }

    void BrowseDBDir(object pSender, RoutedEventArgs pEvents)
    {
      var lDialog = new System.Windows.Forms.FolderBrowserDialog()
      {
        Description = "Choose DB Folder",
      };
      if (lDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
      {
        DBDirTextBox.Text = lDialog.SelectedPath;
      }
    }

    void BtnOkClick(object pSender, RoutedEventArgs pEvents)
    {
      m_federationInfo.UsesServerClient = (bool)RadioServer.IsChecked;
      m_federationInfo.ClassesFilenames = (from ListViewItem lItem in AssemblyList.Items select (string)lItem.Content).ToArray();
      m_federationInfo.DependencyFiles = (from ListViewItem lItem in DependencyList.Items select (string)lItem.Content).ToArray();
      m_federationInfo.SystemDbsPath = DBDirTextBox.Text;
      m_federationInfo.HostName = HostTextBox.Text;
      //.WindowsAuthentication = 
      DialogResult = true;
    }

    public ConnectionDialog(FederationInfo federationInfo)
    {
      InitializeComponent();
      m_federationInfo = federationInfo;
      RadioNoServ.IsChecked = true;
    }

    private void DependencySelection(Object sender, SelectionChangedEventArgs e)
    {
      BtnRemoveDependency.IsEnabled = DependencyList.SelectedItems.Count > 0;
    }

    void DisableServerParameters(object pSender, RoutedEventArgs pEvents)
    {
      HostTextBox.IsEnabled = false;
    }

    void EnableServerParameters(object pSender, RoutedEventArgs pEvents)
    {
      HostTextBox.IsEnabled = true;
    }

    void RemoveAssembly(object pSender, RoutedEventArgs pEvents)
    {
      while (AssemblyList.SelectedItems.Count > 0)
      {
        AssemblyList.Items.Remove(AssemblyList.SelectedItem);
      }
    }

    void RemoveDependency(object pSender, RoutedEventArgs pEvents)
    {
      while (DependencyList.SelectedItems.Count > 0)
      {
        DependencyList.Items.Remove(DependencyList.SelectedItem);
      }
    }
  }
}
