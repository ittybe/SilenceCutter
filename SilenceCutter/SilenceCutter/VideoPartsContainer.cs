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
        public VideoPartsContainer(List<TimeLineVolume> DetectedTime, string tempDir, string preferExtension, string noiseMark, string silenceMark) 
        {
            Container = new List<VideoPartName>();
            TempDir = new DirectoryInfo(tempDir);
            long SplitedPartNumber = 0;

            foreach (var timeSpan in DetectedTime)
            {
                // mark silence as 'S' and noise as 'N'
                string VolumeLevel = timeSpan.Volume == VolumeValue.Silence ?
                    silenceMark : noiseMark;

                var newName = new VideoPartName(VolumeLevel, SplitedPartNumber++, preferExtension);
                Container.Add(newName);
            }
        }

        /// <summary>
        /// Create container from temp directory
        /// </summary>
        /// <param name="videoFiles">video file from temp directory</param>
        /// <param name="tempDir">temp dir from where we create container</param>
        public VideoPartsContainer(FileInfo[] videoFiles, string tempDir) 
        {
            Container = new List<VideoPartName>();
            TempDir = new DirectoryInfo(tempDir);
            foreach (var videoFile in videoFiles)
            {
                if (!(TempDir == videoFile.Directory)) 
                {
                    throw new ArgumentException($"video file {videoFile.FullName} not in temp directory {TempDir.FullName}");
                }
            }
            foreach (var videoFile in videoFiles)
            {
                Container.Add((VideoPartName)videoFile.Name);
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
        /// <summary>
        /// remove all video files in temp directory
        /// </summary>
        public void RemoveVideoFiles() 
        {
            for (int i = 0; i < Container.Count; i++)
            {
                this[i].Delete();
            }
            Container.Clear();
        }
    }
}
