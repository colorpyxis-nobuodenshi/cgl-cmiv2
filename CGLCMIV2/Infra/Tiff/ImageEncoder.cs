using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Tiff
{
    public class ImageEncoder : IDisposable
    {
        protected Stream stream;
        protected string path;

        public ImageEncoder(string path)
        {
            Open(path);
        }

        public void Open(string path)
        {
            this.path = path;
            stream = new FileStream(path, FileMode.Create, FileAccess.Write);
        }

        public void Close()
        {
            stream.Close();
        }

        public virtual void Encode(ImageInfomation info, byte[] pixels)
        {
        }

        public void Dispose()
        {
            stream.Close();
            stream.Dispose();
        }
    }
}
