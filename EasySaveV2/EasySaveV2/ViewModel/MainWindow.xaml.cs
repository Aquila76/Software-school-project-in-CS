using EasySafe.Model;
using EasySaveV2.View;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Menu = EasySafe.Model.Menu;

namespace EasySaveV2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private AppSettings settings;
        public MainWindow()
        {
            InitializeComponent();
            this.settings = AppSettings.GetAppSettings();

            switch (settings.Language)
            {
                case "1":
                    FrenchMenu.IsChecked = true;
                    break;
                case "2":
                     EnglishMenu.IsChecked = true;
                    break;
            }

            Main.Content = HomePage.GetPage();
            this.Translate();

        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Process.GetCurrentProcess().Kill();
        }

        private void UpdateTranslation()
        {
            this.Translate();
            HomePage.GetPage().Translate();
            CreateBackup.GetPage().Translate();
            EditBackup.GetPage(new Backup()).Translate();
            Settings.GetPage(this.settings).Translate();
        }

        private void Translate()
        {
            Translator translator = Translator.GetTranslator();

            

            this.ExecuteAllButton.Content = translator.Translate(Menu.ExecuteAll);
            this.CreateBackupButton.Content = translator.Translate(Menu.CreateBackup);
            this.EditBackupButton.Content = translator.Translate(Menu.EditBackup);
            this.DeleteBackupButton.Content = translator.Translate(Menu.DeleteBackup);
            this.ExecuteOneBackupButton.Content = translator.Translate(Menu.ExecuteOne);

            this.MenuOpenLog.Header = translator.Translate(Menu.OpenLog);
            this.MenuOpenStateLog.Header = translator.Translate(Menu.OpenLogState);
            this.MenuExit.Header = translator.Translate(Menu.Exit);
        }

        private void ExitApp(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }
        
        private void OpenLog(object sender, RoutedEventArgs e)
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
            string path = System.IO.Path.GetDirectoryName(tempPath) +"\\logs";

            Process.Start("explorer.exe",path);
        }
        private void OpenLogState(object sender, RoutedEventArgs e)
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
            string path = System.IO.Path.GetDirectoryName(tempPath) + "\\logs_state";
            Process.Start("explorer.exe", path);

        }

        private void LoadCreateBackup(object sender, RoutedEventArgs e)
        {
            Main.Content = CreateBackup.GetPage();
        }
        private void LoadSettingsPage(object sender, RoutedEventArgs e)
        {
            Main.Content = Settings.GetPage(this.settings);
        }
        
        private void SetEnglish(object sender, RoutedEventArgs e)
        {
            this.settings.Language = "2";
            this.UpdateTranslation();
            FrenchMenu.IsChecked = false;
        } 
        
        private void SetFrench(object sender, RoutedEventArgs e)
        {
            this.settings.Language = "1";
            this.UpdateTranslation();
            EnglishMenu.IsChecked = false;

        }

        private void LoadEditBackup(object sender, RoutedEventArgs e)
        {
            Translator translator = Translator.GetTranslator();

            string title, content;
            MessageBoxButton buttons;
            MessageBoxResult result;

            if (this.IsHomePage())
            {
                HomePage page = (HomePage)Main.Content;

                if (page.SelectedBackup == null)
                {
                    title = translator.Translate(Menu.DeleteBackup);
                    content = translator.TranslateError(Error.NoSaveSelected);
                    buttons = MessageBoxButton.OK;

                result = System.Windows.MessageBox.Show(content, title, buttons, MessageBoxImage.Error);
                }
                else
                {
                    Backup backup = page.SelectedBackup;
                    Main.Content = EditBackup.GetPage(backup);
                }
                

               
            }
            else
            {
                title = translator.Translate(Menu.DeleteBackup);
                content = translator.TranslateError(Error.NoSaveSelected);
                buttons = MessageBoxButton.OK;
                result = System.Windows.MessageBox.Show(content, title, buttons, MessageBoxImage.Error);

                this.LoadHomePage();
            }

           
        }

        private void HomeButton_click(object sender, RoutedEventArgs e)
        {
            this.LoadHomePage();
        }



        private void LoadHomePage()
        {
            Main.Content = HomePage.GetPage();
        }

        List<Backup> list = HomePage.GetPage().ListBackup;
        private static EventWaitHandle waitHandle = new ManualResetEvent(initialState: true);

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (Backup backup in list)
            {
                backup.PauseBackup();
            }
            HomePage.GetPage().ExecSaveAppendNewLine(Translator.GetTranslator().TranslateLogPlayPauseStop(LogPlayPauseStop.CopyPaused));
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (Backup backup in list)
            {
                backup.ResumeBackup();
            }
            HomePage.GetPage().ExecSaveAppendNewLine(Translator.GetTranslator().TranslateLogPlayPauseStop(LogPlayPauseStop.CopyResumed));
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (Backup backup in list)
            {
                backup.Active = false;
            }
            HomePage.GetPage().ExecSaveAppendNewLine(Translator.GetTranslator().TranslateLogPlayPauseStop(LogPlayPauseStop.CopyStopped));
        }

        private static int ThreadReadyCount = 0;
        public static void ContinueExecution()
        {
            ThreadReadyCount++;
            if (MainThread != null && MainThread.ThreadState == System.Threading.ThreadState.WaitSleepJoin)
                MainThread.Interrupt();
        }

        private void WaitForThreads(List<Thread> List)
        {
            // Wait for all the threads to do a task
            while (ThreadReadyCount != List.Count)
            {
                try { Thread.Sleep(Timeout.Infinite); }
                catch (ThreadInterruptedException) { }
            }
            ThreadReadyCount = 0;

            // Interrupt each thread from their sleep to do the next task
            foreach (Thread thread in List)
            {
                if (thread.ThreadState == System.Threading.ThreadState.WaitSleepJoin)
                    thread.Interrupt();
            }
        }

        private static Thread MainThread;
        private void ExecuteAll(object sender, RoutedEventArgs e)
        {
            MainThread = new Thread(new ThreadStart(delegate ()
            {
                List<Backup> list = HomePage.GetPage().ListBackup;
                Translator translator = Translator.GetTranslator();
                string error = translator.TranslateError(Error.JobProgrammRunning);

                List<Thread> ThreadList = new List<Thread>();
                foreach (Backup backup in list)
                {
                    HomePage.GetPage().SelectedBackup = backup;
                    bool test = false;
                    Thread thread = new Thread(new ThreadStart(delegate ()
                    {
                        test = backup.Execute(true);
                        if (!test)
                        {
                            HomePage.GetPage().ExecSaveAppendNewLine(error);
                        }
                    }));
                    ThreadList.Add(thread);
                    backup.BackupThread = thread;
                    thread.Start();
                }
                // Wait for threads to count the files then resume
                WaitForThreads(ThreadList);
                // Wait for threads to copy the priority files then resume
                WaitForThreads(ThreadList);
            }));
            MainThread.Start();
        }

        private void DeleteBackupDialog(object sender, RoutedEventArgs e)
        {

            Translator translator = Translator.GetTranslator();
            string title, content;
            MessageBoxButton buttons;
            MessageBoxResult result;
            if (this.IsHomePage())
            {
                HomePage page = (HomePage)Main.Content;

                if(page.SelectedBackup == null)
                {
                     title = translator.Translate(Menu.DeleteBackup);
                    content = translator.TranslateError(Error.NoSaveSelected);
                    buttons = MessageBoxButton.OK;

                }
                else
                {
                    title = translator.Translate(Menu.DeleteBackup);
                    content = translator.Translate(Menu.ConfirmDelete);
                    buttons = MessageBoxButton.YesNo;

                }

                result = System.Windows.MessageBox.Show(content, title, buttons,MessageBoxImage.Error);
                if(result == MessageBoxResult.Yes)
                {
                    this.DeleteBackup(page.SelectedBackup);
                    page.Refresh();
                }
            }
            else
            {
                 title = translator.Translate(Menu.DeleteBackup);
                 content = translator.TranslateError(Error.NoSaveSelected);
                 buttons = MessageBoxButton.OK;
                 result = System.Windows.MessageBox.Show(content, title, buttons, MessageBoxImage.Error);

                this.LoadHomePage();
            }
        } 

        private void DeleteBackup(Backup backup)
        {
            List<Backup> list = HomePage.GetPage().ListBackup;
            list.Remove(backup);
            HomePage.SaveBackup(list);

        }

        private bool IsHomePage()
        {
            return Main.Content == HomePage.GetPage();
        }
        
        private void ExecuteOneButton(object sender, RoutedEventArgs e)
        {
            new Thread(new ThreadStart(delegate ()
            { 
                Translator translator = Translator.GetTranslator();
                bool result = HomePage.GetPage().SelectedBackup.Execute();
                if (!result)
                {
                    HomePage.GetPage().ExecSaveAppendNewLine(translator.TranslateError(Error.JobProgrammRunning));
                }
            })).Start();
        }
    }
}
