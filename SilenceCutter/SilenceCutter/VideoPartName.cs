using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SilenceCutter.Detecting;
namespace SilenceCutter.VideoPartNaming
{
    /// <summary>
    /// struct for Naming video parts 
    /// </summary>
    public struct VideoPartName
    {
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
                return $"{PartCode}{PartNumber}{FileExten}";
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
        //public static implicit operator VideoPartName(string str)
        //{

        //}
    }
}




