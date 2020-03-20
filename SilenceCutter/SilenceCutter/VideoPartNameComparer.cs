using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace SilenceCutter.VideoPartNaming
{
    class VideoPartNameComparer : IComparer<VideoPartName>
    {
        public int Compare(VideoPartName x, VideoPartName y)
        {
            return x.PartNumber.CompareTo(y.PartNumber);
        }
    }
}
