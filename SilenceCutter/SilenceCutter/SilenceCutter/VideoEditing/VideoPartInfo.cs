using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using SilenceCutter.Detecting;
namespace SilenceCutter.VideoEditing
{
    /// <summary>
    /// class for naming splitted video parts
    /// </summary>
    public class VideoPartInfo
    {
        /// <summary>
        /// separator
        /// </summary>
        public const string SEPARATOR = "_";
        /// <summary>
        /// regex pattern 
        /// 1. w+ is name 
        /// 2. w+ is Mark Silence or Noise
        /// 3. d+ is number of part 
        /// 4. w+ is extension 
        /// "_" is separator
        /// </summary>
        public readonly string PATTERN = $@"\w*{SEPARATOR}\w+{SEPARATOR}\d+\.\w+";
        
        /// <summary>
        /// silence mark 
        /// </summary>
        public const string SILENCE_MARK = "SLNCE";

        /// <summary>
        /// noise mark
        /// </summary>
        public const string NOISE_MARK = "NOISE";

        /// <summary>
        /// Directory where it placed
        /// </summary>
        public DirectoryInfo Dir { get; set; }
        /// <summary>
        /// file extension
        /// </summary>
        public string FileExtension { get; set; }

        /// <summary>
        /// is noise marks video part as noise or silence
        /// </summary>
        public bool IsNoise { get; set; }

        ///// <summary>
        ///// noise or silence mark
        ///// </summary>
        //public string Mark { get; set; }

        /// <summary>
        /// number of video part
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// Name of video part file 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// merged name from propeties of this instance except Dir
        /// </summary>
        public string FullName 
        {
            get 
            {
                string mark = IsNoise ? NOISE_MARK : SILENCE_MARK;
                return $"{Name}{SEPARATOR}{mark}{SEPARATOR}{Number}{FileExtension}";
            }
        }

        /// <summary>
        /// merged name from propeties of this instance
        /// </summary>
        public string FullNamePath
        {
            get
            {
                char osSep = Path.DirectorySeparatorChar;
                string mark = IsNoise ? NOISE_MARK : SILENCE_MARK;

                return $"{Dir.FullName}{osSep}{Name}{SEPARATOR}{mark}{SEPARATOR}{Number}{FileExtension}";
            }
        }


        /// <summary>
        /// get VideoPartInfo instance from string name
        /// </summary>
        /// <param name="name">video name</param>
        public void FromString(string name) 
        {
            if (!Regex.IsMatch(name, PATTERN))
                throw new ArgumentException($"arg name \"{name}\" is not match pattern \"{PATTERN}\"");
            
            string[] props = name.Split('_', '.');

            // point in extension
            FileExtension = $".{props[props.Length - 1]}";
            Number = int.Parse(props[props.Length - 2]);

            // detecting mark
            string mark = props[props.Length - 3];
            if (mark == NOISE_MARK)
                IsNoise = true;
            else
                IsNoise = false;
            //Mark = props[props.Length - 3];
            
            if (props.Length - 4 == 0)
                Name = props[props.Length - 4];
            else
                Name = string.Empty;
        }

        /// <summary>
        /// from string to VideoPartInfo
        /// </summary>
        /// <param name="name">video file name</param>
        /// <returns>instance</returns>
        public static VideoPartInfo FromStringToVideoPartInfo(string name) 
        {
            VideoPartInfo partInfo = new VideoPartInfo();
            partInfo.FromString(name);
            return partInfo;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>FullName prop</returns>
        public override string ToString()
        {
            return FullName;
        }
    }
}
