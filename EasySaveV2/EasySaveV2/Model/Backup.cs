using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using EasySaveV2;
using EasySaveV2.View;

namespace EasySafe.Model
{
    public class Backup
    {
        public Backup()
        {
        }

        public string Name { get; set; }

        public string Source { get; set; }

        public string Target { get; set; }

        public BackupType BackupType { get; set; }

        public bool Active { get; set; }

        private Process ActiveJobProcess { get; set; }

        public static bool CopyingBigFile { get; set; }
        public bool WaitingToCopyBigFile { get; set; }

        private List<FileInfo> filesToCopy;
        private List<FileInfo> priorityFilesToCopy;

        public Thread BackupThread { get; set; }

        public enum LogMessage
        {
            CopyInProgress,
            EncryptionInProgress,
            CopyFinished,
            CopyPaused,
            CopyStoped
        }

        // Following attributes are only used for the state log and to show to the user.
        private int _totalFilesToCopy;
        public int TotalFilesToCopy
        {
            get { return this._totalFilesToCopy; }
            set
            {
                this._totalFilesToCopy = value;
                HomePage.GetPage().TotalFilesToCopyText(value.ToString());
            }
        }

        private long _totalFilesSize;
        public long TotalFilesSize
        {
            get { return this._totalFilesSize; }
            set
            {
                this._totalFilesSize = value;
                HomePage.GetPage().TotalFilesSizeText(value.ToString());
            }
        }

        private int _nbFilesLeftToDo;
        public int NbFilesLeftToDo
        {
            get { return this._nbFilesLeftToDo; }
            set
            {
                this._nbFilesLeftToDo = value;
                HomePage.GetPage().NbFilesLeftToDoText(value.ToString());
            }
        }

        private long _filesLeftToDoSize;
        public long FilesLeftToDoSize
        {
            get { return this._filesLeftToDoSize; }
            set
            {
                this._filesLeftToDoSize = value;
                HomePage.GetPage().FilesLeftToDoSizeText(value.ToString());
            }
        }

        /// <summary>
        /// Execute the backup of the source directory to the target.
        /// </summary>
        public bool Execute(bool multipleBackups = false)
        {
            if (VerifyProcess())
            {
                // TODO: Error Job program is running
                return false;
            }

            this.Active = true;

            DirectoryInfo sourceDir = new DirectoryInfo(Source);
            if (!sourceDir.Exists)
            {
                // TODO: Error Source directory does not exist or could not be found: (the path)
                return false;
            }

            filesToCopy = new List<FileInfo>();
            priorityFilesToCopy = new List<FileInfo>();

            this.TotalFilesToCopy = 0;
            this.TotalFilesSize = 0;

            // Get the list of the files to copy.
            CountFilesToCopy(filesToCopy, priorityFilesToCopy, sourceDir, BackupType, Target);

            // Ready to copy priority files
            if(multipleBackups)
                ThreadReady();

            //Copy the priority files first
            bool result = DirectoryCopy(priorityFilesToCopy, Target, true);

            if (!result)
            {
                // TODO: Error Job program is running, the complete save has ended
                return false;
            }

            // Wait for the interrupt from main thread to continue when all priority files have been copied
            if(multipleBackups)
                ThreadReady();

            // Copy all the files from the list.
            result = DirectoryCopy(filesToCopy, Target);

            if (!result)
            {
                // TODO: Error Job program is running, the complete save has ended
                return false;
            }

            this.Active = false;

            new LogState(Name, "", "", "END", 0, 0, 0, 0);

            return true;
        }

        private void ThreadReady()
        {
            // Say to the MainThread that it is ready for the next task
            MainWindow.ContinueExecution();

            // Wait for the MainThread to interrupt its sleep
            try { Thread.Sleep(Timeout.Infinite); }
            catch (ThreadInterruptedException) { }
        }

        private static EventWaitHandle waitHandle = new ManualResetEvent(initialState: true);

        public void PauseBackup()
        {
            waitHandle.Reset();
        }

        public void ResumeBackup()
        {
            waitHandle.Set();
        }

