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
        /// Volume value like Silence or Noise
        /// </summary>
        public enum VolumeValue
        {
            Silence, Noise
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

        /// <summary>
        /// Time line, have properties start, end, volumeLevel, duration
        /// </summary>
        public struct TimeLineVolume
        {
            public VolumeValue Volume { get; set; }
            
            private TimeSpan start;
            public TimeSpan Start
            {
                get { return start; }
                set 
                {
                    
                    start = value; 
                }
            }
            public TimeSpan End { get; set; }
            public TimeSpan Duration 
            {
                get 
                {
                    return End - Start;
                }
            }
            
            public TimeLineVolume(VolumeValue volume, TimeSpan _start, TimeSpan _end) 
            {
                start = new TimeSpan();

                End = _end;
                Volume = volume;
                Start = _start;
            }

            public override string ToString()
            {
                return $"start [{Start}] end [{End}] duration [{Duration}] Volume [{Enum.GetName(typeof(VolumeValue), Volume)}]";
            }
        }

        /// <summary>
        /// Detect audio volume
        /// </summary>
        public class VolumeDetector
        {
            /// <summary>
            /// AudioReader fundamental property for this class
            /// </summary>
            public AudioFileReader AudioReader { get; set; }

            /// <summary>
            /// DetectedTime 
            /// </summary>
            public List<TimeLineVolume> DetectedTime { get; protected set; }

            /// <summary>
            /// Buffer's Size in method DetectVolumeLevel()
            /// </summary>
            public long BufferSize
            {
                get 
                {
                    return AudioReader.WaveFormat.SampleRate;
                }
            }
            
            public VolumeDetector(AudioFileReader audioReader)
            {
                AudioReader = audioReader;
            }
            public VolumeDetector(string filePath)
            {
                AudioReader = new AudioFileReader(filePath);
            }

            /// <summary>
            /// Detect volume, detected time will be in DetectedTime
            /// </summary>
            /// <param name="amplitudeSilenceThreshold">amplitude Threshold ( between 1 and 0 )</param>
            /// <param name="Millisec">we split all audio on millisec blocks and detect this block as silence or sound</param>
            public void DetectVolume(float amplitudeSilenceThreshold, int Millisec = 1000) 
            {
                var tmp = DetectVolumeLevel(amplitudeSilenceThreshold, Millisec);
                FormatDetectedTimeSpans(tmp);
            }
            /// <summary>
            /// Reformating DetectedTime into start-end TimeSpan list
            /// </summary>
            /// <param name="DetectedTime">result of DetectVolumeLevel()</param>
            /// <returns> squeezed list </returns>
            private void FormatDetectedTimeSpans(List<TimeSpanVolume> DetectedTime)
            {
                if (DetectedTime.Count < 1)
                    throw new ArgumentException($"Size of list DetectedTime ({DetectedTime.Count}) less than 1");

                List<TimeLineVolume> formatedList = new List<TimeLineVolume>();

                // first elem case
                TimeLineVolume tmp = new TimeLineVolume(DetectedTime[0].Volume, new TimeSpan(), new TimeSpan());

                foreach (var span in DetectedTime)
                {
                    if (tmp.Volume != span.Volume)
                    {
                        if (tmp.Duration > TimeSpan.FromMilliseconds(0))
                            formatedList.Add(tmp);
                        
                        // clear tmp and set span

                        tmp = new TimeLineVolume(span.Volume, tmp.End, tmp.End);
                        tmp.Volume = span.Volume;
                        tmp.End += span.TimeSpan;
                    }
                    else
                    {
                        tmp.End += span.TimeSpan;
                    }
                }
                if (tmp.Duration > TimeSpan.FromMilliseconds(0)) 
                {
                    tmp.End = AudioReader.TotalTime; // set to the end because sometimes it Round millisec so we got less time
                    formatedList.Add(tmp);
                }
                formatedList.TrimExcess();
                this.DetectedTime = formatedList;
            }

            /// <summary>
            /// if time span is less than the value, this time span will be marged with previous time span
            /// </summary>
            /// <param name="millisecMergeThreshold">if time span is less than the value, this time span will be marged with previous time span</param>
            private void MergeTimeLinesByThreshold(int millisecMergeThreshold) 
            {
                TimeSpan span = TimeSpan.FromMilliseconds(millisecMergeThreshold);
                for (int i = 1; i < DetectedTime.Count; i++)
                {
                    if (DetectedTime[i].Duration < span) 
                    {
                        var tmp = DetectedTime[i - 1];
                        tmp.End = DetectedTime[i].End;
                        DetectedTime[i - 1] = tmp;
                        
                    }
                }
            }

            /// <summary>
            /// detect volume level in audio file
            /// </summary>
            /// <param name="amplitudeSilenceThreshold">amplitude Threshold ( between 1 and 0 )</param>
            /// <param name="Millisec">we split all audio on millisec blocks and detect this block as silence or sound</param>
            /// <returns>
            /// List of Time duration and Volume level ( Sound or Silence )
            /// </returns>
            private List<TimeSpanVolume> DetectVolumeLevel(
                float amplitudeSilenceThreshold,
                int Millisec = 1000)
            {
                if (amplitudeSilenceThreshold > 1 || amplitudeSilenceThreshold < 0)
                    throw new ArgumentOutOfRangeException($"amplitudeSilenceThreshold ({amplitudeSilenceThreshold}) can't be more than 1 or less than 0");

                List<TimeSpanVolume> TimeSpanVolumes = new List<TimeSpanVolume>();

                // safe old position of cursor

                long oldPosition = AudioReader.Position;

                // define blockSamples by millisec

                TimeSpan timeSpan = TimeSpan.FromMilliseconds(Millisec);

                // number of Samples we ananlyze for 1 time 

                int blockSamples = MillisecToSamplesBlock(Millisec);

                // buffer
                float[] amplitudeArray = new float[BufferSize];

                // end of file

                bool eof = false;

                while (!eof)
                {

                    int ReadedSamples = AudioReader.Read(amplitudeArray, 0, amplitudeArray.Length);

                    if (ReadedSamples == 0)
                        eof = true;

                    // Samples that is not divided on blockSamples

                    int residueSamples = ReadedSamples % blockSamples;
                    ReadedSamples -= residueSamples;

                    // MAIN ANALYZE

                    for (int i = 0; i < ReadedSamples; i += blockSamples) 
                    {
                        float average = 0;

                        // one block can be not completed ( size of block not equals blockSamples )

                        int analyzedSamples = 0;
                        
                        // i + j < amplitudeArray.Length  -  out of the range
                        
                        for (int j = 0; j < blockSamples && i + j < amplitudeArray.Length; j++)
                        {
                            // amplitude can be negative

                            float sampleLocal = Math.Abs(amplitudeArray[i + j]);
                            average += sampleLocal;
                            analyzedSamples++;
                        }
                        average /= analyzedSamples;

                        // DETECT Is Silence

                        bool isSilenceResidue = average < amplitudeSilenceThreshold ? true : false;
                        VolumeValue volumeResidue = isSilenceResidue ?
                            VolumeValue.Silence :
                            VolumeValue.Noise;

                        // add timespan to list

                        TimeSpanVolume spanResidue = new TimeSpanVolume(volumeResidue, timeSpan);
                        TimeSpanVolumes.Add(spanResidue);
                    }

                    // RESIDUE ANALYZE

                    // if residue samples is not 0, that means we need to analyze it separately (last samples is not clear for dividing it on blocks)

                    float averageResidue = 0;
                    for (int i = ReadedSamples; i < ReadedSamples + residueSamples; i++) 
                    {
                        float sampleLocal = Math.Abs(amplitudeArray[i]);
                        averageResidue += sampleLocal;
                    }
                    averageResidue /= residueSamples;

                    // DETECT Is Silence

                    bool isSilence = averageResidue < amplitudeSilenceThreshold ? true : false;
                    VolumeValue volume = isSilence ?
                        VolumeValue.Silence :
                        VolumeValue.Noise;

                    // add timespan to list
                    TimeSpan ResidueTimeSpan = TimeSpan.FromMilliseconds(SamplesBlockToMillisec(residueSamples));
                    TimeSpanVolume span = new TimeSpanVolume(volume, ResidueTimeSpan);
                    TimeSpanVolumes.Add(span);
                }

                AudioReader.Position = oldPosition;
                TimeSpanVolumes.TrimExcess();
                //TimeSpanVolumes = SqueezeListOfTimeSpans(TimeSpanVolumes);
                return TimeSpanVolumes;
            }

            /// <summary>
            /// convert millisec to samples
            /// </summary>
            /// <param name="Millisec">millisec</param>
            /// <returns>block of samples</returns>
            public int MillisecToSamplesBlock(int Millisec)
            {
                int blockSample = Millisec * AudioReader.WaveFormat.SampleRate / 1000;

                // sterio have 2 chanals, if we dont multi that, amount time of audio will be 2 times more than origin

                blockSample *= AudioReader.WaveFormat.Channels;

                return blockSample;
            }


            /// <summary>
            /// convert samples to millisec
            /// </summary>
            /// <param name="SamplesBlock">number of samples</param>
            /// <returns>millisec</returns>
            public int SamplesBlockToMillisec(int SamplesBlock)
            {
                int Millisec = SamplesBlock / AudioReader.WaveFormat.SampleRate * 1000;

                // sterio have 2 chanals, if we dont multi that, amount time of audio will be 2 times more than origin

                Millisec /= AudioReader.WaveFormat.Channels;

                return Millisec;
            }

            /// <summary>
            /// Get recommended millisec for method DetectVolumeLevel
            /// </summary>
            /// <returns>
            /// millisec (less than 1000) that you should put in "DetectVolumeLevel" method
            /// </returns>
            public int GetRecommendedMillisec() 
            {
                long AmountNumOfSamples = AudioReader.Length / sizeof(float);
                long lastSamplesBlock = AmountNumOfSamples % AudioReader.WaveFormat.SampleRate;
                int millisec = 1000;
                while (true)
                {
                    int sampleBlock = MillisecToSamplesBlock(--millisec);
                    if (millisec == 0)
                        break;
                    if (lastSamplesBlock % sampleBlock == 0 && BufferSize % sampleBlock == 0)
                        return millisec;
                }
                return AudioReader.WaveFormat.Channels;
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
