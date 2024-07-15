using CGLCMIV2.Application;
using CGLCMIV2.Domain;
using OpenCvSharp;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tiff;

namespace CGLCMIV2.Infrastructure
{
    public class Logger : Application.ILogger
    {
        public Logger()
        {
            Log.Logger = new LoggerConfiguration()
            .WriteTo.File(@"log\app.log", rollOnFileSizeLimit: true, fileSizeLimitBytes: 104857600, retainedFileCountLimit: 5)
            .CreateLogger();
        }
        ~Logger()
        {
            Log.CloseAndFlush();
        }
        public void Debug(string message)
        {
            Log.Logger.Debug(message);
        }

        public void Fatal(string message)
        {
            Log.Logger.Fatal(message);
        }

        public void Infomation(string message)
        {
            Log.Logger.Information(message);
        }

        public void Warning(string message)
        {
            Log.Logger.Warning(message);
        }
        public void Error(Exception ex, string message)
        {
            Log.Logger.Error(ex, message);
        }
    }

    public class ErrorLogger : IErrorLogger
    {
        public ErrorLogger()
        {
            Log.Logger = new LoggerConfiguration()
            .WriteTo.File("error.log.txt", rollOnFileSizeLimit: true)
            .CreateLogger();
        }
        ~ErrorLogger()
        {
            Log.CloseAndFlush();
        }

        public void Error(Exception ex, string message)
        {
            Log.Logger.Error(ex, message);
        }
    }
    public class MeasureResultWriter : IMeasureResultWriter
    {
        //string _dirPath = string.Empty;
        //string _fileName = string.Empty;
        ////string _fullPath = string.Empty;
        //string _pixPath = string.Empty;
        //IPixelsTiffFileStore _tiffStore;
        //public MeasureResultWriter(IPixelsTiffFileStore tiffStore)
        //{
        //    //_tiffStore = tiffStore;

        //    //CreateOptputDirectory();
        //}
        public MeasureResultWriter()
        {

        }
        public void Write(string dir, string title, string message)
        {
            //if(!Directory.Exists(dir))
            //{
            //    Directory.CreateDirectory(dir);
            //}

            var fileName = Path.Combine(dir, title);
            using(var sw = new StreamWriter(new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None)))
            {
                sw.WriteLine(message);
            }

        }

        public void Write(string message)
        {
            using (var sw = new StreamWriter(new FileStream(@"log\CMI.log", FileMode.Append, FileAccess.Write, FileShare.None)))
            {
                sw.WriteLine(message);
            }
        }
        //public void Write(string title, IList<XYZPixels> measureResultPixels)
        //{
        //    var p = _dirPath + _pixPath;
        //    if(!Directory.Exists(p))
        //    {
        //        Directory.CreateDirectory(p);
        //    }
        //    for(var i=0;i<measureResultPixels.Count;i++)
        //    {
        //        _tiffStore.ExecuteAsync(p + $"\\{title}_{i + 1}.tif", measureResultPixels[i]);
        //    }
        //}
        //public void Write(string title, XYZPixels measureResultPixels)
        //{
        //    var p = _dirPath + _pixPath;
        //    if (!Directory.Exists(p))
        //    {
        //        Directory.CreateDirectory(p);
        //    }
        //    _tiffStore.ExecuteAsync(p + $"\\{title}.tif", measureResultPixels);
        //}

        //public void CreateOptputDirectoryAndHeader(string headerMessage)
        //{
        //    var now = DateTime.Now.ToString("yyyyMMdd");
        //    _dirPath = @"mes\";
        //    _fileName = "MES" + now + ".txt";
        //    _pixPath = "MES" + now + "PIX";

        //    var fullPath = _dirPath + _fileName;

        //    if (!Directory.Exists(_dirPath))
        //    {
        //        Directory.CreateDirectory(_dirPath);
        //    }
        //    if (!File.Exists(fullPath))
        //    {
        //        File.Create(fullPath).Close();
        //        Write(headerMessage);
        //    }

        //    //if (!Directory.Exists(_path + _imagePath))
        //    //{
        //    //    Directory.CreateDirectory(_path + _imagePath);
        //    //}

