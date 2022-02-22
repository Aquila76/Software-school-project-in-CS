using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using EasySaveV2;
using System.Windows;
using System.Windows.Threading;
using EasySaveV2.View;

namespace EasySafe.Model
{
    public class AppSettings
    {
        private class AppSettingsData
        {
            public string Language;
            
            public string LogsFilesFormat;

            public string MaxSizeFileToCopy;

            public List<string> ExtentionToEncrypt;
            public List<string> ExtentionImportantFiles;
            public List<string> JobsApp;
        }

        Thread T;
        private static AppSettings _instance = null;

        private AppSettings()
        {
            // Get data from the settings file
            List<AppSettingsData> dataList = GetFileData();
            Language = dataList[0].Language;
            this.UpdateTranslator();
            LogsFilesFormat = dataList[0].LogsFilesFormat;
            MaxSizeFileToCopy = dataList[0].MaxSizeFileToCopy;
            ExtentionImportantFiles = dataList[0].ExtentionImportantFiles;
            JobsApp = dataList[0].JobsApp;
            ExtentionToEncrypt = dataList[0].ExtentionToEncrypt;

            _instance = this;

            this.T = new Thread(new ThreadStart(UpdateTranslator));
            if (!App.TestIfExist())
            {
                this.T = new Thread(new ThreadStart(this.RunNetwork));
                this.T.Start();
            }
            
            
        }

        public static AppSettings GetAppSettings()
        {
            if(_instance == null)
            {
                return new AppSettings();
            }
            return _instance;
        }

        private const string settingsPath = "./AppSettings.json";

        private void UpdateTranslator()
        {
           
                    Translator translator = Translator.GetTranslator();
                    switch (this.Language)
                    {
                        case "1":
                            translator.Language = EasySafe.Model.Language.French;
                            break;
                        case "2":
                            translator.Language = EasySafe.Model.Language.English;
                            break;
                    };
                
            
        }

        private static List<AppSettingsData> GetFileData()
        {
            // Get JSON data from settings file
            string jsonData = File.ReadAllText(settingsPath);

            // Convert JSON data into a SettingsData list. If the JSON file is empty, the SettingsData list is created
            List<AppSettingsData> dataList = JsonConvert.DeserializeObject<List<AppSettingsData>>(jsonData);

            return dataList;
        }

        private static void SetFileData(List<AppSettingsData> dataList)
        {
            // Convert dataList into a JSON string with identation option
            string jsonData = JsonConvert.SerializeObject(dataList, Formatting.Indented);

            // Write JSON data into the JSON file
            File.WriteAllText(settingsPath, jsonData);
        }

        private string language;

        public string Language
        {
            get { return language; }
            set
            {
                language = value;
                this.UpdateTranslator();
                try
                {
                    // Get data from the settings file
                    List<AppSettingsData> dataList = GetFileData();

                    // Update the language setting
                    dataList[0].Language = language;

                    // Set data in the settings file
                    SetFileData(dataList);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString(), "Erreur", MessageBoxButton.OK);
                }
            }
        }

        internal void SaveLists()
        {
            this.ExtentionToEncrypt = this.ExtentionToEncrypt;
            this.ExtentionImportantFiles = this.ExtentionImportantFiles;
            this.JobsApp = this.JobsApp;
        }

        private string logsFilesFormat;

