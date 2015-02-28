using System;
using System.IO;
using System.Linq;

namespace Diagnosis.Common
{
    public static class FileHelper
    {
        /// <summary>
        /// Make a numbered backup copy of the specified file. 
        /// Backup files have the name yymmdd-##-filename, where yymmdd is the date
        /// and ## is a zero justified sequence number starting at 0
        /// </summary>
        /// <param name="fileName">Name of the file to backup.</param>
        /// <param name="backupDir">Backup folder.</param>
        /// <param name="maxBackupsPerDay">The maximum backups to keep per day, up to 99.</param>
        /// <param name="maxDaysToKeep">The maximum distinct days to keep.</param>
        /// <remarks>based on http://www.rajapet.com/2014/03/a-file-versioning-helper-class-in-c-to-make-a-backup-copy-of-a-file-and-keep-the-last-n-copies-of-that-file.html </remarks>
        public static void Backup(string fileName, string backupDir, int maxBackupsPerDay = 5, int maxDaysToKeep = 7)
        {
            var maxNumber = 99;
            maxBackupsPerDay = Math.Min(Math.Max(maxBackupsPerDay, 1), maxNumber); // from 1 to 99
            maxDaysToKeep = Math.Max(maxDaysToKeep, 1); // from 1

            var file = new FileInfo(fileName);
            if (file.Exists)
            {
                var dir = new DirectoryInfo(backupDir);
                if (!dir.Exists)
                    dir.Create();

                // Get the list of previous backups of the file, skipping the current file
                // do not touch non-backup files
                var backupFiles = Directory.GetFiles(backupDir, "*" + file.Name)
                    .ToList()
                    .Where(d => !d.Equals(file.FullName) && Path.GetFileName(d).Length == 10 + fileName.Length) // 10 - date and number length
                    .OrderBy(d => d);

                var byDays = (from f in backupFiles
                              group f by f.Substring(backupDir.Length, 6) into gr
                              select new
                              {
                                  Date = gr.Key,
                                  Files = gr.ToList()
                              }).ToDictionary(x => x.Date, x => x.Files);

                // delete out of date files
                var expiredDates = byDays.Keys
                    .Take(byDays.Keys.Count - maxDaysToKeep);
                foreach (var date in expiredDates)
                {
                    byDays[date].ForEach(f => File.Delete(f));
                }

                // note - we leave all backups between (today - maxDaysToKeep) and today

                int newSequence = 0;
                var today = String.Format("{0:yyMMdd}", DateTime.Now);

                Action<string, int> rename = (oldName, seqNum) =>
                {
                    var newName = new FileInfo(Path.Combine(dir.Name,
                                    String.Format("{0}-{1:00}-{2}", today, seqNum, file.Name)));
                    File.Move(oldName, newName.FullName);
                };

                // If we have at least one today backup copy
                var lastTodayBackup = byDays.GetValueOrDefault(today).LastOrDefault();
                if (lastTodayBackup != null)
                {
                    // delete oldest todays backups
                    var todayFiles = byDays[today];
                    var todayCopiesCount = todayFiles.Count;
                    if (todayCopiesCount >= maxBackupsPerDay)
                    {
                        var expiredFiles = backupFiles.Take(todayCopiesCount - maxBackupsPerDay + 1);
                        foreach (var expiredFile in expiredFiles)
                        {
                            File.Delete(expiredFile);
                            todayFiles.Remove(expiredFile);
                        }
                    }

                    // Get the last sequence number back taking the 2 middle characters and convert them to an int. And add 1 to that number
                    string number = Path.GetFileName(lastTodayBackup).Substring(7, 2);
                    if (Int32.TryParse(number, out newSequence))
                        newSequence++;

                    if (newSequence > maxNumber)
                    {
                        // rename all files starting from zero
                        newSequence = 0;
                        todayFiles.ForEach(f =>
                        {
                            rename(f, newSequence);
                            newSequence++;
                        });
                    }
                }

                // new backup
                var backupFile = new FileInfo(Path.Combine(dir.Name,
                                    String.Format("{0}-{1:00}-{2}", today, newSequence, file.Name)));
                File.Copy(file.FullName, backupFile.FullName, true);
            }
        }

        public static void CreateDirectoryForPath(string path)
        {
            string directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
        }
    }
}