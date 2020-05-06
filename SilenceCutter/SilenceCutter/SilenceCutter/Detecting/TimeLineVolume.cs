using System;
using System.Collections.Generic;
using System.Text;

namespace SilenceCutter.Detecting
{

    /// <summary>
    /// Time line, have properties start, end, volumeLevel, duration
    /// </summary>
    public struct TimeLineVolume
    {
        public VolumeValue Volume { get; set; }

        private TimeSpan start;
        /// <summary>
        /// begin
        /// </summary>
        public TimeSpan Start
        {
            get { return start; }
            set
            {

                start = value;
            }
        }
        /// <summary>
        /// end
        /// </summary>
        public TimeSpan End { get; set; }
        /// <summary>
        /// Duration of timeline
        /// </summary>
        public TimeSpan Duration
        {
            get
            {
                return End - Start;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="volume">Volume level</param>
        /// <param name="_start">begin of time line</param>
        /// <param name="_end">end of time line</param>
        public TimeLineVolume(VolumeValue volume, TimeSpan _start, TimeSpan _end)
        {
            start = new TimeSpan();

            End = _end;
            Volume = volume;
            Start = _start;
        }
        /// <summary>
        /// to string format
        /// </summary>
        /// <returns>string format</returns>
        public override string ToString()
        {
            return $"start [{Start}] end [{End}] duration [{Duration}] Volume [{Enum.GetName(typeof(VolumeValue), Volume)}]";
        }
    }

}
