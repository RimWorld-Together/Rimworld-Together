using RTServer.Core;
using RTServer.Misc;
using RTShared.Files;
using RTShared.Misc;
using System.IO.Compression;
using RTServer.PacketManagers;
using RTShared.Files.Default;
using static RTShared.Misc.Printer;

namespace RTServer.Managers
{
    public static class BackupManager
    {
        private static readonly Semaphore savingSemaphore = new Semaphore(1, 1);

        public static void BackupServer()
        {
            savingSemaphore.WaitOne();

            try
            {
                string backupName = $"Server_{DateTime.Now.Year}-{DateTime.Now.Month}-{DateTime.Now.Day}_{DateTime.Now.Hour}-{DateTime.Now.Minute}-{DateTime.Now.Second}";
                string backupPath = $"{Master.BackupServerPath + Path.DirectorySeparatorChar}{backupName}{CommonValues.CompressedSaveFormat}";

                List<string> toArchive = new List<string>();
                toArchive.AddRange(Directory.GetFiles(Master.AssetsPath, "*.*", SearchOption.AllDirectories));
                toArchive.AddRange(Directory.GetFiles(Master.ConfigsPath, "*.*", SearchOption.AllDirectories));
                toArchive.AddRange(Directory.GetFiles(Master.LogsPath, "*.*", SearchOption.AllDirectories));

                CreateArchive(toArchive, backupPath);

                if (Directory.GetFiles(Master.BackupServerPath).Count() > Master.BackupConfig.Amount && Master.BackupConfig.AutomaticDeletion == true)
                {
                    DeleteOldestArchive();
                }

                InformationDisplayer.DisplayServerBackup(backupPath);
            }
            catch (Exception ex) { Printer.Error(ex.ToString()); }

            savingSemaphore.Release();
        }

        public static void BackupUser(string username, bool persistent = false)
        {
            savingSemaphore.WaitOne();

            try
            {
                string playerArchivedSavePath = Path.Combine(Master.BackupUsersPath, username);
                if (persistent) playerArchivedSavePath += " - persistent";
                playerArchivedSavePath += CommonValues.CompressedSaveFormat;

                if (File.Exists(playerArchivedSavePath))
                {
                    if (persistent == true)
                    {
                        Printer.Error($"Could not backup user {username} because the file {playerArchivedSavePath} already exist. Consider running a non-persistent backup if you want to overwrite it.");
                        savingSemaphore.Release();
                        return;
                    }

                    else
                    {
                        File.Delete(playerArchivedSavePath);
                        Printer.Warning($"Deleting backup of {username} because he already had one.", Verbosity.Verbose);
                    }
                }

                List<string> toArchive = new List<string>();

                string userFilePath = Path.Combine(Master.UsersPath, username + CommonValues.DefaultSaveFormat);
                if (File.Exists(userFilePath)) toArchive.Add(userFilePath);

                string userSavePath = Path.Combine(Master.SavesPath, username + CommonValues.DefaultSaveFormat);
                if (File.Exists(userSavePath)) toArchive.Add(userSavePath);

                FL_Site[] playerSites = PM_Sites.GetAllSitesFromUsername(username);
                foreach (FL_Site site in playerSites) toArchive.Add(Path.Combine(Master.SitesPath, site.Tile + CommonValues.DefaultSaveFormat));

                FL_Settlement[] playerSettlements = PM_Settlements.GetAllSettlementsFromUsername(username);
                foreach (FL_Settlement settlementFile in playerSettlements) toArchive.Add(Path.Combine(Master.SettlementsPath, settlementFile.Tile + CommonValues.DefaultSaveFormat));

                CreateArchive(toArchive, playerArchivedSavePath);

                InformationDisplayer.DisplayUserBackup(playerArchivedSavePath);
            }
            catch (Exception ex) { Printer.Error(ex.ToString()); }

            savingSemaphore.Release();
        }

        private static void CreateArchive(List<string> files, string toPath)
        {
            using FileStream zip = new FileStream(toPath, FileMode.CreateNew);
            using ZipArchive archive = new ZipArchive(zip, ZipArchiveMode.Create);

            foreach (string file in files)
            {
                if (File.Exists(file))
                {
                    string relativePath = Path.GetRelativePath(Master.MainPath, file);
                    archive.CreateEntryFromFile(file, relativePath);
                }
            }
        }

        private static void DeleteOldestArchive()
        {
            while (Directory.GetFiles(Master.BackupServerPath).Length > Master.BackupConfig.Amount)
            {
                FileSystemInfo fileInfo = new DirectoryInfo(Master.BackupServerPath).GetFileSystemInfos().OrderBy(file => file.CreationTime).First();
                Printer.Warning($"Deleting backup {fileInfo.Name} because we've reached the limit of {Master.BackupConfig.Amount}", Verbosity.Verbose);
                fileInfo.Delete();
            }
        }

        public static void StartFeature()
        {
            if (!Master.BackupConfig.AutomaticBackups) return;
            else
            {
                while (true)
                {
                    Thread.Sleep(TimeSpan.FromHours(Master.BackupConfig.IntervalHours));
                    BackupServer();
                }
            }
        }
    }
}
