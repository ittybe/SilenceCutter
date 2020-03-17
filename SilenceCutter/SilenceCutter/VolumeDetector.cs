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
        /// <param name="audioReader">File Reader</param>
        /// <param name="silenceThreshold">RMS value</param>
        /// <param name="blockSize">size of each block, to calc RMS</param>
        /// <returns>
        /// List of Time duration and Volume level ( Sound or Silence )
        /// </returns>
        public static List<TimeSpanVolume> DetectVolumeLevel(
            this AudioFileReader audioFileReader, 
            float silenceThreshold, 
            int blockSize = 500)
        {
            List<TimeSpanVolume> TimeSpanVolumes = new List<TimeSpanVolume>();
            
            // safe old position of cursor
            
            long oldPosition = audioFileReader.Position;

            // calc duration of blockSize in milisec
            
            double MilisecPerBlock = (double)blockSize / audioFileReader.WaveFormat.SampleRate*1000;

            bool eof = false;
            float[] ReadBuffer = new float[audioFileReader.WaveFormat.SampleRate * sizeof(float)];
            while (!eof)
            {
                int samplesRead = audioFileReader.Read(ReadBuffer, 0, ReadBuffer.Length);
                if (samplesRead == 0)
                {
                    eof = true;
                    break;
                }
                for (int x = 0; x < samplesRead; x += blockSize)
                {
                    // calc rms 

                    double total = 0.0;
                    for (int y = 0; y < blockSize && x + y < samplesRead; y++)
                    {
                        total += ReadBuffer[x + y] * ReadBuffer[x + y];
                    }
                    var rms = (float)Math.Sqrt(total / blockSize);
                    
                    // compare rms with threshold and define it as silence( or sound ) part

                    bool IsSilence = rms < silenceThreshold ? true : false;
                    
                    TimeSpanVolume.VolumeValue volumeValue = IsSilence ?
                        TimeSpanVolume.VolumeValue.Silence :
                        TimeSpanVolume.VolumeValue.Noise;

                    TimeSpan timeSpan = TimeSpan.FromMilliseconds(MilisecPerBlock);

                    TimeSpanVolume timeSpanVolume = new TimeSpanVolume(volumeValue, timeSpan);
                    TimeSpanVolumes.Add(timeSpanVolume);
                }
            }
            audioFileReader.Position = oldPosition;
            TimeSpanVolumes.TrimExcess();
            //TimeSpanVolumes = SqueezeListOfTimeSpans(TimeSpanVolumes);
            return TimeSpanVolumes;
        }
    }
}
