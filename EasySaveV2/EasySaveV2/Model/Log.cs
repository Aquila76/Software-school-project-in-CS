using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows;
using System.Xml;
using System.Xml.XPath;

namespace EasySafe.Model
{
    public class Log
    {
        private class DailyLogData
        {
            public string Name;
            public string FileSource;
            public string FileTarget;
            public string DestinationPath;
            public long FileSize;
            public double FileTransferTime;
            public string Time;
        }

        private static Mutex MutexLog = new Mutex();
        public Log(string name, string source, string target, long size, double duration)
        {
            MutexLog.WaitOne();
            Path = $"./logs/Logs_{DateTime.Now:dd-MM-yyyy}";
            Name = name;
            Source = source;
            Target = target;
            Size = size;
            Duration = duration;
            DateTime = DateTime.Now;

            try
            {
                AppSettings appSettings = AppSettings.GetAppSettings();

                // Create logs folder if doesn't exist
                Directory.CreateDirectory("./logs");

                // Do logs in JSON or XML format depending on the application settings
                switch (appSettings.LogsFilesFormat)
                {
                    case "1":
                        {
                            // Create a logs file if doesn't exist
                            if (!File.Exists($"{Path}.json"))
                            {
                                StreamWriter file = File.AppendText($"{Path}.json");
                                file.Close();
                            }

                            // Get JSON data from logs file
                            string jsonData = File.ReadAllText($"{Path}.json");

                            // Convert JSON data into a DailyLogData list. If the JSON file is empty, the DailyLogData list is created
                            List<DailyLogData> dataList = JsonConvert.DeserializeObject<List<DailyLogData>>(jsonData) ?? new List<DailyLogData>();

                            // Add a new data list to dataList
                            dataList.Add(new DailyLogData()
                            {
                                Name = Name,
                                FileSource = Source,
                                FileTarget = Target,
                                DestinationPath = Target.Substring(0, Target.LastIndexOf("\\")),
                                FileSize = Size,
                                FileTransferTime = Duration,
                                Time = DateTime.ToString("G")
                            });

                            // Convert dataList into a JSON string with identation option
                            jsonData = JsonConvert.SerializeObject(dataList, Newtonsoft.Json.Formatting.Indented);

                            // Write JSON data into the JSON file
                            File.WriteAllText($"{Path}.json", jsonData);
                        }
                        break;
                    case "2":
                        {
                            // Create the logs file if doesn't exist or edit it
                            if (!File.Exists($"{Path}.xml"))
                            {
                                // Set the indentation setting to true
                                XmlWriterSettings xmlSettings = new XmlWriterSettings
                                {
                                    Indent = true
                                };

                                // Create the logs file and fill it with provided data
                                using (XmlWriter xml = XmlWriter.Create($"{Path}.xml", xmlSettings))
                                {
                                    xml.WriteStartElement($"logs_{DateTime.Now:dd-MM-yyyy}");

                                    xml.WriteStartElement(Name);
                                    xml.WriteElementString("FileSource", Source);
                                    xml.WriteElementString("FileTarget", Target);
                                    xml.WriteElementString("DestinationPath", Target.Substring(0, Target.LastIndexOf("\\")));
                                    xml.WriteElementString("FileSize", Size.ToString());
                                    xml.WriteElementString("FileTransferTime", Duration.ToString());
                                    xml.WriteElementString("Time", DateTime.ToString("G"));
                                    xml.WriteEndElement();

                                    xml.WriteEndElement();
                                }
                            }
                            else
                            {
                                // Load the logs file
                                XmlDocument xmlDocument = new XmlDocument();
                                xmlDocument.Load($"{Path}.xml");

                                // Move to the right place in the document
                                XPathNavigator navigator = xmlDocument.CreateNavigator();
                                navigator.MoveToChild($"logs_{DateTime.Now:dd-MM-yyyy}", "");

                                // Add the log to the file with provided data
                                using (XmlWriter xml = navigator.AppendChild())
                                {
                                    xml.WriteStartElement(Name);
                                    xml.WriteElementString("FileSource", Source);
                                    xml.WriteElementString("FileTarget", Target);
                                    xml.WriteElementString("DestinationPath", Target.Substring(0, Target.LastIndexOf("\\")));
                                    xml.WriteElementString("FileSize", Size.ToString());
                                    xml.WriteElementString("FileTransferTime", Duration.ToString());
                                    xml.WriteElementString("Time", DateTime.ToString("G"));
                                    xml.WriteEndElement();
                                }

                                xmlDocument.Save($"{Path}.xml");
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Erreur", MessageBoxButton.OK);
            }

            MutexLog.ReleaseMutex();
        }

        private string Path { get; set; }

        private string Name { get; set; }

        private string Source { get; set; }

        private string Target { get; set; }

        private long Size { get; set; }

        private double Duration { get; set; }

        private DateTime DateTime { get; set; }
    }

    public class LogState
    {
        private class StateLogData
        {
            public string Name;
            public string FileSource;
            public string FileTarget;
            public string State;
            public int TotalFiles;
            public long TotalSize;
            public int FilesLeft;
            public long SizeLeft;
            public int Progression;
            public string Time;
        }

        private static Mutex MutexLogState = new Mutex();
        public LogState(string name, string source, string target, string state, int totalFiles, long totalSize, int filesLeft, long sizeLeft)
        {
            MutexLogState.WaitOne();
            Path = "./logs_state/Logs_State";
            Name = name;
            Source = source;
            Target = target;
            State = state;
            TotalFiles = totalFiles;
            TotalSize = totalSize;
            FilesLeft = filesLeft;
            SizeLeft = sizeLeft;
            if (totalFiles != 0)
                Progression = 100 - 100 * filesLeft / totalFiles;
            else
                Progression = 0;
            DateTime = DateTime.Now;

            try
            {
                AppSettings appSettings = AppSettings.GetAppSettings();


                // Create logs_state folder if doesn't exist
                Directory.CreateDirectory("./logs_state");

                // Do state logs in JSON or XML format depending on the application settings
                switch (appSettings.LogsFilesFormat)
                {
                    case "1":
                        {
                            // Create a state logs file if doesn't exist
                            if (!File.Exists($"{Path}.json"))
                            {
                                StreamWriter file = File.AppendText($"{Path}.json");
                                file.Close();
                            }

                            // Get JSON data from state logs file
                            string jsonData = File.ReadAllText($"{Path}.json");

                            // Convert JSON data into a StateLogData list. If the JSON file is empty, the StateLogData list is created
                            List<StateLogData> dataList = JsonConvert.DeserializeObject<List<StateLogData>>(jsonData) ?? new List<StateLogData>();

                            // Update the state log if it exists in dataList
                            List<StateLogData> modifiedDataList = new List<StateLogData>();
                            bool itemInList = false;
                            foreach (StateLogData item in dataList)
                            {
                                if (item.Name == name)
                                {
                                    item.FileSource = Source;
                                    item.FileTarget = Target;
                                    item.State = State;
                                    item.TotalFiles = TotalFiles;
                                    item.TotalSize = TotalSize;
                                    item.FilesLeft = FilesLeft;
                                    item.SizeLeft = SizeLeft;
                                    item.Progression = Progression;
                                    item.Time = DateTime.ToString("G");
                                    itemInList = true;
                                }
                                modifiedDataList.Add(item);
                            }

                            // Add a new data list to modifiedDataList if the state log is new
                            if (!itemInList)
                            {
                                modifiedDataList.Add(new StateLogData()
                                {
                                    Name = Name,
                                    FileSource = Source,
                                    FileTarget = Target,
                                    State = State,
                                    TotalFiles = TotalFiles,
                                    TotalSize = TotalSize,
                                    FilesLeft = FilesLeft,
                                    SizeLeft = SizeLeft,
                                    Progression = Progression,
                                    Time = DateTime.ToString("G")
                                });
                            }

                            // Convert modifiedDataList into a JSON string with identation option
                            jsonData = JsonConvert.SerializeObject(modifiedDataList, Newtonsoft.Json.Formatting.Indented);

                            // Write JSON data into the JSON file
                            File.WriteAllText($"{Path}.json", jsonData);
                        }
                        break;
                    case "2":
                        {
                            // Create the state logs file if doesn't exist or edit it
                            if (!File.Exists($"{Path}.xml"))
                            {
                                // Set the indentation setting to true
                                XmlWriterSettings xmlSettings = new XmlWriterSettings
                                {
                                    Indent = true
                                };

                                // Create the state logs file and fill it with provided data
                                using (XmlWriter xml = XmlWriter.Create($"{Path}.xml", xmlSettings))
                                {
                                    xml.WriteStartElement("logs_state");

                                    xml.WriteStartElement(Name);
                                    xml.WriteElementString("FileSource", Source);
                                    xml.WriteElementString("FileTarget", Target);
                                    xml.WriteElementString("State", State);
                                    xml.WriteElementString("TotalFiles", TotalFiles.ToString());
                                    xml.WriteElementString("TotalSize", TotalSize.ToString());
                                    xml.WriteElementString("FilesLeft", FilesLeft.ToString());
                                    xml.WriteElementString("SizeLeft", SizeLeft.ToString());
                                    xml.WriteElementString("Progression", Progression.ToString());
                                    xml.WriteElementString("Time", DateTime.ToString("G"));
                                    xml.WriteEndElement();

                                    xml.WriteEndElement();
                                }
                            }
                            else
                            {
                                // Load the state logs file
                                XmlDocument xmlDocument = new XmlDocument();
                                xmlDocument.Load($"{Path}.xml");

                                // Move to the right place in the document
                                XPathNavigator navigator = xmlDocument.CreateNavigator();
                                navigator.MoveToChild("logs_state", "");

                                // Edit the state log if does exist, else add a new one, with provided data
                                if (navigator.MoveToChild(name, ""))
                                {
                                    navigator.SelectSingleNode("FileSource").InnerXml = Source;
                                    navigator.SelectSingleNode("FileTarget").InnerXml = Target;
                                    navigator.SelectSingleNode("State").InnerXml = State;
                                    navigator.SelectSingleNode("TotalFiles").InnerXml = TotalFiles.ToString();
                                    navigator.SelectSingleNode("TotalSize").InnerXml = TotalSize.ToString();
                                    navigator.SelectSingleNode("FilesLeft").InnerXml = FilesLeft.ToString();
                                    navigator.SelectSingleNode("SizeLeft").InnerXml = SizeLeft.ToString();
                                    navigator.SelectSingleNode("Progression").InnerXml = Progression.ToString();
                                    navigator.SelectSingleNode("Time").InnerXml = DateTime.ToString("G");
                                }
                                else
                                {
                                    using (XmlWriter xml = navigator.AppendChild())
                                    {
                                        xml.WriteStartElement(Name);
                                        xml.WriteElementString("FileSource", Source);
                                        xml.WriteElementString("FileTarget", Target);
                                        xml.WriteElementString("State", State);
                                        xml.WriteElementString("TotalFiles", TotalFiles.ToString());
                                        xml.WriteElementString("TotalSize", TotalSize.ToString());
                                        xml.WriteElementString("FilesLeft", FilesLeft.ToString());
                                        xml.WriteElementString("SizeLeft", SizeLeft.ToString());
                                        xml.WriteElementString("Progression", Progression.ToString());
                                        xml.WriteElementString("Time", DateTime.ToString("G"));
                                        xml.WriteEndElement();
                                    }
                                }

                                xmlDocument.Save($"{Path}.xml");
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Erreur", MessageBoxButton.OK);
            }

            MutexLogState.ReleaseMutex();
        }

        public static void RemoveStateLog(string name)
        {
            try
            {
                string path = "./logs_state/Logs_State";
                AppSettings appSettings = AppSettings.GetAppSettings();


                // Remove state logs in the JSON or XML file, depending on the application settings
                switch (appSettings.LogsFilesFormat)
                {
                    case "1":
                        {
                            // Get JSON data from logs file
                            string jsonData = File.ReadAllText($"{path}.json");

                            // Convert JSON data into a StateLogData list
                            List<StateLogData> dataList = JsonConvert.DeserializeObject<List<StateLogData>>(jsonData);

                            // Remove the log corresponding to the name parameter
                            foreach (StateLogData item in dataList)
                            {
                                if (item.Name == name)
                                {
                                    dataList.Remove(item);
                                    break;
                                }
                            }

                            // Convert dataList into a JSON string with identation option
                            jsonData = JsonConvert.SerializeObject(dataList, Newtonsoft.Json.Formatting.Indented);

                            // Write JSON data into the JSON file
                            File.WriteAllText($"{path}.json", jsonData);
                        }
                        break;
                    case "2":
                        {
                            // Load the state logs file
                            XmlDocument xmlDocument = new XmlDocument();
                            xmlDocument.Load($"{path}.xml");

                            // Remove the corresponding state logs
                            XPathNavigator navigator = xmlDocument.CreateNavigator();
                            navigator.MoveToChild("logs_state", "");
                            navigator.MoveToChild(name, "");
                            navigator.DeleteSelf();
                            xmlDocument.Save($"{path}.xml");
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Erreur", MessageBoxButton.OK);
            }
        }

        public string Path { get; set; }

        private string Name { get; set; }

        private string Source { get; set; }

        private string Target { get; set; }

        private string State { get; set; }

        private int TotalFiles { get; set; }

        private long TotalSize { get; set; }

        private int FilesLeft { get; set; }

        public long SizeLeft { get; set; }

        private int Progression { get; set; }

        private DateTime DateTime { get; set; }
    }
}
