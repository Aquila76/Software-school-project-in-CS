using EasySafe.Model;
using static EasySafe.Model.Backup;

namespace EasySafe.Model
{
    public class Translator
    {
        internal Language Language { get; set; } = Language.French;

        private static Translator translator;

        private Translator()
        {
        }

        //Design Pattern : Singleton -> get the instance of Translator or instanciate it
        public static Translator GetTranslator()
        {
            if (Translator.translator == null)
            {
                Translator.translator = new Translator();
            }
            return Translator.translator;
        }

        //Translate the menu put in parameter depending on the app language
        internal string Translate(Menu menu)
        {
            switch (this.Language)
            {
                case Language.French:
                    switch (menu)
                    {
                        case Menu.Exit:
                            return "Quitter l'application";
                        case Menu.CreateBackup:
                            return "Créer une sauvegarde";
                        case Menu.EditBackup:
                            return "Modifier une sauvegarde";
                        case Menu.DeleteBackup:
                            return "Supprimer une sauvegarde";
                        case Menu.ChangeLanguage:
                            return "Changer la langue";
                        case Menu.Backup:
                            return "Sauvegarde(s)";
                        case Menu.ExecuteOne:
                            return "Exécuter une sauvegarde";
                        case Menu.ExecuteAll:
                            return "Exécuter toutes les sauvegardes";
                        case Menu.ConfirmDelete:
                            return "Voulez vous vraiment supprimer cette sauvegarde ?  ";
                        case Menu.BackupName:
                            return "Nom de la Sauvegarde";
                        case Menu.BackupType:
                            return "Type de la Sauvegarde";
                        case Menu.BackupSource:
                            return "Source de la Sauvegarde";
                        case Menu.BackupTarget:
                            return "Destination de la Sauvegarde";
                        case Menu.NbFilesToCopy:
                            return "Nombre de fichier à copier";
                        case Menu.BackupSize:
                            return "Taille total de la Sauvegarde (o)";
                        case Menu.NbFilesRemaining:
                            return "Nombre de fichier restants";
                        case Menu.BackupSizeRemaining:
                            return "Taille restante à copier (o)";
                        case Menu.Cancel:
                            return "Annuler";
                        case Menu.Create:
                            return "Créer";
                        case Menu.Edit:
                            return "Modifier";
                        case Menu.Browse:
                            return "Parcourir";
                        case Menu.OpenLog:
                            return "Ouvrir les logs";
                        case Menu.OpenLogState:
                            return "Ouvrir les logs d'états";
                        case Menu.MaxSizeFile:
                            return "Taille maximum de deux fichiers copiées simulatanément";
                        case Menu.LogFormat:
                            return "Format des logs";
                        case Menu.JobsApp:
                            return "Applications metier";
                        case Menu.ExtentionImportantFiles:
                            return "Extentions des fichiers prioritaires";
                        case Menu.ExtentionToEncrypt:
                            return "Extentions de fichier à chiffrer";
                    }
                    break;
                case Language.English:
                    switch (menu)
                    {
                        case Menu.Exit:
                            return "Exit the application";
                        case Menu.CreateBackup:
                            return "Create a backup";
                        case Menu.EditBackup:
                            return "Edit a backup";
                        case Menu.DeleteBackup:
                            return "Delete a backup";
                        case Menu.ChangeLanguage:
                            return "Change the language";
                        case Menu.Backup:
                            return "Backup(s)";
                        case Menu.ExecuteOne:
                            return "Execute one backup";
                        case Menu.ExecuteAll:
                            return "Execute all backups";
                        case Menu.ConfirmDelete:
                            return "Do you want to delete this backup ?  ";
                        case Menu.BackupName:
                            return "Backup Name";
                        case Menu.BackupType:
                            return "Backup Type";
                        case Menu.BackupSource:
                            return "Backup Source";
                        case Menu.BackupTarget:
                            return "Backup Target";
                        case Menu.NbFilesToCopy:
                            return "Number of remaining to copy";
                        case Menu.BackupSize:
                            return "Backup Size (o)";
                        case Menu.NbFilesRemaining:
                            return "Number of remaining to files";
                        case Menu.BackupSizeRemaining:
                            return "Backup size remaing until the end (o)";
                        case Menu.Cancel:
                            return "Cancel";
                        case Menu.Create:
                            return "Create";
                        case Menu.Edit:
                            return "Edit";
                        case Menu.Browse:
                            return "Browse";
                        case Menu.OpenLog:
                            return "Open logs";
                        case Menu.OpenLogState:
                            return "Open state logs";
                        case Menu.MaxSizeFile:
                            return "Size max of two copying file";
                        case Menu.LogFormat:
                            return "Log Format";
                        case Menu.JobsApp:
                            return "Job App";
                        case Menu.ExtentionImportantFiles:
                            return "Important Files Extensions";
                        case Menu.ExtentionToEncrypt:
                            return "File extensions to encrypt";
                    }
                    break;
            }
            return "";
        }

