using EasySafe.Model;
using System;
using System.Collections.Generic;
using System.Linq;
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
using Menu = EasySafe.Model.Menu;

namespace EasySaveV2.View
{
    /// <summary>
    /// Logique d'interaction pour EditBackup.xaml
    /// </summary>
    public partial class EditBackup : Page
    {

        private static EditBackup home = null;
        private Backup Backup;


        private EditBackup(Backup backup)
        {
            InitializeComponent();
            home = this;

            this.Translate();
            List<BackupType> list = Enum.GetValues(typeof(BackupType)).Cast<BackupType>().ToList();
            foreach (BackupType item in list)
            {
                this.BackupTypeSelect.Items.Add(item);
            }
            this.Backup = backup;
            this.Update(backup);
        }

        public static EditBackup GetPage(Backup backup)
        {
            if (home != null)
            {
                home.Update(backup);
                return home;

            }
            return new EditBackup(backup);
        }

        internal void Translate()
        {
            Translator translator = Translator.GetTranslator();
            this.BackupNameLabel.Content = translator.Translate(Menu.BackupName);
            this.SourcePathLabel.Content = translator.Translate(Menu.BackupSource);
            this.TargetPathLabel.Content = translator.Translate(Menu.BackupTarget);
            this.BackupTypeLabel.Content = translator.Translate(Menu.BackupType);
            this.CancelButton.Content = translator.Translate(Menu.Cancel);
            this.EditButton.Content = translator.Translate(Menu.Edit);
            this.SourcePathButton.Content = translator.Translate(Menu.Browse);
            this.TargetPathButton.Content = translator.Translate(Menu.Browse);

        }

        public void Update(Backup backup)
        {
            this.Backup = backup;
            BackupName.Text = backup.Name;
            SourceTextBox.Text = backup.Source;
            TargetTextBox.Text = backup.Target;
            BackupTypeSelect.SelectedItem = backup.BackupType;
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            this.NavigationService.GoBack();
        }

        private void SaveBackup(object sender, RoutedEventArgs e)
        {
            this.Backup.Name = this.BackupName.Text;
            this.Backup.Source = this.SourceTextBox.Text;
            this.Backup.Target = this.TargetTextBox.Text;
            this.Backup.BackupType = (BackupType)this.BackupTypeSelect.SelectedItem;

            HomePage page = HomePage.GetPage();
            int index = page.ListBackup.IndexOf(this.Backup);
            page.ListBackup[index] = this.Backup;

            HomePage.SaveBackup(page.ListBackup);
            page.Refresh();
            this.NavigationService.GoBack();

        }

        private void BrowseSourceButton(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                SourceTextBox.Text = fbd.SelectedPath;
            }
        }

        private void BrowseTargetButton(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                TargetTextBox.Text = fbd.SelectedPath;
            }
        }
    }
}
