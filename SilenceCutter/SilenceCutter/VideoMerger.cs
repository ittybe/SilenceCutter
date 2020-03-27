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
    /// merge video parts, splited by video splitter
    /// </summary>
    public class VideoMerger : VideoManipulator
    {
        /// <summary>
        /// temp directory for video parts
        /// </summary>
        public DirectoryInfo TempDir
        {
            get
            {
                return base.tempDir;
            }
        }
        /// <summary>
        /// list of time line with definition of volume level
        /// </summary>
        public List<TimeLineVolume> DetectedTime
        {
            get
            {
                return detectedTime;
            }
            protected set
            {
                detectedTime = value;
            }
        }

        /// <summary>
        /// output fileInfo
        /// </summary>
        public FileInfo OutputPath { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="FilePath">output filepath</param>
        /// <param name="tempDirName">Temp directory path for save all splited part</param>
        /// <param name="detectedTime">Detected time</param>
        public VideoMerger(List<TimeLineVolume> detectedTime, string tempDir, string noiseMark, string silenceMark, string outputPath) : base(detectedTime, tempDir, noiseMark, silenceMark)
        {
            OutputPath = new FileInfo(outputPath);
            if (!TempDir.Exists)
                throw new ArgumentException($"Temp directory {TempDir.FullName} is not exists");
        }


        /// <summary>
        /// split video on part with only silent or noise
        /// </summary>
        /// <param name="OnProgressHandler">handler for event OnProgress IConvertion's object </param>
        /// <param name="PreferExtension">prefer extension for splited parts of video</param>
        public void MergeVideo(string PreferExtension, ConversionProgressEventHandler OnProgressHandler = null)
        {
            VideoPartsContainer container = new VideoPartsContainer(DetectedTime, TempDir.FullName, PreferExtension, noiseMark, silenceMark);

            // create and write to file, that places in temp windows directory, all video part names

            FileInfo videoPartsList = new FileInfo(Path.ChangeExtension(Path.GetTempFileName(), ".txt"));
            StreamWriter writer = File.CreateText(videoPartsList.FullName);
            foreach (var videoPart in container.Container) 
            {
                string fullpath = $"{container.TempDir.FullName}\\{videoPart}";
                writer.WriteLine($"file '{fullpath}'");
            }
            writer.Flush();
            writer.Dispose();
            // concate all videopart to one video

            IConversion conversion = Conversion.New()
                .AddParameter($"-f concat -safe 0 -i \"{videoPartsList.FullName}\" -c copy")
                .SetOutput(OutputPath.FullName);

            conversion.OnProgress += OnProgressHandler;

            conversion.Start().Wait();

            // delete already useless file
            videoPartsList.Delete();
        }
    }
}
