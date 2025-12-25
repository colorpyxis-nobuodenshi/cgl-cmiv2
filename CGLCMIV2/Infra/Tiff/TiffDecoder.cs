using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Tiff
{

    public class TiffDecoder : IDisposable
    {
        ByteOrder _endian = ByteOrder.LittleEndian;
        Stream _stream;
        string _path;
        public TiffDecoder(string path)
        {

            Open(path);
        }
        public void Open(string path)
        {
            _path = path;
            _stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        }

        public void Close()
        {
            _stream.Close();
        }
        public void Decode(out TiffInfomation info, out byte[] pixels)
        {
            var br = new BinaryReader(_stream, System.Text.Encoding.ASCII);
            info = new TiffInfomation();
            
            ReadHeader(br);
            
            ReadIFD(br, ref info);

            pixels = new byte[info.Width * info.Height * info.Channel * info.Depth / 8];
            if (info.Depth == 16)
            {
                ReadPixels16(br, ref pixels);
            }
            else if (info.Depth == 32)
            {
                ReadPixels32(br, ref pixels);
            }
        }

        public (TiffInfomation info, byte[] pixels) Decode()
        {
            var br = new BinaryReader(_stream, System.Text.Encoding.ASCII);
            var info = new TiffInfomation();

            ReadHeader(br);

            ReadIFD(br, ref info);

            var pixels = new byte[info.Width * info.Height * info.Channel * info.Depth / 8];
            if (info.Depth == 16)
            {
                ReadPixels16(br, ref pixels);
            }
            else if (info.Depth == 32)
            {
                ReadPixels32(br, ref pixels);
            }

            return (info, pixels);
        }

        private void ReadHeader(BinaryReader br)
        {
            var b = new byte[2];
            br.Read(b, 0, 2);
            if (b[0] == 0x49 && b[1] == 0x49)
            {
                _endian = ByteOrder.LittleEndian;
            }
            else
            {
                _endian = ByteOrder.BigEndian;
            }
            b = new byte[2];
            br.Read(b, 0, 2);
            b = new byte[4];
            br.Read(b, 0, 4);
            var ifdOffset = BitConverterEx.ToInt32(b, 0, _endian);
            _stream.Seek(ifdOffset, SeekOrigin.Begin);
        }

        private void ReadIFD(BinaryReader br, ref TiffInfomation info)
        {
            var imageOffset = 0;

            var b = new byte[2];
            br.Read(b, 0, 2);
            var tagCount = (int)BitConverterEx.ToUInt16(b, 0, _endian);
            for (var i = 0; i < tagCount; i++)
            {
                b = new byte[2];
                br.Read(b, 0, 2);
                var tag = (TagCode)BitConverterEx.ToUInt16(b, 0, _endian);
                b = new byte[2];
                br.Read(b, 0, 2);
                var type = (TagType)BitConverterEx.ToUInt16(b, 0, _endian);
                b = new byte[4];
                br.Read(b, 0, 4);
                var count = BitConverterEx.ToUInt32(b, 0, _endian);
                var value = new byte[4];
                br.Read(value, 0, 4);
                //var value = BitConverterEx.ToUInt32(b, 0, endian);

                switch(tag)
                {
                    case TagCode.ImageWidth:
                        info.Width = (int)BitConverterEx.ToUInt32( value,0,_endian);
                        break;
                    case TagCode.ImageLength:
                        info.Height = (int)BitConverterEx.ToUInt32(value, 0, _endian);
                        break;
                    case TagCode.BitsPerSample:
                        if (count == 1)
                        {
                            info.Channel = 1;
                            info.Depth = (int)BitConverterEx.ToUInt16(value, 0, _endian);
                        }
                        else if(count == 3)
                        {
                            info.Channel = 3;
                            var currentPos = _stream.Position;
                            var offset = (int)BitConverterEx.ToUInt32(value, 0, _endian);
                            _stream.Seek(offset, SeekOrigin.Begin);
                            var v = new byte[2];
                            br.Read(v, 0, 2);
                            info.Depth = (int)BitConverterEx.ToUInt16(v, 0, _endian);
                            _stream.Seek(currentPos, SeekOrigin.Begin);
                        }
                        break;
                    case TagCode.StripOffsets:
                        imageOffset = (int)BitConverterEx.ToUInt32(value, 0, _endian);
                        break;

                }
            }
            b = new byte[4];
            br.Read(b, 0, 4);
            var nextIFDOffset = BitConverterEx.ToUInt32(b, 0, _endian);
            _stream.Seek(imageOffset, SeekOrigin.Begin);
        }

        private void ReadPixels16(BinaryReader br, ref byte[] p)
        {
            if (_endian == ByteOrder.LittleEndian)
            {
                br.Read(p, 0, p.Length);
                return;
            }
            var b = new byte[2];
            var size = p.Length / 2;
            unsafe
            {
                fixed (byte* pp = p)
                {
                    var ppp = (ushort*)pp;
                    for (var i = 0; i < size; i++)
                    {
                        br.Read(b, 0, b.Length);
                        ppp[i] = BitConverterEx.ToUInt16(b, 0, _endian);
                    }
                }
            }   
        }
        private void ReadPixels32(BinaryReader br, ref byte[] p)
        {
            if (_endian == ByteOrder.LittleEndian)
            {
                br.Read(p, 0, p.Length);
                return;
            }

            var b = new byte[4];
            var size = p.Length / 4;
            unsafe
            {
                fixed (byte* pp = p)
                {
                    var ppp = (float*)pp;
                    for (var i = 0; i < size; i++)
                    {
                        br.Read(b, 0, b.Length);
                        ppp[i] = BitConverterEx.ToUInt16(b, 0, _endian);
                    }
                }
            }
        }

        public void Dispose()
        {
            _stream.Close();
            _stream.Dispose();
        }
    }
}
