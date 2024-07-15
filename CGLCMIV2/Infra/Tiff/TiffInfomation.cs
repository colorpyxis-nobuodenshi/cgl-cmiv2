using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tiff
{
    public class TiffInfomation
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
        public int PlanarConfig { get; set; }
        public int SampleFormat { get; set; }
        
        public TiffInfomation()
        {
            Width = 0;
            Height = 0;
            Depth = 16;
            Channel = 1;
            Name = string.Empty;
            Date = DateTime.Now.ToLongDateString();
            Time = DateTime.Now.ToLongTimeString();
            Maker = string.Empty;
            CameraModel = string.Empty;
            SampleFormat = 1;
            PlanarConfig = 1;
        }

    }
}
