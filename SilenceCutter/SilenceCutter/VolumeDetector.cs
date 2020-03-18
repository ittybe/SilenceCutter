using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NAudio.Wave;

namespace SilenceCutter 
{
    namespace Detecting
    {
        /// <summary>
        /// struct for VolumeDetector
        /// </summary>
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
                return $"{Volume,10} {TimeSpan,10}";
            }
        }

        /// <summary>
        /// Detect audio volume
        /// </summary>
        public class VolumeDetector
        {
            public AudioFileReader AudioReader { get; set; }
            //public List<TimeSpanVolume> DetectedTime { get; private set; }

            public VolumeDetector(AudioFileReader audioReader)
            {
                AudioReader = audioReader;
            }
            public VolumeDetector(string filePath)
            {
                AudioReader = new AudioFileReader(filePath);
            }

            /// <summary>
            /// squeeze list by adding time of time span with same volume
            /// </summary>
            /// <param name="DetectedTime">result of DetectVolumeLevel()</param>
            /// <returns> squeezed list </returns>
            public List<TimeSpanVolume> SqueezeListOfTimeSpans(List<TimeSpanVolume> DetectedTime)
            {
                if (DetectedTime.Count < 1)
                    throw new ArgumentException($"Size of list DetectedTime ({DetectedTime.Count}) less than 1");

                List<TimeSpanVolume> squeezed = new List<TimeSpanVolume>();

                // first elem case
                TimeSpanVolume tmp = new TimeSpanVolume(DetectedTime[0].Volume, new TimeSpan());

                foreach (var span in DetectedTime)
                {
                    if (tmp.Volume != span.Volume)
                    {
                        squeezed.Add(tmp);

                        // clear tmp and set span

                        tmp = new TimeSpanVolume();
                        tmp.Volume = span.Volume;
                        tmp.TimeSpan += span.TimeSpan;
                    }
                    else
                    {
                        tmp.TimeSpan += span.TimeSpan;
                    }
                }
                squeezed.Add(tmp);
                squeezed.TrimExcess();
                return squeezed;
            }

            /// <summary>
            /// detect volume level in audio file
            /// </summary>
            /// <param name="amplitudeSilenceThreshold">amplitude Threshold ( between 1 and 0 )</param>
            /// <param name="Millisec">we split all audio on millisec blocks and detect this block as silence or sound</param>
            /// <returns>
            /// List of Time duration and Volume level ( Sound or Silence )
            /// </returns>
            public List<TimeSpanVolume> DetectVolumeLevel(
                float amplitudeSilenceThreshold,
                int Millisec = 2)
            {
                if (amplitudeSilenceThreshold > 1 || amplitudeSilenceThreshold < 0)
                    throw new ArgumentOutOfRangeException($"amplitudeSilenceThreshold ({amplitudeSilenceThreshold}) can't be more than 1 or less than 0");

                List<TimeSpanVolume> TimeSpanVolumes = new List<TimeSpanVolume>();

                // safe old position of cursor

                long oldPosition = AudioReader.Position;

                // buffer

                float[] amplitudeArray = new float[AudioReader.WaveFormat.SampleRate * 4];   // TODO delete that

                // end of file

                bool eof = false;

                // define blockSamples by millisec

                TimeSpan timeSpan = TimeSpan.FromMilliseconds(Millisec);

                // number of Samples we ananlyze for 1 time 
                int blockSamples = MillisecToSamplesBlock(Millisec);

                while (!eof)
                {

                    int ReadedSamples = AudioReader.Read(amplitudeArray, 0, amplitudeArray.Length);

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

                        bool isSilence = average < amplitudeSilenceThreshold ? true : false;
                        TimeSpanVolume.VolumeValue volume = isSilence ?
                            TimeSpanVolume.VolumeValue.Silence :
                            TimeSpanVolume.VolumeValue.Noise;

                        // add timespan to list

                        TimeSpanVolume span = new TimeSpanVolume(volume, timeSpan);
                        TimeSpanVolumes.Add(span);
                    }
                }

                AudioReader.Position = oldPosition;
                TimeSpanVolumes.TrimExcess();
                //TimeSpanVolumes = SqueezeListOfTimeSpans(TimeSpanVolumes);
                return TimeSpanVolumes;
            }

            public int MillisecToSamplesBlock(int Millisec)
            {
                int blockSample = Millisec * AudioReader.WaveFormat.SampleRate / 1000;

                // sterio have 2 chanals, if we dont multi that, amount time of audio will be 2 times more than origin

                blockSample *= AudioReader.WaveFormat.Channels;

                return blockSample;
            }

            /// <summary>
            /// Get max amplitude ( for right choose silence threshold for method DetectSilenceLevel() )
            /// </summary>
            /// <returns>Max Amplitude</returns>
            public float GetMaxAmplitude()
            {
                // safe old position of cursor

                long oldPosition = AudioReader.Position;

                // buffer

                float[] amplitudeArray = new float[AudioReader.WaveFormat.SampleRate];

                // end of file

                bool eof = false;

                float max = 0;

                while (!eof)
                {

                    int ReadedSamples = AudioReader.Read(amplitudeArray, 0, amplitudeArray.Length);

                    if (ReadedSamples == 0)
                        eof = true;
                    for (int i = 0; i < ReadedSamples; i++)
                    {
                        max = Math.Max(amplitudeArray[i], max);
                    }
                }

                AudioReader.Position = oldPosition;
                return max;
            }


            /// <summary>
            /// Get max abs amplitude ( for right choose silence threshold for method DetectSilenceLevel() )
            /// </summary>
            /// <returns>Max Abs Amplitude</returns>
            public float GetMaxAbsAmplitude()
            {
                // safe old position of cursor

                long oldPosition = AudioReader.Position;

                // buffer

                float[] amplitudeArray = new float[AudioReader.WaveFormat.SampleRate];

                // end of file

                bool eof = false;

                float max = 0;

                while (!eof)
                {

                    int ReadedSamples = AudioReader.Read(amplitudeArray, 0, amplitudeArray.Length);

                    if (ReadedSamples == 0)
                        eof = true;
                    for (int i = 0; i < ReadedSamples; i++)
                    {
                        if (Math.Abs(amplitudeArray[i]) > Math.Abs(max))
                            max = amplitudeArray[i];
                    }
                }

                AudioReader.Position = oldPosition;
                return max;
            }

            /// <summary>
            /// Get min amplitude ( for right choose silence threshold for method DetectSilenceLevel() )
            /// </summary>
            /// <returns>Min Amplitude</returns>
            public float GetMinAmplitude()
            {
                // safe old position of cursor

                long oldPosition = AudioReader.Position;

                // buffer

                float[] amplitudeArray = new float[AudioReader.WaveFormat.SampleRate];

                // end of file

                bool eof = false;

                float min = 0;

                while (!eof)
                {
                    int ReadedSamples = AudioReader.Read(amplitudeArray, 0, amplitudeArray.Length);

                    if (ReadedSamples == 0)
                        eof = true;
                    for (int i = 0; i < ReadedSamples; i++)
                    {
                        min = Math.Min(amplitudeArray[i], min);
                    }
                }

                AudioReader.Position = oldPosition;
                return min;
            }

            /// <summary>
            /// Get min abs amplitude ( for right choose silence threshold for method DetectSilenceLevel() )
            /// </summary>
            /// <returns>Min Abs Amplitude</returns>
            public float GetMinAbsAmplitude()
            {
                // safe old position of cursor

                long oldPosition = AudioReader.Position;

                // buffer

                float[] amplitudeArray = new float[AudioReader.WaveFormat.SampleRate];

                // end of file

                bool eof = false;

                float min = 0;

                while (!eof)
                {
                    int ReadedSamples = AudioReader.Read(amplitudeArray, 0, amplitudeArray.Length);

                    if (ReadedSamples == 0)
                        eof = true;
                    for (int i = 0; i < ReadedSamples; i++)
                    {
                        if (Math.Abs(amplitudeArray[i]) < Math.Abs(min))
                            min = amplitudeArray[i];
                    }
                }

                AudioReader.Position = oldPosition;
                return min;
            }
        }
    }
}
