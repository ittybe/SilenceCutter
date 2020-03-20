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
    class VideoMerger
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
        /// <param name="tempDirName">Temp directory name for save all splited part</param>
        public VideoMerger(string FilePath, string tempDirName = "Temp")
        {
            outputFile = new FileInfo(FilePath);
            TempDir = new DirectoryInfo(tempDirName);
            if (!TempDir.Exists)
                throw new ArgumentException($"Directory \"{tempDirName}\" doesn't exist!");
        }
        /// <summary>
        /// split video on part with only silent or noise
        /// </summary>
        /// <param name="OnProgressHandler">handler for event OnProgress IConvertion's object </param>
        /// <param name="PreferExtension">prefer extension for splited parts of video</param>
        public async void MergeVideo(ConversionProgressEventHandler OnProgressHandler = null, string PreferExtension = FileExtensions.Mp4)
        {
            long SplitedPartNumber = 0;
            var fileList = TempDir.GetFiles();

            for (int i = 0; i < fileList.Length; i++)
            {
                // find file
                
                FileInfo inputFile = Array.Find(fileList, file => file.Name.IndexOf(SplitedPartNumber.ToString()) != -1);
                SplitedPartNumber++;
         
                // detect volume value

                bool isSilence = inputFile.Name[0].CompareTo('S') == 0 ? true : false;

                IConversion conversion = Conversion.New()
                .AddParameter("")
                .SetOutput(outputFile.FullName);
                conversion.OnProgress += OnProgressHandler;

                _ = await conversion.Start();
            }

            
        }
    }
}
