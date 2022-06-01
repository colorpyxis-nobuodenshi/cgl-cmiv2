using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tiff
{
    public class ImageInfomation
    {
        public string Name { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public int Depth { get; set; }

        public int Channel { get; set; }

        public string Date { get; set; }

        public string Time { get; set; }

        public string Maker { get; set; }

        public string CameraModel { get; set; }
        //-2014/04/17 zhang-start
        public int ColorSpace { get; set; }
        public int ImageType { get; set; }
        //-2014/04/17 zhang-end
        public ImageInfomation()
        {
            Width = 0;
            Height = 0;
            Depth = 0;
            Channel = 0;
            Name = string.Empty;
            Date = DateTime.Now.ToLongDateString();
            Time = DateTime.Now.ToLongTimeString();
            Maker = string.Empty;
            CameraModel = string.Empty;
            //-2014/04/17 zhang-s
            ColorSpace = 0; //0-XYZ,1-S1S2S3
            ImageType = 0; //0-normal,1-dark,2-shading,3-lighting
            //-2014/04/17 zhang-e
        }

        //public ImageInfomation(int width, int height,int depth,int channel)
        //{
        //    Width = width;
        //    Height = height;
        //    Depth = depth;
        //    Channel = channel;
        //    Name = string.Empty;
        //    Date = string.Empty;
        //    UT = string.Empty;
        //    Maker = string.Empty;
        //}
    }
}
