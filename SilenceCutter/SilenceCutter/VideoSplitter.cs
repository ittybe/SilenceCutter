using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xabe.FFmpeg.Streams;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Enums;

using SilenceCutter.Detecting;
using SilenceCutter.VideoPartNaming;
using Xabe.FFmpeg.Model;

using System.IO;
using Xabe.FFmpeg.Events;

namespace SilenceCutter.VideoManipulating
{
    /// <summary>
    /// split video by volume level
    /// </summary>
    public class VideoSplitter
    {
        /// <summary>
        /// Temp directory for save all splited part
        /// </summary>
        public DirectoryInfo TempDir { get; protected set; }

        private IMediaInfo media;

        /// <summary>
        /// Media property
        /// </summary>
        public IMediaInfo Media 
        {
            get { return media; }
            protected set { media = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="media">IMediaInfo object</param>
        /// <param name="tempDirName">Temp directory name for save all splited part</param>
        public VideoSplitter(IMediaInfo media, string tempDirName)
        {
            Media = media;
            TempDir = new DirectoryInfo(tempDirName);
            if (!TempDir.Exists)
                TempDir.Create();
        }

        /// <summary>
        /// split video on part with only silent or noise
        /// </summary>
        /// <param name="DetectedTime">result of method SilenceCutter.Detecting.VolumeDetector.DetectVolumeLevel()</param>
        /// <param name="OnProgressHandler">handler for event OnProgress IConvertion's object </param>
        /// <param name="PreferExtension">prefer extension for splited parts of video</param>
        public void SplitVideo(List<TimeLineVolume> DetectedTime, string PreferExtension, ConversionProgressEventHandler OnProgressHandler = null)
        {
            VideoPartsContainer container = VideoPartNamesGenerator.GenerateNames(DetectedTime, TempDir, PreferExtension);
            for (int i = 0; i < DetectedTime.Count; i++)
            {
                string outputPath = container[i].FullName;

                IStream audioStream = Media.AudioStreams.FirstOrDefault();
                IStream videoStream = Media.VideoStreams.FirstOrDefault();

                IConversion conversion = Conversion.New()
                    .AddStream(audioStream, videoStream)
                    .AddParameter($"-ss {DetectedTime[i].Start} -t {DetectedTime[i].Duration}")
                    .SetOutput(outputPath);
                conversion.OnProgress += OnProgressHandler;
                
                // wait because it go thought and if we have to convert many video part, it will crush OS

                conversion.Start().Wait();
            }
        }

        /// <summary>
        /// Clear temp directory
        /// </summary>
        public void ClearTempDir() 
        {
            foreach (FileInfo file in TempDir.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in TempDir.GetDirectories())
            {
                dir.Delete(true);
            }
        }
        
        /// <summary>
        /// remove directory
        /// </summary>
        public void RemoveTempDir() 
        {
            TempDir.Delete(true);
        }
    }
}
