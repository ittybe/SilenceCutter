using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;
using NAudio;
using System.Drawing;

namespace Detecting
{
    public struct TimeSpanVolume 
    {
        public enum VolumeValue
        {
            Silence, Noise
        }
        public VolumeValue Volume { get; set; }
        public TimeSpan TimeSpan { get; set; }
        public TimeSpanVolume(VolumeValue volume, TimeSpan timeSpan) 
        {
            TimeSpan = timeSpan;
            Volume = volume;
        }
        public override string ToString()
        {
            return $"{Volume} {TimeSpan}";
        }
    }
    public static class VolumeDetector
    {
        private static bool IsSilence(float amplitude, sbyte threshold)
        {
            double dB = 20*Math.Log10(Math.Abs(amplitude));
            return dB < threshold;
        }
        private static List<TimeSpanVolume> SqueezeListOfTimeSpans(List<TimeSpanVolume> timeSpanVolumes) 
        {
            TimeSpanVolume tmp = new TimeSpanVolume();
            List<TimeSpanVolume> squeezed = new List<TimeSpanVolume>();
            for (int i = 0; i < timeSpanVolumes.Count-1; i++) 
            {
                tmp.Volume = timeSpanVolumes[i].Volume;
                tmp.TimeSpan = tmp.TimeSpan.Add(timeSpanVolumes[i].TimeSpan);
                if (timeSpanVolumes[i+1].Volume != tmp.Volume) 
                {
                    squeezed.Add(tmp);
                    tmp.TimeSpan = new TimeSpan();
                }
            }
            squeezed.TrimExcess();
            return squeezed;
        }
        /// <summary>
        /// detect volume level in audio file
        /// </summary>
        /// <param name="audioFileReader">File Reader</param>
        /// <param name="amplitudeSilenceThreshold">amplitude Threshold ( between 1 and 0 )</param>
        /// <param name="blockSamples">block of samples, we calc the averange value of block and compare it with amplitudeSilenceThreshold</param>
        /// <returns>
        /// List of Time duration and Volume level ( Sound or Silence )
        /// </returns>
        public static List<TimeSpanVolume> DetectVolumeLevel(
            this AudioFileReader audioFileReader, 
            float amplitudeSilenceThreshold, 
            int blockSamples = 500)
        {
            if (amplitudeSilenceThreshold > 1 || amplitudeSilenceThreshold < 0)
                throw new ArgumentOutOfRangeException($"amplitudeSilenceThreshold ({amplitudeSilenceThreshold}) can't be more than 1 or less than 0");
            List<TimeSpanVolume> TimeSpanVolumes = new List<TimeSpanVolume>();
            
            // safe old position of cursor
            
            long oldPosition = audioFileReader.Position;

            // buffer

            float[] amplitudeArray = new float[audioFileReader.WaveFormat.SampleRate];

            // end of file
            
            bool eof = false;

            // define duration of blockSamples

            double MilisecPerSamples = blockSamples / audioFileReader.WaveFormat.SampleRate * 1000;
            TimeSpan timeSpan = TimeSpan.FromMilliseconds(MilisecPerSamples);

            while (!eof) 
            {
                
                int ReadedSamples = audioFileReader.Read(amplitudeArray, 0, amplitudeArray.Length);

                if (ReadedSamples == 0)
                    eof = true;

                for (int i = 0; i < ReadedSamples; i += blockSamples) 
                {
                    float average = 0;
                    
                    // i + j < amplitudeArray.Length  -  out of the range
                    
                    for (int j = 0; j < blockSamples && i + j < amplitudeArray.Length; j++) 
                    {
                        // amplitude can be negative

                        float sampleLocal = Math.Abs(amplitudeArray[i + j]);
                        average += sampleLocal;
                    }
                    average /= blockSamples;

                    // DETECT Is Silence

                    bool isSilence = average > amplitudeSilenceThreshold ? true : false;
                    TimeSpanVolume.VolumeValue volume = isSilence ?
                        TimeSpanVolume.VolumeValue.Silence :
                        TimeSpanVolume.VolumeValue.Noise;

                    // convert Samples into milisec

                    TimeSpanVolume span = new TimeSpanVolume(volume, timeSpan);
                    TimeSpanVolumes.Add(span);
                }
            }
            
            audioFileReader.Position = oldPosition;
            TimeSpanVolumes.TrimExcess();
            //TimeSpanVolumes = SqueezeListOfTimeSpans(TimeSpanVolumes);
            return TimeSpanVolumes;
        }
    }
}
