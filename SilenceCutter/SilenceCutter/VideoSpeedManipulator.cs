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

using System.IO;
using Xabe.FFmpeg.Events;


namespace SilenceCutter.VideoManipulating
{
    /// <summary>
    /// Change Speed of the video
    /// </summary>
    public class VideoSpeedManipulator
    {
        /// <summary>
        /// temp directory for video parts
        /// </summary>
        public DirectoryInfo TempDir { get; private set; }
        public List<TimeLineVolume> DetectedTime{ get; protected set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tempDir">temp directory that contains video parts</param>
        public VideoSpeedManipulator(string tempDir, List<TimeLineVolume> detectedTime) 
        {
            TempDir = new DirectoryInfo(tempDir);
            DetectedTime = detectedTime;
        }

        /// <summary>
        /// change speed of the video
        /// </summary>
        /// <param name="noiseSpeed">noise change speed (0.25, 0.5 ...)</param>
        /// <param name="PreferExtension">file extension like .mp4, .mkv, ... etc.(use same extension as origin video files to make faster conversion)</param>
        /// <param name="silenceSpeed">silence change speed (0.25, 0.5 ...)</param>
        /// <param name="OnProgressHandler">Event handler OnProgress interface IConversion</param>
        public void ChangeSpeed(
            double silenceSpeed, double noiseSpeed, string PreferExtension,
            ConversionProgressEventHandler OnProgressHandler = null) 
        {
            VideoPartsContainer container = new VideoPartsContainer(DetectedTime, TempDir.FullName, FileExtensions.Mp4);
            for (int i = 0; i < DetectedTime.Count; i++) 
            {
                double changeSpeed = DetectedTime[i].Volume == VolumeValue.Silence ? silenceSpeed : noiseSpeed;

                // Calc speed value
                double audioSpeed = 1 * changeSpeed;
                double videoSpeed = 1 / changeSpeed;

                // format audio double value
                string audioSpeedStr = audioSpeed.ToString().Replace(',', '.');
                string videoSpeedStr = videoSpeed.ToString().Replace(',', '.');

                IConversion conversion = Conversion.New()
                    .AddParameter(" -y ")
                    .AddParameter($"-i \"{container[i].FullName}\" ")
                    .AddParameter($" -af \"atempo = {audioSpeedStr}\", ")       // audio speed op
                    .AddParameter($" -vf \"setpts = {videoSpeedStr} * PTS\" ") // video speed up
                    .SetOutput($"\"{container[i].FullName}\""); // overwrite param, yes we agree to over write this part
                    
                conversion.OnProgress += OnProgressHandler;


                conversion.Start().Wait();
            }
        }
    }
}