        private static Mutex mutex = new Mutex();
        // Used to copy the files from the parameter list to the target.
        private bool DirectoryCopy(List<FileInfo> filesToCopy, string targetDirName, bool priority = false)
        {
            string sourceDirName = new DirectoryInfo(this.Source).Name;

            targetDirName = Path.Combine(targetDirName, sourceDirName);

            // Completly delete the dir that holds the old backup in the case of a complete backup.
            DirectoryInfo targetDir = new DirectoryInfo(targetDirName);
            if (this.BackupType == BackupType.Complet)
            {
                if (targetDir.Exists && priority)
                {
                    targetDir.Delete(true);
                }
            }

            Translator translator = Translator.GetTranslator();

            foreach (FileInfo file in filesToCopy)
            {
                while (this.VerifyProcess())
                {
                    waitHandle.Reset();
                    HomePage.GetPage().ExecSaveAppendNewLine(translator.TranslateError(Error.JobProgrammRunning));
                    ActiveJobProcess.WaitForExit();
                    waitHandle.Set();
                }

                waitHandle.WaitOne();
                
                if (!Active)
                {
                    break;
                }

                if (file.Length > Convert.ToInt64(AppSettings.GetAppSettings().MaxSizeFileToCopy))
                {
                    mutex.WaitOne();
                    if (!CopyingBigFile)
                    {
                        CopyingBigFile = true;
                    }
                    else
                    {
                        WaitingToCopyBigFile = true;
                        try { Thread.Sleep(Timeout.Infinite); }
                        catch (ThreadInterruptedException) { }
                        WaitingToCopyBigFile = false;
                        CopyingBigFile = true;
                    }
                    mutex.ReleaseMutex();
                }

                string path;
                string internalPath = file.DirectoryName.Split(sourceDirName)[1];

                // Gets the path that had the file in the source dir to keep the same hierarchy.
                if (internalPath != "")
                    path = targetDirName + internalPath;
                else
                    path = targetDirName;

                Directory.CreateDirectory(path);
                path = Path.Combine(path, file.Name);

                string source = file.FullName;

                HomePage.GetPage().ExecSaveAppendNewLine(translator.TranslateLogString(LogMessage.CopyInProgress) + file.FullName);

                Stopwatch stopWatch = Stopwatch.StartNew();
                                        
                if (AppSettings.GetAppSettings().ExtentionToEncrypt.Contains(file.Extension))
                {
                    HomePage.GetPage().ExecSaveAppendNewLine(translator.TranslateLogString(LogMessage.EncryptionInProgress) + file.FullName);
                    ProcessStartInfo pro = new ProcessStartInfo("CryptoSoft/CryptoSoft.exe");
                    pro.ArgumentList.Add(source);
                    pro.ArgumentList.Add(path);
                    pro.CreateNoWindow = true;
                    Process.Start(pro);
                }
                else
                    file.CopyTo(path, true);

                this.NbFilesLeftToDo--;
                this.FilesLeftToDoSize -= file.Length;

                new Log(Name, source, path, file.Length, stopWatch.ElapsedMilliseconds);
                new LogState(Name, source, path, "ACTIVE", TotalFilesToCopy, TotalFilesSize, NbFilesLeftToDo, FilesLeftToDoSize);

                HomePage.GetPage().ExecSaveAppendNewLine(translator.TranslateLogString(LogMessage.CopyFinished) + file.FullName);

                if (CopyingBigFile && file.Length > Convert.ToInt64(AppSettings.GetAppSettings().MaxSizeFileToCopy))
                {
                    CopyingBigFile = false;
                    foreach (Backup backup in HomePage.GetPage().ListBackup)
                    {
                        if (backup.WaitingToCopyBigFile)
                        {
                            if(backup.BackupThread.ThreadState == System.Threading.ThreadState.WaitSleepJoin)
                            {
                                backup.BackupThread.Interrupt();
                            }

                            break;
                        }
                    }
                }
            }

            return true;
        }

        // Analyse the source directory to determine the files to backup (Complete or differential backup).
        private void CountFilesToCopy(List<FileInfo> filesToCopy, List<FileInfo> priorityFilesToCopy, DirectoryInfo sourceDir, BackupType backupType, string targetDirPath, bool firstDir = true)
        {
            FileInfo[] files = sourceDir.GetFiles();

            if (backupType == BackupType.Complet)
            {
                foreach (FileInfo file in files)
                {
                    this.TotalFilesToCopy += 1;
                    this.TotalFilesSize += file.Length;

                    if (AppSettings.GetAppSettings().ExtentionImportantFiles.Contains(file.Extension))
                        priorityFilesToCopy.Add(file);
                    else
                        filesToCopy.Add(file);
                }
            }

            DirectoryInfo targetDir = new DirectoryInfo(targetDirPath);
            if (backupType == BackupType.Differentiel)
            {
                targetDir = new DirectoryInfo(Path.Combine(targetDirPath, sourceDir.Name));
                if (!targetDir.Exists)
                {
                    CountFilesToCopy(filesToCopy, priorityFilesToCopy, sourceDir, BackupType.Complet, targetDirPath);
                    this.NbFilesLeftToDo = this.TotalFilesToCopy;
                    this.FilesLeftToDoSize = this.TotalFilesSize;

                    // Call this function again for each sub directory
                    DirectoryInfo[] subDirsDiff = sourceDir.GetDirectories();
                    foreach (DirectoryInfo subDir in subDirsDiff)
                    {
                        CountFilesToCopy(filesToCopy, priorityFilesToCopy, subDir, backupType, targetDir.FullName, false);
                    }
                    return;
                }

                FileInfo[] targetFiles = targetDir.GetFiles();

                foreach (FileInfo file in files)
                {
                    bool copy = false;
                    bool fileFound = false;
                    // if two file from the source dir and the complete backup have the same name and same last write time, it won't be copied.
                    // If a file in the source dir is not found, it will be copied.
                    foreach (FileInfo targetFile in targetFiles)
                    {
                        if (file.Name == targetFile.Name)
                        {
                            fileFound = true;
                            if (file.LastWriteTime.Ticks > targetFile.LastWriteTime.Ticks)
                            {
                                copy = true;
                            }
                            break;
                        }
                    }

                    if (!fileFound)
                    {
                        copy = true;
                    }

                    if (copy)
                    {
                        this.TotalFilesToCopy += 1;
                        this.TotalFilesSize += file.Length;

                        if (AppSettings.GetAppSettings().ExtentionImportantFiles.Contains(file.Extension))
                            priorityFilesToCopy.Add(file);
                        else
                            filesToCopy.Add(file);
                    }
                }
            }

            this.NbFilesLeftToDo = this.TotalFilesToCopy;
            this.FilesLeftToDoSize = this.TotalFilesSize;

            // Call this function again for each sub directory
            DirectoryInfo[] subDirs = sourceDir.GetDirectories();
            foreach (DirectoryInfo subDir in subDirs)
            {
                CountFilesToCopy(filesToCopy, priorityFilesToCopy, subDir, backupType, targetDir.FullName, false);
            }
        }


        internal string ToString()
        {
            return this.Name;
        }

        internal bool VerifyProcess()
        {
            foreach(var App in AppSettings.GetAppSettings().JobsApp)
            {
                Process[] process = Process.GetProcessesByName(App);
                if (process.Length > 0)
                {
                    ActiveJobProcess = process[0];
                    return true;
                }
            }
            return false;
        }

    }
}
