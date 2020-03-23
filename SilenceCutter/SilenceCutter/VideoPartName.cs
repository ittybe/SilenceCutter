using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SilenceCutter.Detecting;
using System.Text.RegularExpressions;

namespace SilenceCutter.VideoPartNaming
{
    /// <summary>
    /// struct for Naming video parts 
    /// </summary>
    public struct VideoPartName
    {
        /// <summary>
        /// RegularExpression for video part name
        /// </summary>
        public const string Pattern = @"(\w+)_([0-9]+)\.[0-9A-Za-z]+";
        /// <summary>
        /// Part code in char
        /// </summary>
        public string PartCode { get; set; }

        /// <summary>
        /// number of video
        /// </summary>
        public long PartNumber { get; set; }
        /// <summary>
        /// Extension of the video part
        /// </summary>
        public string FileExten { get; set; }

        /// <summary>
        /// Full name in str format
        /// </summary>
        public string FullName 
        {
            get 
            {
                string fullname = $"{PartCode}_{PartNumber}{FileExten}";
                if (Regex.IsMatch(fullname, Pattern)) 
                {
                    return fullname;
                }
                throw new ApplicationException($"full name {fullname} is not match this patter {Pattern}");
            }
        }
       
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="code">code</param>
        /// <param name="number">number</param>
        /// <param name="fileExtension">file extension</param>
        public VideoPartName(string code, long number, string fileExtension) 
        {
            PartCode = code;
            PartNumber = number;
            FileExten = fileExtension;
        }

        public override string ToString()
        {
            return FullName;
        }

        //public static extern VideoPartName(string str)
        //{

        //}
        /// <summary>
        /// convert string to video part name
        /// </summary>
        /// <param name="str">input string</param>
        public static explicit operator VideoPartName(string str)
        {
            if (Regex.IsMatch(str, Pattern))
            {
                // split str on parts with extension, code part and number 

                string[] nameParts = str.Split('.', '_');
                
                VideoPartName result = new VideoPartName(nameParts[0], Convert.ToInt64(nameParts[1]), '.' + nameParts[2]);
                return result;
            }
            else 
            {
                throw new ApplicationException($"input str {str} is not match this patter {Pattern}");
            }
        }
    }
}




