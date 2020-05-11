using FFMpegCore.FFMPEG;
using FFMpegCore.FFMPEG.Argument;
using SilenceCutter.Detecting;
using SilenceCutter.VideoEditing.Arguments;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SilenceCutter.VideoEditing
{
    /// <summary>
    /// video editor, that split, change speed, merge media files 
    /// </summary>
    public class VideoEditor
    {

        /// <summary>
        /// Media file
        /// </summary>
        public FileInfo Source { get; set; }

        /// <summary>
        /// directory where place all video part
        /// </summary>
        public DirectoryInfo Dir { get; set; }

        protected string SourceExtension
        {
            get 
            {
                return Path.GetExtension(Source.FullName);
            }
        }

        /// <summary>
        /// split video by time lines in Dir
        /// </summary>
        /// <param name="timeLines">time lines</param>
        public void Split(TimeLineVolume[] timeLines) 
        {
            char sep = Path.DirectorySeparatorChar;
            FFMpeg ffmpeg = new FFMpeg();

            for (int i = 0; i < timeLines.Length; i++)
            {
                ArgumentContainer container = new ArgumentContainer();
                // input arg
                container.Add(new InputArgument(Source));

                // split arg
                container.Add(new SplitArgument(timeLines[i]));
                
                // output path
                VideoPartInfo partInfo = new VideoPartInfo() 
                {
                    IsNoise = timeLines[i].Volume == VolumeValue.Noise ? true: false,
                    Name = "videoPart",
                    FileExtension = SourceExtension,
                    Number = i,
                };
                string outputPath = $"{Dir.FullName}{sep}{partInfo.FullName}";
                container.Add(new OutputArgument(outputPath));

                // convert 
                ffmpeg.Convert(container);
            }
        }

        /// <summary>
        /// do same thing as method Split, but create in dir only parts where VolumeValue equals argument
        /// </summary>
        /// <param name="timeLines">time lines</param>
        /// <param name="value">value that stays</param>
        public void Cut(TimeLineVolume[] timeLines, VolumeValue value) 
        {

        }

        /// <summary>
        /// merge all parts in Dir 
        /// </summary>
        /// <param name="outputPath">output</param>
        public void Merge(string outputPath) 
        {

        }

        /// <summary>
        /// change speed of parts in Dir
        /// </summary>
        /// <param name="noiseSpeed">noise speed</param>
        /// <param name="silenceSpeed">silence speed</param>
        public void ChangeSpeed(decimal noiseSpeed, decimal silenceSpeed) 
        {

        }

        /// <summary>
        /// get video parts info from directory
        /// </summary>
        /// <param name="dir">dir</param>
        /// <returns></returns>
        static VideoPartInfo[] GetVideoPartsInfo(DirectoryInfo dir) 
        {
            List<VideoPartInfo> partsInfo = new List<VideoPartInfo>();
            foreach (var mediaFile in dir.GetFiles())
            {
                VideoPartInfo partInfo = VideoPartInfo.FromStringToVideoPartInfo(mediaFile.Name);
                partsInfo.Add(partInfo);
            }
            return partsInfo.ToArray();
        }
    }
}
