using System;
using System.IO;
using System.Linq;

namespace Diagnosis.Common
{
    public static class FileHelper
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(FileHelper));

        public static void CreateDirectoryForPath(string path)
        {
            string directory = Path.GetDirectoryName(path);
            if (!directory.IsNullOrEmpty() && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);
        }

        public static FileInfo CreateNewFile(string filename)
        {
            FileInfo newFile = new FileInfo(filename);
            for (int i = 0; i < 5; i++)
            {
                if (newFile.Exists)
                {
                    try
                    {
                        newFile.Delete();
                        break;
                    }
                    catch (Exception e)
                    {
                        logger.WarnFormat("Error when delete {0}: {1}", filename, e);
                    }
                }
            }
            newFile = new FileInfo(filename);
            return newFile;
        }
    }
}