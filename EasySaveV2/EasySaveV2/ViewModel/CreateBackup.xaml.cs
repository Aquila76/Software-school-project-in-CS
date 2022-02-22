using EasySafe.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using Menu = EasySafe.Model.Menu;

namespace EasySaveV2.View
{
    /// <summary>
    /// Logique d'interaction pour CreateBackup.xaml
    /// </summary>
    public partial class CreateBackup : Page
    {

        private static CreateBackup home = null;

        private CreateBackup()
        {
            InitializeComponent();
            home = this;

            this.Translate();
            List<BackupType> list = Enum.GetValues(typeof(BackupType)).Cast<BackupType>().ToList();
            foreach (BackupType item in list)
            {
                this.BackupTypeSelect.Items.Add(item);
            }
        }

        public static CreateBackup GetPage()
        {
            if (home != null)
                return home;
            return new CreateBackup();
        }

        internal void Translate()
        {
            Translator translator = Translator.GetTranslator();
            this.BackupNameLabel.Content = translator.Translate(Menu.BackupName);
            this.SourcePathLabel.Content = translator.Translate(Menu.BackupSource);
            this.TargetPathLabel.Content = translator.Translate(Menu.BackupTarget);
            this.BackupTypeLabel.Content = translator.Translate(Menu.BackupType);
            this.CancelButton.Content = translator.Translate(Menu.Cancel);
            this.CreateButton.Content = translator.Translate(Menu.Create);
            this.SourcePathButton.Content = translator.Translate(Menu.Browse);
            this.TargetPathButton.Content = translator.Translate(Menu.Browse);
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

        private void CreateBackupButton(object sender, RoutedEventArgs e)
        {
            Backup backup = new Backup();
            backup.Name = this.BackupName.Text;
            backup.Source = this.SourceTextBox.Text;
            backup.Target = this.TargetTextBox.Text;

            backup.BackupType = (BackupType)this.BackupTypeSelect.SelectedItem;
            List<Backup> list = HomePage.LoadBackup();

            list.Add(backup);
            HomePage.SaveBackup(list);


            HomePage.GetPage().Refresh();

            this.NavigationService.GoBack();

        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            this.NavigationService.GoBack();
        }

        private void BrowseFolder(object sender, RoutedEventArgs e)
        {

        }
    }
}
