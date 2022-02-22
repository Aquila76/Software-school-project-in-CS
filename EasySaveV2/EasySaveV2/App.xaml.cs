using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows;
using Newtonsoft.Json;
using System.IO;
using EasySafe.Model;

namespace EasySaveV2
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            if (TestIfExist())
            {
                System.Windows.MessageBox.Show(messageError());
                Application.Current.Shutdown();
            }
            else
            {
                new MainWindow().Show();
            }
        }

        public static bool TestIfExist()
        {
            Process[] processes = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName);
            if (processes.Length == 1)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static string messageError()
        {
            Translator translator = Translator.GetTranslator();
            
            

            return translator.TranslateError(Error.InstanceExist);
        }

    }

}
