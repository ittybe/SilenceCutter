using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SilenceCutter.VideoPartNaming;

namespace SilenceCutter.VideoManipulating
{
    public class VideoPartsContainer
    {
        /// <summary>
        /// temp directory for files
        /// </summary>
        public DirectoryInfo TempDir { get; set; }
        /// <summary>
        /// contains every video part
        /// </summary>
        public List<VideoPartName> Container { get; private set; }
        public VideoPartsContainer(string tempDirPath)
        {
            Container = new List<VideoPartName>();
            TempDir = new DirectoryInfo(tempDirPath);
        }
        /// <summary>
        /// sort list by part number
        /// </summary>
        public void SortByPartNumber()
        {
            Container.Sort(new VideoPartNameComparer());
        }
        /// <summary>
        /// get video info
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public FileInfo this[int index] 
        {
            get 
            {
                string path = $"{TempDir.FullName}\\{Container[index]}";
                FileInfo videoInfo = new FileInfo(path);
                return videoInfo;
            }
        }
    }
}
