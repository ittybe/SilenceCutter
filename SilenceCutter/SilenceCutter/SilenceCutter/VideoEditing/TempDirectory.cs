using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SilenceCutter.VideoEditing
{
    static class TempDirectory
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns>temp directory in sys temp dir, that not exists</returns>
        static public DirectoryInfo GetTempDir() 
        {
            // get temp system dir 
            DirectoryInfo tempSysDir = new DirectoryInfo(Path.GetTempPath());

            // check if tempDir name is not Exists, if exists random another name
            string tempDirName;
            while (true)
            {
                // full path to temp directory
                tempDirName = $"{tempSysDir.FullName}{Path.DirectorySeparatorChar}{RandomString(20)}";

                DirectoryInfo tempDir = new DirectoryInfo(tempDirName);
                if (!tempDir.Exists)
                    return tempDir;
            }
        }
        // Generate a random string with a given size  
        public static string RandomString(int size, bool lowerCase = true)
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }
            if (lowerCase)
                return builder.ToString().ToLower();
            string result = builder.ToString();
            return result;
        }

    }
}
