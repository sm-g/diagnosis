using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Diagnosis.Common
{
    public static class FileHelper
    {
        // from http://www.rajapet.com/2014/03/a-file-versioning-helper-class-in-c-to-make-a-backup-copy-of-a-file-and-keep-the-last-n-copies-of-that-file.html
        /// <summary>
        /// Make a numbered backup copy of the specified file.  Backup files have the name filename.exe.yymmdd##, where yymmdd is the date and ## is a zero justified sequence number starting at 1
        /// </summary>
        /// <param name="fileName">Name of the file to backup.</param>
        /// <param name="backupDir">Backup folder.</param>
        /// <param name="maxBackupsPerDay">The maximum backups to keep, up to 99.</param>
        public static void Backup(string fileName, string backupDir, int maxBackupsPerDay)
        {
            maxBackupsPerDay = Math.Min(maxBackupsPerDay, 99);
            var file = new FileInfo(fileName);
            if (file.Exists)
            {
                var dir = new DirectoryInfo(backupDir);

                if (!dir.Exists)
                    dir.Create();

                int newSequence = 0;

                // Get the list of previous backups of the file, skipping the current file
                var backupFiles = Directory.GetFiles(backupDir, "*" + file.Name)
                    .ToList()
                    .Where(d => !d.Equals(file.FullName) && Path.GetFileName(d).Length == 10 + fileName.Length) // 10 - date and number
                    .OrderBy(d => d);

                // Get the name of the last backup performed
                var lastBackupFilename = backupFiles.LastOrDefault();

                // If we have at least one previous backup copy
                if (lastBackupFilename != null)
                {
                    // Get the last sequence number back taking the 2 middle characters and convert them to an int. And add 1 to that number
                    string number = "";
                    try
                    {
                        number = Path.GetFileName(lastBackupFilename).Substring(7, 2);
                        if (Int32.TryParse(number, out newSequence))
                            newSequence++;
                    }
                    catch // not this backup, will always rewrite 00
                    {
                    }

                    if (backupFiles.Count() >= maxBackupsPerDay)
                    {
                        // Get a list of the oldest files to delele
                        var expiredFiles = backupFiles.Take(backupFiles.Count() - maxBackupsPerDay + 1);

                        foreach (var expiredFile in expiredFiles)
                        {
                            File.Delete(expiredFile);
                        }
                    }
                }
                var backupFile = new FileInfo(Path.Combine(dir.Name,
                    String.Format("{0:yyMMdd}-{1:00}-{2}", DateTime.Now, newSequence, file.Name)));

                File.Copy(file.FullName, backupFile.FullName, true);
            }
        }
    }
}
