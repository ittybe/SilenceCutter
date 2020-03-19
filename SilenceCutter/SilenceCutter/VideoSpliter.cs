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
using Xabe.FFmpeg.Model;

using System.IO;
using Xabe.FFmpeg.Events;

namespace SilenceCutter.VideoManipulating
{
    /// <summary>
    /// split video by volume level
    /// </summary>
    public class VideoSpliter
    {
        /// <summary>
        /// Temp directory for save all splited part
        /// </summary>
        public DirectoryInfo TempDir { get; private set; }
        public IMediaInfo Media { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="media">IMediaInfo object</param>
        /// <param name="tempDirName">Temp directory name for save all splited part</param>
        public VideoSpliter(IMediaInfo media, string tempDirName = "Temp")
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
        public async void SplitVideo(List<TimeLineVolume> DetectedTime, ConversionProgressEventHandler OnProgressHandler, string PreferExtension = FileExtensions.Mp4)
        {
            long PartNumber = 0;

            foreach (var timeSpan in DetectedTime)
            {
                string VolumeLevel = timeSpan.Volume == VolumeValue.Silence ?
                    "S" : "N";

                string outputFileName = $"{PartNumber++}{VolumeLevel}{PreferExtension}";
                string outputPath = $"{TempDir.Name}\\{outputFileName}";

                IStream audioStream = Media.AudioStreams.FirstOrDefault();
                IStream videoStream = Media.VideoStreams.FirstOrDefault();

                IConversion conversion = Conversion.New()
                    .AddStream(audioStream, videoStream)
                    .AddParameter($"-ss {timeSpan.Start} -t {timeSpan.Duration}")
                    .SetOutput(outputPath);
                conversion.OnProgress += OnProgressHandler;

                _ = await conversion.Start();
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
        public void Convert()
        {

        }
    }
}
