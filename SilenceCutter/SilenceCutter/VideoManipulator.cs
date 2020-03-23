using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SilenceCutter.Detecting;
using SilenceCutter.VideoPartNaming;

using System.IO;


namespace SilenceCutter.VideoManipulating
{
    /// <summary>
    /// super class for all video manipulating classes
    /// </summary>
    public abstract class VideoManipulator
    {
        /// <summary>
        /// silence mark for video part name
        /// </summary>
        protected string silenceMark;
        /// <summary>
        /// noise mark for video part name
        /// </summary>
        protected string noiseMark;
        /// <summary>
        /// detected time
        /// </summary>
        protected List<TimeLineVolume> detectedTime;
        /// <summary>
        /// temp dir where places all video file 
        /// </summary>
        protected DirectoryInfo tempDir;
        public VideoManipulator(List<TimeLineVolume> detectedTime, string tempDir, string noiseMark, string silenceMark)
        {
            this.detectedTime = detectedTime;
            this.noiseMark = noiseMark;
            this.silenceMark = silenceMark;
            this.tempDir = new DirectoryInfo(tempDir);
        }
    }
}
