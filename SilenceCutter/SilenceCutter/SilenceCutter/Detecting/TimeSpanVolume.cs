using System;
using System.Collections.Generic;
using System.Text;

namespace SilenceCutter.Detecting
{
    /// <summary>
    /// Volume value like Silence or Noise
    /// </summary>
    public enum VolumeValue
    {
        /// <summary>
        /// Silence mark
        /// </summary>
        Silence,
        /// <summary>
        /// Noise mark
        /// </summary>
        Noise
    }
    /// <summary>
    /// struct for VolumeDetector
    /// </summary>
    public struct TimeSpanVolume
    {
        public VolumeValue Volume { get; set; }
        public TimeSpan TimeSpan { get; set; }
        public TimeSpanVolume(VolumeValue volume, TimeSpan timeSpan)
        {
            TimeSpan = timeSpan;
            Volume = volume;
        }
        public override string ToString()
        {
            return $"{Volume,10} {TimeSpan,10}";
        }
    }

}
