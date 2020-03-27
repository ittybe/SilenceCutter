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
    public class VideoSpeedManipulator : VideoManipulator
    {
        // TODO rewrite all summary
        /// <summary>
        /// noise copy mark for speeded up video part names in temp directory
        /// </summary>
        public const string NoiseCopyMark = "NCopy";
        /// <summary>
        /// silence copy mark for speeded up video part names in temp directory
        /// </summary>
        public const string SilenceCopyMark = "SCopy";

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
        /// 
        /// </summary>
        /// <param name="tempDir">temp directory that contains video parts</param>
        public VideoSpeedManipulator(List<TimeLineVolume> detectedTime, string tempDir, string noiseMark, string silenceMark)  : base(detectedTime, tempDir, noiseMark, silenceMark)
        {
            if (!TempDir.Exists)
                throw new ArgumentException($"Temp directory {TempDir.FullName} is not exists");
        }

        /// <summary>
        /// change speed of the video
        /// </summary>
        /// <param name="noiseSpeed">noise change speed (0.25, 0.5 ...) WARNING: if you set this param more than ~ 10 (depends of the smallest duration value in list TimeLineVolume), it will corrupt the output final video</param>
        /// <param name="Extension">video part extension in temp directory like .mp4, .mkv, ... etc.(use same extension as origin video files to make faster conversion)</param>
        /// <param name="silenceSpeed">silence change speed (0.25, 0.5 ...) WARNING: if you set this param more than ~ 10 (depends of the smallest duration value in list TimeLineVolume), it will corrupt the output final video</param>
        /// <param name="OnProgressHandler">Event handler OnProgress interface IConversion</param>
        public void ChangeSpeed(
            double silenceSpeed, double noiseSpeed, string Extension, 
            ConversionProgressEventHandler OnProgressHandler = null) 
        {
            VideoPartsContainer container = new VideoPartsContainer(DetectedTime, TempDir.FullName, Extension, noiseMark, silenceMark);
            VideoPartsContainer containerCopy = new VideoPartsContainer(DetectedTime, TempDir.FullName, Extension, NoiseCopyMark, SilenceCopyMark);
            for (int i = 0; i < DetectedTime.Count; i++) 
            {
                double changeSpeed = DetectedTime[i].Volume == VolumeValue.Silence ? silenceSpeed : noiseSpeed;

                // Calc speed value
                double audioSpeed = 1 * changeSpeed;
                double videoSpeed = 1 / changeSpeed;

                // format audio double value
                string audioSpeedStr = audioSpeed.ToString().Replace(',', '.');
                string videoSpeedStr = videoSpeed.ToString().Replace(',', '.');

                // get media
                IMediaInfo media = MediaInfo.Get(container[i].FullName).Result;

                IStream audioStream = media.AudioStreams.FirstOrDefault();
                IStream videoStream = media.VideoStreams.FirstOrDefault();

                IConversion conversion = Conversion.New()
                    .AddStream(audioStream, videoStream)
                    .AddParameter(" -video_track_timescale 900000 ")
                    .AddParameter($"-filter:a \"atempo = {audioSpeedStr}\"")
                    .AddParameter($"-filter:v \"setpts = {videoSpeedStr} * PTS\"")
                    .SetOutput(containerCopy[i].FullName);
                    
                conversion.OnProgress += OnProgressHandler;

                conversion.Start().Wait();
            }
            container.RemoveVideoFiles();
        }
    }
}
