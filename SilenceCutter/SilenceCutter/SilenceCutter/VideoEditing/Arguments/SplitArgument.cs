using FFMpegCore.FFMPEG.Argument;
using SilenceCutter.Detecting;
using System;
using System.Collections.Generic;
using System.Text;

namespace SilenceCutter.VideoEditing.Arguments
{
    /// <summary>
    /// Split argument
    /// </summary>
    public class SplitArgument : Argument<TimeLineVolume>
    {
        public SplitArgument(TimeLineVolume _timeLine) 
        {
            Value = _timeLine;
        }

        public override string GetStringValue()
        {
            return $"--ss {Value.Start} -t {Value.Duration}";
        }
    }
}
