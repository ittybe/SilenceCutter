using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FFMpegCore.FFMPEG;
using FFMpegCore.FFMPEG.Argument;
using SilenceCutter.VideoEditing;


namespace SilenceCutterTests
{
    public class Program
    {
        static void Main(string[] args)
        {
            //new FFMpeg().Convert();
            ArgumentContainer container = new ArgumentContainer();
            VideoPartInfo partInfo = new VideoPartInfo();
            partInfo.FromString("VideoName_NOISEsfsNOISE_23.mp4");
            
            Console.WriteLine(partInfo);
            Console.WriteLine(partInfo.Name);
            Console.WriteLine(partInfo.Mark);
            Console.WriteLine(partInfo.Number);
            Console.WriteLine(partInfo.FileExtension);

        }
    }
}
