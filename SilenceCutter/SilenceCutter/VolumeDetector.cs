using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NAudio.Wave;

namespace SilenceCutter 
{
    namespace Detecting
    {
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
                return $"start [{Start}] end [{End}] Volume [{Enum.GetName(typeof(VolumeValue), Volume)}]";
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
            /// format list of detected timespans by convert time duration to timelines
            /// </summary>
            /// <param name="DetectedTime">result of DetectVolumeLevel()</param>
            /// <returns> squeezed list </returns>
            public List<TimeLineVolume> FormatDetectedTimeSpans(List<TimeSpanVolume> DetectedTime)
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
                formatedList.Add(tmp);
                formatedList.TrimExcess();
                return formatedList;
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

                // define blockSamples by millisec

                TimeSpan timeSpan = TimeSpan.FromMilliseconds(Millisec);

                // number of Samples we ananlyze for 1 time 

                int blockSamples = MillisecToSamplesBlock(Millisec);

                //// buffer
                //int SizeBuffer = blockSamples * (AudioReader.WaveFormat.SampleRate / blockSamples);
                //AudioReader.WaveFormat.SampleRate * 4
                float[] amplitudeArray = new float[blockSamples];

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

                    // residue analyze

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

            public int MillisecToSamplesBlock(int Millisec)
            {
                int blockSample = Millisec * AudioReader.WaveFormat.SampleRate / 1000;

                // sterio have 2 chanals, if we dont multi that, amount time of audio will be 2 times more than origin

                blockSample *= AudioReader.WaveFormat.Channels;

                return blockSample;
            }

            public int SamplesBlockToMillisec(int SamplesBlock)
            {
                int Millisec = SamplesBlock / AudioReader.WaveFormat.SampleRate * 1000;

                // sterio have 2 chanals, if we dont multi that, amount time of audio will be 2 times more than origin

                Millisec /= AudioReader.WaveFormat.Channels;

                return Millisec;
            }
            /// <summary>
            /// you can get recommended millisec value for method DetectVolumeLevel
            /// (we need recommended millisec because audio file does not have const number of samples (last block of samples can be less than sample rate))
            /// </summary>
            /// <param name="LengthOfList">Size of list</param>
            /// <returns>List of recommended millisec (less than 1 second), sometimes list can be empty, then just use 2 millisec</returns>
            public List<int> GetListOfRecommendedMillisec(int LengthOfList) 
            {
                List<int> recommendedMillisec = new List<int>();
                long AmountNumOfSamples = AudioReader.Length / sizeof(float);
                long lastSamplesBlock = AmountNumOfSamples % AudioReader.WaveFormat.SampleRate;
                int millisec = 1;
                while (recommendedMillisec.Count < LengthOfList) 
                {
                    int sampleBlock = MillisecToSamplesBlock(millisec);
                    if (lastSamplesBlock % sampleBlock == 0 && AudioReader.WaveFormat.SampleRate % sampleBlock == 0)
                        recommendedMillisec.Add(millisec);
                    if (millisec > 1000)
                        break;
                    millisec++;
                }
                return recommendedMillisec;
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
