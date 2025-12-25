using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Reflection;

namespace Tiff
{
    

    public class ImageDecoder : IDisposable
    {
        protected Stream stream;
        protected string path;

        public ImageDecoder(string path)
        {
            Open(path);
        }

        public void Open(string path)
        {
            this.path = path;
            stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        }

        public void Close()
        {
            stream.Close();
        }

        public virtual void Decode(out ImageInfomation info, out byte[] pixels)
        {
            info = new ImageInfomation();
            pixels = new byte[1];
        }

        public virtual (ImageInfomation info, byte[] pixels)? Decode()
        {
            return null;
        }

        public void Dispose()
        {
            stream.Close();
            stream.Dispose();
        }
    }

    

    
}