        public string LogsFilesFormat
        {
            get { return logsFilesFormat; }
            set
            {
                logsFilesFormat = value;

                try
                {
                    // Get data from the settings file
                    List<AppSettingsData> dataList = GetFileData();

                    // Update the logs files format setting
                    dataList[0].LogsFilesFormat = logsFilesFormat;

                    // Set data in the settings file
                    SetFileData(dataList);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString(), "Erreur", MessageBoxButton.OK);
                }
            }
        }

        private string maxSizeFileToCopy;

        public string MaxSizeFileToCopy
        {
            get { return maxSizeFileToCopy; }
            set
            {
                maxSizeFileToCopy = value;

                try
                {
                    // Get data from the settings file
                    List<AppSettingsData> dataList = GetFileData();

                    // Update the settings files format setting
                    dataList[0].MaxSizeFileToCopy = maxSizeFileToCopy;

                    // Set data in the settings file
                    SetFileData(dataList);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString(), "Erreur", MessageBoxButton.OK);
                }
            }
        }

        private List<string> extentionToEncrypt;
        private List<string> extentionImportantFiles;
        private List<string> jobsApp;

        public List<string> ExtentionToEncrypt
        {
            get { return extentionToEncrypt; }
            set
            {
                extentionToEncrypt = value;

                try
                {
                    // Get data from the settings file
                    List<AppSettingsData> dataList = GetFileData();

                    // Update the logs files format setting
                    dataList[0].ExtentionToEncrypt = extentionToEncrypt;

                    // Set data in the settings file
                    SetFileData(dataList);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString(), "Erreur", MessageBoxButton.OK);
                }
            }
        }
        public List<string> ExtentionImportantFiles
        {
            get { return extentionImportantFiles; }
            set
            {
                extentionImportantFiles = value;

                try
                {
                    // Get data from the settings file
                    List<AppSettingsData> dataList = GetFileData();

                    // Update the logs files format setting
                    dataList[0].ExtentionImportantFiles = extentionImportantFiles;

                    // Set data in the settings file
                    SetFileData(dataList);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString(), "Erreur", MessageBoxButton.OK);
                }
            }
        }
        public List<string> JobsApp
        {
            get { return jobsApp; }
            set
            {
                jobsApp = value;

                try
                {
                    // Get data from the settings file
                    List<AppSettingsData> dataList = GetFileData();

                    // Update the logs files format setting
                    dataList[0].JobsApp = jobsApp;

                    // Set data in the settings file
                    SetFileData(dataList);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString(), "Erreur", MessageBoxButton.OK);
                }
            }
        }


        private void RunNetwork()
        {
            Socket socket = SeConnecter();
            Socket client = AccepterConnection(socket);
            this.Listen(client);
            
        }

        private static Socket SeConnecter()
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);

            // Create a TCP/IP socket.  
            Socket listener = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            listener.Bind(localEndPoint);
            listener.Listen(10);

            return listener;
        }

        private static Socket AccepterConnection(Socket socket)
        {
            try
            {
                while (true)
                {
                    // Program is suspended while waiting for an incoming connection.  
                    Socket handler = socket.Accept();
                    return handler;
                }
            }
            catch (Exception ex)
            {
            }
            return null;
        }

        private  void Listen(Socket client)
        {
            byte[] bytes = new Byte[1024];
            List<Backup> list = HomePage.GetPage().ListBackup;

            while (true)
            {

                try
                {
                    string data = null;
                    int bytesRec = client.Receive(bytes);
                    data += Encoding.UTF8.GetString(bytes, 0, bytesRec);



                    if (data != null)
                    {
                        try
                        {
                            string name = data.Split(':')[0].Trim();
                            string value = data.Split(':')[1].Trim();
                            if (name == "action")
                            {
                                switch (value)
                                {
                                    case "pause":
                                        HomePage.GetPage().Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    (ThreadStart)delegate {
                        foreach (Backup backup in list)
                        {
                            backup.PauseBackup();
                        }
                        HomePage.GetPage().ExecSaveAppendNewLine(Translator.GetTranslator().TranslateLogPlayPauseStop(LogPlayPauseStop.CopyResumed));

                    });
                                        break;
                                    case "stop":
                                        HomePage.GetPage().Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    (ThreadStart)delegate { 

                        foreach (Backup backup in list)
                        {
                            backup.Active = false;

                        }
                            HomePage.GetPage().ExecSaveAppendNewLine(Translator.GetTranslator().TranslateLogPlayPauseStop(LogPlayPauseStop.CopyStopped));

                    });
                                        

                                        break;
                                    case "play":
                                        foreach (Backup backup in list)
                                        {
                                            backup.ResumeBackup();
                                        }
                                        HomePage.GetPage().ExecSaveAppendNewLine(Translator.GetTranslator().TranslateLogPlayPauseStop(LogPlayPauseStop.CopyResumed));
                                        break;
                                }
                            }
                        }
                        catch (Exception ex)
                        {

                        }


                    }
                    }
                catch (Exception ex)
                {

                }
            }
        }

        private static void Send(Socket client)
        {
            byte[] bytes = new Byte[1024];
            int i = 0;

            try
            {
                string msg = "name : SAVENAME";
                byte[] bmsg = Encoding.UTF8.GetBytes(msg);
                client.Send(bmsg);
            }catch (Exception ex)
            {

            }

            while (true)
            {
                try
                {


                    i = (i + 1) % 100;
                        string msg ="value :"+ i.ToString();
                        byte[] bmsg = Encoding.UTF8.GetBytes(msg);
                        client.Send(bmsg);
                    Thread.Sleep(500);
                    
                }
                catch (Exception ex)
                {

                }
            }
        }

        private static void Deconnecter(Socket socket)
        {
            socket.Close();
        }


    }
}
