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
    /// Change Speed of the video
    /// </summary>
    public class VideoSpeedManipulator
    {
        public DirectoryInfo TempDir { get; private set; }
        public VideoSpeedManipulator(DirectoryInfo tempDir) 
        {
            TempDir = tempDir;

        }

        /// <summary>
        /// change speed of the video
        /// </summary>
        /// <param name="videoPath">video that we need to speed up or slow down</param>
        /// <param name="outputPath">video path as output (you can enter same value as videoPath)</param>
        /// <param name="changeSpeed">change speed (0.25, 0.5 ...)</param>
        /// <param name="overwrite">overwrite outputPath, it videoPath and outputPath is the same, this arg have to be true</param>
        /// <param name="OnProgressHandler">Event handler OnProgress interface IConversion</param>
        public async void ChangeSpeed(
            FileInfo videoPath, FileInfo outputPath, 
            double changeSpeed, bool overwrite = true, 
            ConversionProgressEventHandler OnProgressHandler = null) 
        {
            // param overwrite -y - overwrite, -n - not overwrite
            string overwriteParam = overwrite ?
                "-y" : "-n";

            // Calc speed value
            double audioSpeed = 1 * changeSpeed;
            double videoSpeed = 1 / changeSpeed;

            // format audio double value
            string audioSpeedStr = audioSpeed.ToString().Replace(',', '.');
            string videoSpeedStr = videoSpeed.ToString().Replace(',', '.');

            // media streams
            IMediaInfo mediaInfo = await MediaInfo.Get(videoPath.FullName);
            IStream videoStream = mediaInfo.VideoStreams.FirstOrDefault();
            IStream audioStream = mediaInfo.AudioStreams.FirstOrDefault();
            
            IConversion conversion = Conversion.New()
                .AddStream(audioStream, videoStream)
                .AddParameter($"-filter:a \"atempo = {audioSpeedStr}\"")
                .AddParameter($"-filter:v \"setpts = {videoSpeedStr} * PTS\"")
                .AddParameter(overwriteParam)
                .SetOutput(outputPath.FullName);

            conversion.OnProgress += OnProgressHandler;

            await conversion.Start();
        }
    }
}











