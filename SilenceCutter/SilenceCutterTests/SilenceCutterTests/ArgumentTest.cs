using FFMpegCore.FFMPEG.Argument;
using SilenceCutter.Detecting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SilenceCutterTests
{
    class ArgumentTest : Argument<TimeLineVolume>
    {
        private TimeLineVolume timeLine;
        public ArgumentTest(TimeLineVolume timeLine)  
        {
            this.timeLine = timeLine;
        }
        public override string GetStringValue()
        {
            return $"--ss {timeLine.Start} -t {timeLine.Duration}";
        }
    }
}
