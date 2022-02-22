using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Newtonsoft.Json;
using System.Reflection;
using System.IO;

namespace EasySafe.Model
{
    public class Model
    {
        internal List<Backup> ListBackup { get; set; }
        string path;

        public Model()
        {
            LoadBackup();
        }

        internal bool CreateBackup(Backup backup)
        {
            this.ListBackup.Add(backup);
            this.Save();
            return true;
        }

        internal void Save()
        {
            var jsonData = JsonConvert.SerializeObject(this.ListBackup, Formatting.Indented);
            Console.WriteLine(this.path);
            File.WriteAllText(this.path, jsonData);
        }

        internal bool RemoveBackup(int BackupChoice)
        {
            if (this.ListBackup.Count() != 0)
            {
                int index = BackupChoice - 1;
                LogState.RemoveStateLog(ListBackup[index].Name);
                this.ListBackup.RemoveAt(index);
                this.Save();
                return true;
            }
            return false;
        }

        internal bool UpdateBackup(int BackupChoice, Backup UpdatedBackup)
        {
            if (this.ListBackup.Count() != 0)
            {
                int index = BackupChoice - 1;
                this.ListBackup[index] = UpdatedBackup;
                this.Save();
                return true;
            }
            return false;
        }

        internal void LoadBackup()
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

            this.path = Path.GetDirectoryName(tempPath) + @"\Backup.json";

            if (!File.Exists(this.path))
            {
                File.Create(this.path);
            }

            if (new FileInfo(this.path).Length != 0)
            {
                var jsonData = File.ReadAllText(this.path);
                this.ListBackup = JsonConvert.DeserializeObject<List<Backup>>(jsonData);
            }
            else
            {
                this.ListBackup = new List<Backup>();
            }
        }
    }
}