        //Translate Backup Type label
        internal string TranslateBackupType(BackupType backupType)
        {
            switch (this.Language)
            {
                case Language.French:
                    switch (backupType)
                    {
                        case BackupType.Complet:
                            return "Sauvegarde complète";
                        case BackupType.Differentiel:
                            return "Sauvegarde differentielle";
                    }
                    break;
                case Language.English:
                    switch (backupType)
                    {
                        case BackupType.Complet:
                            return "Full backup";
                        case BackupType.Differentiel:
                            return "Differential backup";
                    }
                    break;
            }
            return this.TranslateError(Error.InputError);
        }

        //Translate Language put in parameter
        internal string TranslateLanguage(Language language)
        {
            switch (this.Language)
            {
                case Language.French:
                    switch (language)
                    {
                        case Language.French:
                            return "Français";
                        case Language.English:
                            return "Anglais";
                    }
                    break;
                case Language.English:
                    switch (language)
                    {
                        case Language.French:
                            return "French";
                        case Language.English:
                            return "English";
                    }
                    break;
            }
            return this.TranslateError(Error.InputError);
        }

        //Translate Error put in parameter
        internal string TranslateError(Error error)
        {
            if (error == Error.Default) return "";

            switch (this.Language)
            {
                case Language.French:
                    switch (error)
                    {
                        case Error.InputError:
                            return "Saisie incorrecte";
                        case Error.ErrorAppend:
                            return "Une erreur est survenue";
                        case Error.DeleteSuccess:
                            return "Suppression effectuée";
                        case Error.NoSave:
                            return "Aucune sauvegarde";
                        case Error.MaxSaveCountReached:
                            return "Le nombre maximum de sauvegardes a été atteint";
                        case Error.NoSaveSelected:
                            return "Veuillez selectionner une sauvegarde";
                        case Error.JobProgrammRunning:
                            return "Erreur, un programme metier est en cours d'execution";
                        case Error.InstanceExist:
                            return "Erreur, une instance de cette application est en cours d'éxecution";
                    }
                    break;
                case Language.English:
                    switch (error)
                    {
                        case Error.InputError:
                            return "Input error";
                        case Error.ErrorAppend:
                            return "An error has occurred";
                        case Error.DeleteSuccess:
                            return "Delete succeeded";
                        case Error.NoSave:
                            return "No backup available";
                        case Error.MaxSaveCountReached:
                            return "Maximum number of backups reached";
                        case Error.NoSaveSelected:
                            return "You have to choose a backup";
                        case Error.JobProgrammRunning:
                            return "Error, job program is running";
                        case Error.InstanceExist:
                            return "Error, an instance of this app is already running";
                    }
                    
                    break;
            }
            return this.TranslateError(Error.InputError);
        }
        internal string TranslateLogFormat(LogFormat format)
        {
            switch (format)
            {
                case LogFormat.JSON:
                    return "JSON";
                case LogFormat.XML:
                    return "XML";
            }
            return this.TranslateError(Error.InputError);

        }

        internal string TranslateLogString(LogMessage logString)
        {
            switch (Language)
            {
                case (Language.French):
                    switch (logString)
                    {
                        case LogMessage.CopyInProgress:
                            return "Copie en cours : ";
                        case LogMessage.EncryptionInProgress:
                            return "Chiffrement en cours : ";
                        case LogMessage.CopyFinished:
                            return "Copie terminée : ";
                    }
                    break;
                case (Language.English):
                    switch (logString)
                    {
                        case LogMessage.CopyInProgress:
                            return "Copy in progress: ";
                        case LogMessage.EncryptionInProgress:
                            return "Encryption in progress: ";
                        case LogMessage.CopyFinished:
                            return "Copy completed: ";
                    }
                    break;
            }
            return "";
        }

        internal string TranslateLogPlayPauseStop(LogPlayPauseStop Log)
        {
            switch (Language)
            {
                case (Language.French):
                    switch (Log)
                    {
                        case LogPlayPauseStop.CopyResumed:
                            return "Sauvegarde reprise";
                        case LogPlayPauseStop.CopyPaused:
                            return "Sauvegarde mise en pause";
                        case LogPlayPauseStop.CopyStopped:
                            return "Sauvegarde arrêtée";
                    }
                    break;
                case (Language.English):
                    switch (Log)
                    {
                        case LogPlayPauseStop.CopyResumed:
                            return "Backup resumed";
                        case LogPlayPauseStop.CopyPaused:
                            return "Backup paused";
                        case LogPlayPauseStop.CopyStopped:
                            return "Backup stopped";
                    }
                    break;
            }
            return "";
        }
    }
}
