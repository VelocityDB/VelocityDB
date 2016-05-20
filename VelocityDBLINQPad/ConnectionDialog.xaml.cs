using LINQPad.Extensibility.DataContext;
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
using VelocityDBAccess;

namespace VelocityDB.LINQPad
{
    /// <summary>
    /// Interaction logic for ConnectDialog.xaml
    /// </summary>
    public partial class ConnectionDialog : Window
    {
        VelocityDBProperties properties;

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
            // Get seesion type according to pressed radiobutton.
            if (RadioNoServ.IsChecked == true)
            {
                properties.SessionType = SessionInfo.SessionTypeEnum.NoServerSession;
            }
            else if (RadioNoShared.IsChecked == true)
            {
                properties.SessionType = SessionInfo.SessionTypeEnum.NoServerSharedSession;
            }
            else if (RadioServer.IsChecked == true)
            {
                properties.SessionType = SessionInfo.SessionTypeEnum.ServerClientSession;
            }
            // Get list of classes filenames into VelocityDBProperties.
            Char lSeparator = VelocityDBProperties.Separator;
            properties.ClassesFilenames = String.Join(lSeparator.ToString(),
                (from ListViewItem lItem in AssemblyList.Items
                 select lItem.Content));
            // Get list of dependency filenames into VelocityDBProperties.
            properties.DependencyFiles = String.Join(lSeparator.ToString(),
                (from ListViewItem lItem in DependencyList.Items
                 select lItem.Content));

            DialogResult = true;
        }

        public ConnectionDialog(IConnectionInfo pCxInfo)
        {
            DataContext = properties = new VelocityDBProperties(pCxInfo);
            InitializeComponent();
            // Check current session type.
            switch (properties.SessionType)
            {
                case SessionInfo.SessionTypeEnum.NoServerSession:
                    RadioNoServ.IsChecked = true;
                    break;
                case SessionInfo.SessionTypeEnum.NoServerSharedSession:
                    RadioNoShared.IsChecked = true;
                    break;
                case SessionInfo.SessionTypeEnum.ServerClientSession:
                    RadioServer.IsChecked = true;
                    HostTextBox.IsEnabled = true;
                    break;
                default:
                    RadioNoServ.IsChecked = true;
                    break;
            }
            // Fill current classes filenames.
            foreach(string lClassFilename in properties.ClassesFilenamesArray)
            {
                if (lClassFilename.Length < 1) continue;
                ListViewItem lItem = new ListViewItem();
                lItem.Content = lClassFilename;
                AssemblyList.Items.Add(lItem);
            }
            // Fill current dependencies filename.
            foreach (string lDependency in properties.DependencyFilesArray)
            {
                if (lDependency.Length < 1) continue;
                ListViewItem lItem = new ListViewItem();
                lItem.Content = lDependency;
                DependencyList.Items.Add(lItem);
            }
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
