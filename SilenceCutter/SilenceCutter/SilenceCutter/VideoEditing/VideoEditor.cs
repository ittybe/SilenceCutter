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

        /// <summary>
        /// split video by time lines in Dir
        /// </summary>
        /// <param name="timeLines">time lines</param>
        public void Split(TimeLineVolume[] timeLines) 
        {
            FFMpeg ffmpeg = new FFMpeg();
            for (int i = 0; i < timeLines.Length; i++)
            {
                ArgumentContainer container = new ArgumentContainer();
                container.Add(new SplitArgument(timeLines[i]));
                VideoPartInfo partInfo = new VideoPartInfo();

                container.Add(new OutputArgument());
                ffmpeg.Convert(container);
                

            }


        }

        /// <summary>
        /// do same thing as method Split, but create in dir only parts where VolumeValue equels argument
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

    }
}
