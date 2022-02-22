using System.Collections.Generic;
using System.Windows.Controls;
using EasySafe.Model;
using System.Reflection;
using Newtonsoft.Json;
using System.IO;
using System;
using Menu = EasySafe.Model.Menu;
using System.Windows;

namespace EasySaveV2.View
{
    /// <summary>
    /// Logique d'interaction pour HomePage.xaml
    /// </summary>
    public partial class HomePage : Page
    {
        private static HomePage home = null;

        internal List<Backup> ListBackup { get; set; }

        public List<string> ListExtSoft { get; set; }

        internal Backup _selectedBackup;

        public Backup SelectedBackup
        {
            get { return this._selectedBackup; }
            set
            {
                this._selectedBackup = value;
                UpdateSelectedBackup();
            }
        }

        private HomePage()
        {
            InitializeComponent();
            home = this;
            this.Translate();

            ListBoxBackup.SelectionChanged += new SelectionChangedEventHandler(ListBoxBackup_SelectedValueChanged);
            this.Refresh();
            
        }

        public static HomePage GetPage()
        {
            if (home != null)
                return home;
            return new HomePage();
        }

        internal void Translate()
        {
            Translator translator = Translator.GetTranslator();
            this.BackupNameLabel.Content = translator.Translate(Menu.BackupName);
            this.BackupTypeLabel.Content = translator.Translate(Menu.BackupType);
            this.BackupSourceLabel.Content = translator.Translate(Menu.BackupSource);
            this.BackupTargetLabel.Content = translator.Translate(Menu.BackupTarget);
            this.BackupTotalFilesToCopyLabel.Content = translator.Translate(Menu.NbFilesToCopy);
            this.BackupTotalFilesSizeLabel.Content = translator.Translate(Menu.BackupSize);
            this.NbFilesRemainingLabel.Content = translator.Translate(Menu.NbFilesRemaining);
            this.BackupSizeRemainingLabel.Content = translator.Translate(Menu.BackupSizeRemaining);

        }

        internal static List<Backup> LoadBackup()
        {
            string appPath = Assembly.GetExecutingAssembly().CodeBase;
            List<Backup> ListBackup;

            string tempPath = "";
            for (int i = 0; i < appPath.Length; i++)
            {
                if (i > 7)
                {
                    tempPath += appPath[i].ToString();
                }
            }
            string path = System.IO.Path.GetDirectoryName(tempPath) + @"\Backup.json";

            if (!File.Exists(path))
            {
                File.Create(path);
            }

            if (new FileInfo(path).Length != 0)
            {
                var jsonData = File.ReadAllText(path);
                ListBackup = JsonConvert.DeserializeObject<List<Backup>>(jsonData);
            }
            else
            {
                ListBackup = new List<Backup>();
            }

            return ListBackup;
        }

        internal static void SaveBackup(List<Backup> list)
        {
            string appPath = Assembly.GetExecutingAssembly().CodeBase;

            string tempPath = "";
            for (int i = 0; i < appPath.Length; i++)
            {
                if (i > 7)
                {
                    tempPath += appPath[i].ToString();
                }
            }
            string path = System.IO.Path.GetDirectoryName(tempPath) + @"\Backup.json";

            var jsonData = JsonConvert.SerializeObject(list, Formatting.Indented);
            Console.WriteLine(path);
            File.WriteAllText(path, jsonData);
        }

        private void ClearButton(object sender, RoutedEventArgs e)
        {
            ExecSaveTextBox.Clear();
        }

        public void ExecSaveAppendNewLine(string line)
        {
            Dispatcher.Invoke(() => { 
                ExecSaveTextBox.AppendText(line + "\n");
                ExecSaveTextBox.ScrollToEnd();
            });
        }

        internal void ListBoxBackup_SelectedValueChanged(object sender, EventArgs e)
        {
            int index = ListBoxBackup.SelectedIndex;
            if (index >= 0 && index < ListBoxBackup.Items.Count)
            {
                this._selectedBackup = this.ListBackup[index];
                this.UpdateSelectedBackup();
            }
            else
            {
                this.SelectedBackup = null;
            }

        }

        internal void UpdateSelectedBackup()
        {
            if (this.SelectedBackup != null)
            {
                Dispatcher.Invoke(() => { BackupName.Text = SelectedBackup.Name; });
                Dispatcher.Invoke(() => { BackupSource.Text = SelectedBackup.Source; });
                Dispatcher.Invoke(() => { BackupTarget.Text = SelectedBackup.Target; });
                Dispatcher.Invoke(() => { BackupType.Text = SelectedBackup.BackupType.ToString(); });

                TotalFilesToCopyText(SelectedBackup.TotalFilesToCopy.ToString());
                TotalFilesSizeText(SelectedBackup.TotalFilesSize.ToString());
                NbFilesLeftToDoText(SelectedBackup.NbFilesLeftToDo.ToString());
                FilesLeftToDoSizeText(SelectedBackup.FilesLeftToDoSize.ToString());
            }
        }

        public void TotalFilesToCopyText(string value)
        {
            Dispatcher.Invoke(() => { BackupTotalFilesToCopy.Text = value; });
        }
        public void TotalFilesSizeText(string value)
        {
            Dispatcher.Invoke(() => { BackupTotalFilesSize.Text = value; });
        }
        public void NbFilesLeftToDoText(string value)
        {
            Dispatcher.Invoke(() => { BackupNbFilesLeftToDo.Text = value; });
        }
        public void FilesLeftToDoSizeText(string value)
        {
            Dispatcher.Invoke(() => { BackupFilesLeftToDoSize.Text = value; });
        }

        internal void Refresh()
        {
            this.ListBackup = HomePage.LoadBackup();
            ListBoxBackup.Items.Clear();
            foreach (Backup backup in this.ListBackup)
            {
                ListBoxBackup.Items.Add(backup.ToString());
            }
        }

        private void PlayOneBackupButton(object sender, RoutedEventArgs e)
        {
            SelectedBackup.ResumeBackup();
            HomePage.GetPage().ExecSaveAppendNewLine(Translator.GetTranslator().TranslateLogPlayPauseStop(LogPlayPauseStop.CopyResumed));
        }

        private void PauseOneBackupButton(object sender, RoutedEventArgs e)
        {
            SelectedBackup.PauseBackup();
            HomePage.GetPage().ExecSaveAppendNewLine(Translator.GetTranslator().TranslateLogPlayPauseStop(LogPlayPauseStop.CopyPaused));
        }

        private void StopOneBackupButton(object sender, RoutedEventArgs e)
        {
            SelectedBackup.Active = false;
            HomePage.GetPage().ExecSaveAppendNewLine(Translator.GetTranslator().TranslateLogPlayPauseStop(LogPlayPauseStop.CopyStopped));
        }

    }
}