        //}
    }
    public class WhitepointWriter : IWhitepointWriter
    {
        //public void Write(CIEXYZ whitepoint, double temperature, double opticalpower, string processingDatetime, string systemSerialNumber)
        //{
        //    var message = $"whitepoint={whitepoint},temperature1={temperature:F2},temperature2={opticalpower:F2},date={processingDatetime},systemserialnumber={systemSerialNumber}";
        //    var filename = @"log\WHITEPOINT.log";
        //    using (var sw = new StreamWriter(filename, true))
        //    {
        //        sw.WriteLine(message);
        //    }
        //}
        public void Write(string message)
        {
            var filename = @"log\ptfelog.log";
            using (var sw = new StreamWriter(filename, true))
            {
                sw.WriteLine(message);
            }
        }
    }
    public class PixelsTiffFileStore : IPixelsTiffFileStore
    {
        public PixelsTiffFileStore()
        {

        }
        public void Execute(string path, XYZPixels obj)
        {
            var w = obj.Width;
            var h = obj.Height;
            var pixels = obj.Pix;
            var pixels2 = new byte[w * h * 3 * 2];
            var size = w * h;
            var stride = w * 3;

            unsafe
            {
                fixed (ushort* src = pixels)
                fixed (byte* dst = pixels2)
                {
                    var srcPtr = src;
                    var dstPtr = (ushort*)dst;
                    var srcPtr1 = srcPtr;
                    var srcPtr2 = srcPtr + size;
                    var srcPtr3 = srcPtr + size * 2;

                    for (var i = 0; i < size; i++)
                    {
                        *(dstPtr + 0) = *(srcPtr1);
                        *(dstPtr + 1) = *(srcPtr2);
                        *(dstPtr + 2) = *(srcPtr3);
                        srcPtr1++;
                        srcPtr2++;
                        srcPtr3++;
                        dstPtr += 3;
                    }
                }
            }

            var tiff = new Tiff.TiffEncoder(path);
            var info = new Tiff.TiffInfomation() { Width = w, Height = h, Channel = 3, Depth = 16, PlanarConfig = 1 };

            tiff.Encode(info, pixels2);
        }

        public void Execute(string path, ShadingCorrectPixels obj)
        {
            var w = obj.Width;
            var h = obj.Height;
            var pixels = obj.Pix;
            var pixels2 = new byte[w * h * 3 * 4];
            var size = w * h;
            var stride = w * 3;

            unsafe
            {
                fixed (float* src = pixels)
                fixed (byte* dst = pixels2)
                {
                    var srcPtr = src;
                    var dstPtr = (float*)dst;
                    var srcPtr1 = srcPtr;
                    var srcPtr2 = srcPtr + size;
                    var srcPtr3 = srcPtr + size * 2;

                    for (var i = 0; i < size; i++)
                    {
                        *(dstPtr + 0) = *(srcPtr1);
                        *(dstPtr + 1) = *(srcPtr2);
                        *(dstPtr + 2) = *(srcPtr3);
                        srcPtr1++;
                        srcPtr2++;
                        srcPtr3++;
                        dstPtr += 3;
                    }
                }
            }

            var tiff = new Tiff.TiffEncoder(path);
            var info = new Tiff.TiffInfomation() { Width = w, Height = h, Channel = 3, Depth = 32, PlanarConfig = 1, SampleFormat = 3 };

            tiff.Encode(info, pixels2);
        }

        public async Task ExecuteAsync(string path, XYZPixels obj)
        {
            await Task.Run(() => 
            {
                Execute(path, obj);
            });
        }

        public async Task ExecuteAsync(string path, ShadingCorrectPixels obj)
        {
            await Task.Run(() =>
            {
                Execute(path, obj);
            });
        }
    }
    public class PixelsTiffFileLoader : IPixelsTiffFileLoader
    {
        public ShadingCorrectPixels Execte(string path)
        {
            var tiff = new TiffDecoder(path);
            var res = tiff.Decode();

            var pix = new float[res.info.Width * res.info.Height * 3];
            var pix2 = new float[res.info.Width * res.info.Height * 3];
            for (var i = 0; i < pix.Length; i++)
            {
                pix[i] = BitConverter.ToSingle(res.pixels, i * 4);
            }
            var size = res.info.Width * res.info.Height;

            unsafe
            {
                fixed (float* src = pix)
                fixed (float* dst = pix2)
                {
                    var srcPtr = src;
                    var dstPtr = dst;
                    var dstPtr1 = dstPtr;
                    var dstPtr2 = dstPtr + size;
                    var dstPtr3 = dstPtr + size * 2;

                    for (var i = 0; i < size; i++)
                    {
                        *dstPtr1 = *(srcPtr + 0);
                        *dstPtr2 = *(srcPtr + 1);
                        *dstPtr3 = *(srcPtr + 2);
                        dstPtr1++;
                        dstPtr2++;
                        dstPtr3++;
                        srcPtr += 3;
                    }
                }
            }

            return new ShadingCorrectPixels(pix, res.info.Width, res.info.Height);
        }

        public async Task<ShadingCorrectPixels> ExecuteAsync(string path)
        {
            return await Task.Run(() =>
            {
                return Execte(path);
            });
        }
    }
    //public class ShadingPixelsFileStore : IShadingPixelsFileStore
    //{
    //    public void Execte(string path, ShadingCorrectPixels obj)
    //    {
    //        if(File.Exists("shd.raw"))
    //        {
    //            File.Copy("shd.raw", "shd.1.raw", true);
    //        }
    //        using (var sw = new System.IO.BinaryWriter(System.IO.File.Open(path, System.IO.FileMode.Create)))
    //        {
    //            foreach (var v in obj.Pix)
    //            {
    //                sw.Write(v);
    //            }
    //        }
    //    }
    //}

    //public class ShadingPixelsFileLader : IShadingPixelsFileLoader
    //{
    //    public ShadingPixelsFileLader()
    //    {

    //    }

    //    public ShadingCorrectPixels Execte(string path)
    //    {
    //        var w = 1024;
    //        var h = 768;
    //        var len = w * h * 3;
    //        var byteSize = len * 4;
    //        var pixels = new byte[byteSize];
    //        var pixels2 = new float[len];


    //        using (var sr = new System.IO.BinaryReader(System.IO.File.Open(path, System.IO.FileMode.Open)))
    //        {
    //            sr.Read(pixels, 0, byteSize);
    //        }

    //        unsafe
    //        {
    //            fixed (byte* src = pixels)
    //            fixed (float* dst = pixels2)
    //            {
    //                var srcPtr = src;
    //                var dstPtr = dst;
    //                for (var i = 0; i < len; i++)
    //                {
    //                    //var v1 = (byte)(*srcPtr >> 8);
    //                    //var v2 = (byte)(*srcPtr & 0xFF);

    //                    var b = new byte[] { *srcPtr, *(srcPtr + 1), *(srcPtr + 2), *(srcPtr + 3) };
    //                    var val = BitConverter.ToSingle(b, 0);
    //                    *dstPtr = val;

    //                    srcPtr += 4;
    //                    dstPtr++;
    //                }
    //            }
    //        }

    //        return new ShadingCorrectPixels(pixels2, w, h);
    //    }
    //}
    //public class PixelsFileStore : IPixelsFileStore<ushort>
    //{
    //    public void Execute(string path, Pixels<ushort> obj)
    //    {
    //        var dirname = Path.GetDirectoryName(path);
    //        if (!Directory.Exists(dirname))
    //        {
    //            Directory.CreateDirectory(dirname);
    //        }
    //        using (var sw = new System.IO.BinaryWriter(System.IO.File.Open(path, System.IO.FileMode.Create)))
    //        {
    //            foreach (var v in obj.Pix)
    //            {
    //                sw.Write(v);
    //            }
    //        }
    //    }
    //}

    public class ColorGradingConditonFileLoader : IColorGradingConditonFileLoader
    {
        public ColorGradingConditonFileLoader()
        {

        }
        public ColorGradingCondition Execute(string path)
        {
            var lines = System.IO.File.ReadAllLines(path);
            var clist = new List<ColorGradingCondition.CValueThreshold>();
            var h = (0.0, 0.0);
            foreach (var line in lines)
            {
                var value = line.Split(':');
                if (value[0] == "Y?")
                {
                    var value2 = value[1].Split(',');
                    h = (double.Parse(value2[0]), double.Parse(value2[1]));
                    continue;
                }
                if (value[0].StartsWith("/"))
                {
                    continue;
                }
                clist.Add(new ColorGradingCondition.CValueThreshold(value[0], double.Parse(value[1])));
            }

            return new ColorGradingCondition(clist, h);
        }
    }
    //public class ColorimetryConditionRepository : IColorimetryConditionRepository
    //{
    //    public ColorimetryConditionRepository()
    //    {
    //    }

    //    string _configFileName = "";
    //    string _shadinPixelsFileName = "";
    //    string _gradingConditionFilename = "";
    //    public ColorimetryCondition Load()
    //    {
    //        var obj = LoadConfig(_configFileName);
    //        var cmc = ColorimetryCondition.Default();


    //        return cmc;
    //    }

    //    public void Store(ColorimetryCondition condition)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public (int width, int height, float[] pixels) LoadFloatPixels(string name)
    //    {
    //        var w = 1024;
    //        var h = 768;
    //        var len = w * h * 3;
    //        var byteSize = len * 4;
    //        var pixels = new byte[byteSize];
    //        var pixels2 = new float[len];


    //        using (var sr = new System.IO.BinaryReader(System.IO.File.Open(name, System.IO.FileMode.Open)))
    //        {
    //            sr.Read(pixels, 0, byteSize);
    //        }

    //        unsafe
    //        {
    //            fixed (byte* src = pixels)
    //            fixed (float* dst = pixels2)
    //            {
    //                var srcPtr = src;
    //                var dstPtr = dst;
    //                for (var i = 0; i < len; i++)
    //                {
    //                    //var v1 = (byte)(*srcPtr >> 8);
    //                    //var v2 = (byte)(*srcPtr & 0xFF);

    //                    var b = new byte[] { *srcPtr, *(srcPtr + 1), *(srcPtr + 2), *(srcPtr + 3) };
    //                    var val = BitConverter.ToSingle(b, 0);
    //                    *dstPtr = val;

    //                    srcPtr += 4;
    //                    dstPtr++;
    //                }
    //            }
    //        }

    //        return (w, h, pixels2);
    //    }
    //    object LoadConfig(string name)
    //    {
    //        return Codeplex.Data.DynamicJson.Parse("config.json");
    //    }
    //}

    //public class ColorGradingConditionRepository : IColorGradingConditionRepository
    //{

    //    string _gradingConditionFilename = "colorgrade.txt";
    //    public ColorGradingCondition Load()
    //    {
    //        return LoadColorGradingConditions(_gradingConditionFilename);
    //    }

    //    public void Store(ColorGradingCondition condition)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    ColorGradingCondition LoadColorGradingConditions(string name)
    //    {
    //        var lines = System.IO.File.ReadAllLines(name);
    //        var clist = new List<ColorGradingCondition.CValueThreshold>();
    //        var h = (0.0, 0.0);
    //        foreach (var line in lines)
    //        {
    //            var value = line.Split(':');
    //            if (value[0] == "Y?")
    //            {
    //                var value2 = value[1].Split(',');
    //                h = (double.Parse(value2[0]), double.Parse(value2[1]));
    //                continue;
    //            }
    //            if (value[0].StartsWith("/"))
    //            {
    //                continue;
    //            }
    //            clist.Add(new ColorGradingCondition.CValueThreshold(value[0], double.Parse(value[1])));
    //        }

    //        return new ColorGradingCondition(clist, h);
    //    }
    //}

    //public class AppSettingsRepository : IAppSettingsRepository
    //{
    //    //IConfigration _configration;
    //    //public AppSettingsRepository(IConfiguration configuration)
    //    //{

    //    //}
    //    public AppSettings Load()
    //    {
    //        return new AppSettings();
    //    }

    //    public void Store(AppSettings settings)
    //    {
    //        var json = System.Text.Json.JsonSerializer.Serialize(settings);
    //        System.IO.File.WriteAllText("appsettings.json", json);
    //    }
    //}
    public class InstrumentalErrorCorrectionMatrixFileLoader : IInstrumentalErrorCorrectionMatrixFileLoader
    {
        public Dictionary<string, double[]> Execute(string path)
        {
            var text = System.IO.File.ReadAllText(path);
            var json = Codeplex.Data.DynamicJson.Parse(text);
            var d = new Dictionary<string, double[]>();
            foreach(var o in json)
            {
                var v1 = (string)o[0];
                var v2 = (double[])o[1];
                d.Add(v1, v2);
            }
            return d;
        }
    }
}
