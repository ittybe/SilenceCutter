using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FFMpegCore.FFMPEG;
using FFMpegCore.FFMPEG.Argument;
using FFMpegCore.FFMPEG.Argument.Fluent;
using FFMpegCore.FFMPEG.Enums;
using SilenceCutter.VideoEditing;


namespace SilenceCutterTests
{
    public class Program
    {
        static void Main(string[] args)
        {
            //new FFMpeg().Convert();
            ArgumentContainer container = new ArgumentContainer();
            FFMpeg ffmpeg = new FFMpeg();
            var container_ = new ArgumentContainer()
                  .VideoCodec(VideoCodec.LibX264)
                  .Scale(VideoSize.Hd);

            VideoPartInfo partInfo = new VideoPartInfo();
            partInfo.FromString($"VideoName_NOISE_23.mp4");
            
            Console.WriteLine(partInfo);
            Console.WriteLine(partInfo.Name);
            Console.WriteLine(partInfo.IsNoise);
            Console.WriteLine(partInfo.Number);
            Console.WriteLine(partInfo.FileExtension);

            partInfo.IsNoise = !partInfo.IsNoise;
            Console.WriteLine(partInfo);
        }
    }
}
