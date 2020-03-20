using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SilenceCutter.Detecting;
using SilenceCutter.VideoManipulating;
using System.IO;

namespace SilenceCutter.VideoPartNaming
{
    public static class VideoPartNamesGenerator
    {
        /// <summary>
        /// we define part video with this code as Silence
        /// </summary>
        public const string PartCodeSilence = "S";

        /// <summary>
        /// we define part video with this code as Noise
        /// </summary>
        public const string PartCodeNoise = "N";

        /// <summary>
        /// Generate names depends of detectedTime 
        /// </summary>
        /// <param name="DetectedTime">DetectVolumeLevel result</param>
        /// <param name="PreferExtension">Something from FileExtensions (Xabe lib) </param>
        /// <param name="tempDir">dir for containing all splited video parts</param>
        /// <returns></returns>
        public static VideoPartsContainer GenerateNames(List<TimeLineVolume> DetectedTime, DirectoryInfo tempDir, string PreferExtension)
        {
            VideoPartsContainer partsContainer = new VideoPartsContainer(tempDir.FullName);
            long SplitedPartNumber = 0;

            foreach (var timeSpan in DetectedTime)
            {
                string VolumeLevel = timeSpan.Volume == VolumeValue.Silence ?
                    PartCodeSilence : PartCodeNoise;

                partsContainer.Container.Add(new VideoPartName(VolumeLevel, SplitedPartNumber++, PreferExtension));
            }
            return partsContainer;
        }
    }
}
