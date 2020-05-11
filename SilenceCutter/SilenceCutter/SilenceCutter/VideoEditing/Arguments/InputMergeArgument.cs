using FFMpegCore.FFMPEG.Argument;
using System;
using System.Collections.Generic;
using System.Text;

namespace SilenceCutter.VideoEditing.Arguments
{
    class InputMergeArgument: Argument<VideoPartInfo[]>
    {
        public InputMergeArgument(VideoPartInfo[] parts) 
        {
            Value = parts;
        }

        public override string GetStringValue()
        {
            string inputs
            foreach (var part in Value)
            {

            }
            return "-i ";
        }
    }
}
