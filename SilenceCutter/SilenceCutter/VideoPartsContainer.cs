using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SilenceCutter.VideoPartNaming;
using SilenceCutter.Detecting;

namespace SilenceCutter.VideoManipulating
{
    class VideoPartNameComparer : IComparer<VideoPartName>
    {
        public int Compare(VideoPartName x, VideoPartName y)
        {
            return x.PartNumber.CompareTo(y.PartNumber);
        }
    }

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
        /// Generate names depends of detectedTime 
        /// </summary>
        /// <param name="DetectedTime">DetectVolumeLevel result</param>
        /// <param name="preferExtension">Something from FileExtensions (Xabe lib) </param>
        /// <param name="tempDir">dir for containing all splited video parts</param>
        /// <returns></returns>
        public VideoPartsContainer(List<TimeLineVolume> DetectedTime, string tempDir, string preferExtension, string noiseMark = "N", string silenceMark = "S") 
        {
            Container = new List<VideoPartName>();
            TempDir = new DirectoryInfo(tempDir);
            long SplitedPartNumber = 0;

            foreach (var timeSpan in DetectedTime)
            {
                // mark silence as 'S' and noise as 'N'
                string VolumeLevel = timeSpan.Volume == VolumeValue.Silence ?
                    noiseMark : silenceMark;

                var newName = new VideoPartName(VolumeLevel, SplitedPartNumber++, preferExtension);
                Container.Add(newName);
            }
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
