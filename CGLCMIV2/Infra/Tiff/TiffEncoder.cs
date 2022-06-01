﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Tiff
{
    public class TiffEncoder : ImageEncoder
    {
        private ushort byteOrder = 0x4949;
        private const int HEADER_SIZE = 8;

        public TiffEncoder(string path)
            : base(path)
        {

        }

        public override void Encode(ImageInfomation info, byte[] pixels)
        {
            var bw = new BinaryWriter(stream, System.Text.Encoding.ASCII);
            WriteHeader(bw);
            WriteIFD(bw, info);
            WritePixels(bw, pixels);
        }

        private void WriteHeader(BinaryWriter bw)
        {
            bw.Write(byteOrder);
            bw.Write((ushort)42);
            bw.Write(8);
        }

        private void WriteIFD(BinaryWriter bw, ImageInfomation info)
        {
            //var tagCount = 14;
            var tagCount = 16;
            var ifdSize = 2 + tagCount * 12 + 4;
            var tagDataOffset = HEADER_SIZE + ifdSize;
            var imageSize = info.Width * info.Height * info.Channel * info.Depth / 8;
            var width = info.Width;
            var height = info.Height;
            var depth = info.Depth;
            var channel = info.Channel;
            var name = info.Name;
            var maker = info.Maker;
            var model = info.CameraModel;
            var datetime = string.Format("{0} {1}", info.Date, info.Time);
            //-2014/04/17 zhang-start
            var colorSpace = info.ColorSpace;
            var imageType = info.ImageType;
            //--2014/04/17 zhang-end
            var imageOffset = HEADER_SIZE + ifdSize + name.Length + datetime.Length + maker.Length + model.Length;


            if (channel == 3)
                imageOffset += 6;

            //
            bw.Write((ushort)tagCount);

            //
            //WriteEntry(bw, TagCode.SubfileType, TagType.Long, 1, 0);
            WriteEntry(bw, TagCode.ImageWidth, TagType.Long, 1, (uint)width);
            WriteEntry(bw, TagCode.ImageLength, TagType.Long, 1, (uint)height);
            if (info.Channel == 1)
            {
                WriteEntry(bw, TagCode.BitsPerSample, TagType.Short, 1, (ushort)depth);
                WriteEntry(bw, TagCode.SamplesPerPixel, TagType.Short, 1, 1);
                WriteEntry(bw, TagCode.PhotometricInterpretation, TagType.Long, 1, (uint)PhotometricInterpretation.BlackIsZero);

            }
            else
            {
                WriteEntry(bw, TagCode.BitsPerSample, TagType.Short, 3, (uint)tagDataOffset);
                tagDataOffset += 6;
                WriteEntry(bw, TagCode.SamplesPerPixel, TagType.Short, 1, 3);
                WriteEntry(bw, TagCode.PhotometricInterpretation, TagType.Long, 1, (uint)PhotometricInterpretation.RGB);
            }
            WriteEntry(bw, TagCode.PlanarConfiguration, TagType.Short, 1, 1);
            WriteEntry(bw, TagCode.Compression, TagType.Long, 1, (uint)Compression.Uncompressed);
            WriteEntry(bw, TagCode.StripOffsets, TagType.Long, 1, (uint)imageOffset);
            WriteEntry(bw, TagCode.RowsPerStrip, TagType.Long, 1, (uint)info.Width*3);
            WriteEntry(bw, TagCode.StripByteCounts, TagType.Long, 1, (uint)imageSize);
            WriteEntry(bw, TagCode.DocumentName, TagType.Ascii, (uint)name.Length, (uint)tagDataOffset);
            tagDataOffset += name.Length;
            WriteEntry(bw, TagCode.DateTime, TagType.Ascii, (uint)datetime.Length, (uint)tagDataOffset);
            tagDataOffset += datetime.Length;
            WriteEntry(bw, TagCode.Make, TagType.Ascii, (uint)maker.Length, (uint)tagDataOffset);
            tagDataOffset += maker.Length;
            WriteEntry(bw, TagCode.Model, TagType.Ascii, (uint)model.Length, (uint)tagDataOffset);
            tagDataOffset += model.Length;
            //--2014/04/17 zhang-s
            WriteEntry(bw, TagCode.ImageType, TagType.Long, 1, (uint)info.ImageType);
            WriteEntry(bw, TagCode.ColorSpace, TagType.Long, 1, (uint)info.ColorSpace);
            //--2014/04/17 zhang-e

            //
            bw.Write(0);

            //
            if (info.Channel == 3)
            {
                bw.Write(BitConverter.GetBytes((ushort)depth));
                bw.Write(BitConverter.GetBytes((ushort)depth));
                bw.Write(BitConverter.GetBytes((ushort)depth));
            }
            bw.Write(name.ToCharArray());
            bw.Write(datetime.ToCharArray());
            bw.Write(maker.ToCharArray());
            bw.Write(model.ToCharArray());
        }

        private void WriteEntry(BinaryWriter bw, TagCode tag, TagType type, uint count, uint value)
        {
            bw.Write((ushort)tag);
            bw.Write((ushort)type);
            bw.Write(count);
            bw.Write(value);
        }

        private void WritePixels(BinaryWriter bw, byte[] pixels)
        {
            bw.Write(pixels, 0, pixels.Length);
        }
    }
}
