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
    public class VideoMerger
    {
        /// <summary>
        /// Temp directory for save all splited part
        /// </summary>
        public DirectoryInfo TempDir { get; private set; }

        /// <summary>
        /// output fileInfo
        /// </summary>
        public FileInfo outputFile { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="FilePath">output filepath</param>
        /// <param name="tempDirName">Temp directory path for save all splited part</param>
        public VideoMerger(string FilePath, string tempDirName)
        {
            outputFile = new FileInfo(FilePath);
            TempDir = new DirectoryInfo(tempDirName);
            if (!TempDir.Exists)
                throw new ArgumentException($"Directory \"{tempDirName}\" doesn't exist!");
        }


        /// <summary>
        /// split video on part with only silent or noise
        /// </summary>
        ///<param name="container">contains video part names</param>
        /// <param name="OnProgressHandler">handler for event OnProgress IConvertion's object </param>
        /// <param name="PreferExtension">prefer extension for splited parts of video</param>
        public void MergeVideo(VideoPartsContainer container, ConversionProgressEventHandler OnProgressHandler = null, string PreferExtension = FileExtensions.Mp4)
        {
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
                .SetOutput(outputFile.FullName);
            
            conversion.Start().Wait();

            // delete already useless file

            videoPartsList.Delete();
        }
    }
}
