using EasySafe.Model;
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
using Menu = EasySafe.Model.Menu;

namespace EasySaveV2.View
{
    /// <summary>
    /// Logique d'interaction pour Settings.xaml
    /// </summary>
    public partial class Settings : Page
    {

        private static Settings _instance;
        private AppSettings settings;

        private Settings(AppSettings settings)
        {
            InitializeComponent();
            this.MaxSizeLabel_Invalid.Foreground = Brushes.Red;
            this.MaxSizeLabel_Invalid.Visibility = Visibility.Hidden;

            this.Translate();
            _instance = this;
            this.settings = settings;
            this.MaxSize.Text = settings.MaxSizeFileToCopy;


            List<LogFormat> list = Enum.GetValues(typeof(LogFormat)).Cast<LogFormat>().ToList();
            foreach(LogFormat item in list)
            {
                this.LogFormat.Items.Add(item);
                string tmp = ((int)item).ToString();
                if(tmp == this.settings.LogsFilesFormat)
                {
                    this.LogFormat.SelectedItem = item;
                }
            }

            foreach (string item in this.settings.ExtentionToEncrypt)
            {
                this.ListExtentionToCrypt.Items.Add(item);
            }
            foreach (string item in this.settings.ExtentionImportantFiles)
            {
                this.ListExtensionImportantFiles.Items.Add(item);
            }
            foreach (string item in this.settings.JobsApp)
            {
                this.ListJobsApp.Items.Add(item);
            }
        }

        public static Settings GetPage(AppSettings settings)
        {
            if(_instance == null)
            {
                return new Settings(settings);
            }
            return _instance;
        }

        internal void Translate()
        {
            Translator translator = Translator.GetTranslator();
            this.MaxSizeLabel.Content = translator.Translate(Menu.MaxSizeFile);
            this.MaxSizeLabel_Invalid.Content = translator.TranslateError(Error.InputError);
            this.LogFormatLabel.Content = translator.Translate(Menu.LogFormat);
            this.ExtentionImpFilesLabel.Content = translator.Translate(Menu.ExtentionImportantFiles);
            this.ExtentionNotCopyLabel.Content = translator.Translate(Menu.ExtentionToEncrypt);
            this.JobsAppLabel.Content = translator.Translate(Menu.JobsApp);

        }

        public void MaxSizeValid(object sender, RoutedEventArgs e)
        {
            int tmp;
            if (!Int32.TryParse(this.MaxSize.Text,out tmp))
            {
               this.MaxSizeLabel_Invalid.Visibility = Visibility.Visible;
            }
            else if (this.MaxSizeLabel_Invalid != null)
            {
                this.MaxSizeLabel_Invalid.Visibility = Visibility.Hidden;
            }
        }

        public void RemoveExtToCrypt(object sender, RoutedEventArgs e)
        {
            string value = this.ListExtentionToCrypt.SelectedItem as string;
            if (value != null)
            {
                this.ListExtentionToCrypt.Items.Remove(value);
                this.settings.ExtentionToEncrypt.Remove(value.Trim());
                this.settings.SaveLists();

            }
        }
        public void AddExtToCrypt(object sender, RoutedEventArgs e)
        {
            string value = this.ExtentionTextBox.Text;
            if (!string.IsNullOrEmpty(value))
            {
                this.ListExtentionToCrypt.Items.Add(value);
                this.settings.ExtentionToEncrypt.Add(value.Trim());
                this.settings.SaveLists();

            }
        }
        public void RemoveExtImpFiles(object sender, RoutedEventArgs e)
        {
            string value = this.ListExtensionImportantFiles.SelectedItem as string;
            if (value != null)
            {
                this.ListExtensionImportantFiles.Items.Remove(value);
                this.settings.ExtentionImportantFiles.Remove(value.Trim());
                this.settings.SaveLists();


            }
        }
        public void AddExtImpFiles(object sender, RoutedEventArgs e)
        {
            string value = this.ExtentionTextBox.Text;
            if (!string.IsNullOrEmpty(value))
            {
                this.ListExtensionImportantFiles.Items.Add(value);
                this.settings.ExtentionImportantFiles.Add(value.Trim());
                this.settings.SaveLists();

            }
        }
        public void RemoveJobApp(object sender, RoutedEventArgs e)
        {
            string value = this.ListJobsApp.SelectedItem as string;
            if (value != null)
            {
                this.ListJobsApp.Items.Remove(value);
                this.settings.JobsApp.Remove(value.Trim());
                this.settings.SaveLists();

            }
        }
        public void AddJobApp(object sender, RoutedEventArgs e)
        {
            string value = this.ExtentionTextBox.Text;
            if (!string.IsNullOrEmpty(value))
            {
                this.ListJobsApp.Items.Add(value);
                this.settings.JobsApp.Add(value.Trim());
                this.settings.SaveLists();
            }
        }
        
        public void Cancel(object sender, RoutedEventArgs e)
        {
            this.NavigationService.GoBack();
        }

        public void SaveSettings(object sender, RoutedEventArgs e)
        {
            if (!this.MaxSizeLabel_Invalid.IsVisible)
            {
                this.settings.MaxSizeFileToCopy = this.MaxSize.Text;
            }
            int index = this.LogFormat.SelectedIndex + 1;
            this.settings.LogsFilesFormat = index.ToString();

            this.NavigationService.GoBack();
        }

    }
}
