using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;

namespace CGLCMIV2.Domain
{
    public class ColorimetryResult
    {
        public ColorimetryResult(CIEXYZ cxyz, CIELCH clch, CIELAB clab, XYZPixels pixels, MeasureArea area, CIEXYZ whitepoint, CIELAB[] labValues)
        {
            CenterOfGravityXYZ = cxyz;
            CenterOfGravityLCH = clch;
            CenterOfGravityLAB = clab;
            Pixels = pixels;
            MeasureArea = area;
            Whitepoint = whitepoint;
            LABValues = labValues;
        }

        public CIEXYZ CenterOfGravityXYZ
        {
            get;
        }

        public CIELCH CenterOfGravityLCH
        {
            get;
        }
        public CIELAB CenterOfGravityLAB
        {
            get;
        }
        public XYZPixels Pixels
        {
            get;
        }
        public MeasureArea MeasureArea
        {
            get;
        }
        
        public CIEXYZ Whitepoint
        {
            get;
        }
        public CIELAB[] LABValues
        {
            get;
        }
    }


    public class ColorimetryReport
    {
        ColorimetryReport(CIEXYZ cxyz, CIELCH clch, CIELAB clab, string serialnumber, CIEXYZ whitepoint, IList<XYZPixels> pixels)
        {
            XYZ = cxyz;
            LCH = clch;
            LAB = clab;
            SerialNumber = serialnumber;
            Whitepoint = whitepoint;
            Pixels = pixels;
        }

        public CIEXYZ XYZ
        {
            get;
        }

        public CIELAB LAB
        {
            get;
        }

        public CIELCH LCH
        {
            get;
        }

        public string SerialNumber
        {
            get;
        }
        public CIEXYZ Whitepoint
        {
            get;
        }
        public IList<XYZPixels> Pixels
        {
            get;
        }
        public static ColorimetryReport Aggregate(ColorimetryResult[] result, string serialnumber)
        {
            var Xw = result?.Select(a => a.Whitepoint.X).Average();
            var Yw = result?.Select(a => a.Whitepoint.Y).Average();
            var Zw = result?.Select(a => a.Whitepoint.Z).Average();
            var whitepoint = new CIEXYZ(Xw.Value, Yw.Value, Zw.Value);
            var labValues = new List<CIELAB>();
            foreach(var r in result)
            {
                //var lv = r.LABValues;
                //foreach(var v in lv)
                //{
                //    labValues.Add(v);
                //}
                labValues.AddRange(r.LABValues);
            }
            var cvlab = new ColorValuesLab(labValues.ToArray());
            var cglab = cvlab.CenterOfGravity;
            var cglch = cglab.ToLCH();
            var cgXYZ = cglab.ToXYZ(whitepoint);

            return new ColorimetryReport(cgXYZ, cglch, cglab, serialnumber, whitepoint, result.Select(x => x.Pixels).ToList());
        }

        public static ColorimetryReport Create(ColorimetryResult result, string serialnumber)
        {

            return new ColorimetryReport(result.CenterOfGravityXYZ, result.CenterOfGravityLCH, result.CenterOfGravityLAB, serialnumber, result.Whitepoint, new List<XYZPixels>() { result.Pixels });
        }
    }
    public class ColorGradeReport
    {
        public ColorGradeReport(ColorGrade grade, ColorimetryReport colorimetryReport)
        {
            ColorGrade = grade;
            ColorimetryReport = colorimetryReport;
            CreateDateTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
        }

        public ColorGrade ColorGrade { get; }
        public ColorimetryReport ColorimetryReport { get; }
        public string CreateDateTime { get; }
    }

    public class EffectiveArea
    {
        public EffectiveArea(int width, int height, byte[] pixels)
        {
            Width = width;
            Height = height;
            Pixels = pixels;
        }

        public int Width
        {
            get;
            set;
        }
        public int Height
        {
            get;
            set;
        }
        public byte[] Pixels
        {
            get;
            set;
        }

        public static EffectiveArea Create(int x, int y, ROI roi)
        {
            var p = new byte[x * y];
            var x1 = roi.X;
            var y1 = roi.Y;
            var x2 = roi.X + roi.Width;
            var y2 = roi.Y + roi.Height;
            var w = roi.Width;
            var h = roi.Height;
            for (var j = y1; j < y2; j++)
            {
                for (var i = x1; i < x2; i++)
                {
                    p[x + y * w] = 255;
                }
            }
            return new EffectiveArea(w, y, p);
        }
    }
    public class MeasureArea
    {
        public MeasureArea(int width, int height, byte[] pixels)
        {
            Width = width;
            Height = height;
            Pix = pixels;
        }

        public int Width
        {
            get;
            set;
        }
        public int Height
        {
            get;
            set;
        }
        public byte[] Pix
        {
            get;
            set;
        }
    }

    public interface IColorimetry
    {
        ColorimetryResult Measure(ColorimetryCondition measureCondition);
        (CIEXYZ whitepoint, CIEXYZ whitepointCorner, XYZPixels pixels) ScanWhitepoint(ColorimetryCondition condition);
    }

    public class Colorimetry : IColorimetry
    {
        public ICamera _camera;
        public Colorimetry(ICamera camera)
        {
            _camera = camera;
        }

        public ColorimetryResult Measure(ColorimetryCondition measureCondition)
        {

            var p = _camera.TakePicture(measureCondition.ExposureTime, measureCondition.Integration);
            var area = DetectContour(p);
            p = CorrectShading(p, measureCondition.ShadingCorrectPixels);
            p = RAW2XYZ(p, measureCondition.RAW2XYZMatrix);
            var xyzw = CalcWhiteReference(p);
            //var p2 = MaskDetectContour(p.Width, p.Height, p.Pix, area.Pixels);
            //var xyz = XYZ1D(p2);
            var xyz = MeasureEffectivePixelValues(p, area);
            var labValues = Lab1D(xyz, xyzw);
            var cvLab = new ColorValuesLab(labValues);
            var cgLAB = cvLab.CenterOfGravity;
            cgLAB = CorrectLAB(cgLAB, measureCondition.LabCorrectMat);
            var cgLCH = cgLAB.ToLCH();
            var cgXYZ = cgLAB.ToXYZ(xyzw);
            //var cvXYZ = new ColorValuesXYZ(xyz);
            //var cgXYZ = cvXYZ.CenterOfGravity;
            //var cgLAB = cgXYZ.ToLAB(xyzw3);
            //var cgLCH = cgLAB.ToLCH();

            return new ColorimetryResult(cgXYZ, cgLCH, cgLAB, p, area, xyzw, labValues);

        }

        public (CIEXYZ whitepoint, CIEXYZ whitepointCorner, XYZPixels pixels) ScanWhitepoint(ColorimetryCondition condition)
        {
            var pixels = _camera.TakePicture(condition.ExposureTime, condition.Integration);
            pixels = CorrectShading(pixels, condition.ShadingCorrectPixels);
            pixels = RAW2XYZ(pixels, condition.RAW2XYZMatrix);

            //var v1 = CalcWhiteReference(new ROI(314, 234, 400, 300), pixels);
            var v2 = CalcWhiteReference(pixels);

            return (v2, v2, pixels);
        }
        public static ShadingCorrectPixels MakeShadingData(XYZPixels pixels)
        {
            
            var w = pixels.Width;
            var h = pixels.Height;
            var src = pixels.Pix;
            var tmp = new float[w * h * 3];
            var framesize = w * h;

            //20x20 median
            var filterSize = 20;

            unsafe
            {
                fixed (ushort* srcBase = src)
                fixed (float* tmpBase = tmp)
                {
                    var srcPtr1 = srcBase;
                    var srcPtr2 = srcBase + framesize;
                    var srcPtr3 = srcBase + framesize * 2;
                    var tmpPtr1 = tmpBase;
                    var tmpPtr2 = tmpBase + framesize;
                    var tmpPtr3 = tmpBase + framesize * 2;

                    for (var j = 0; j < h; j++)
                    {
                        for (var i = 0; i < w; i++)
                        {
                            var n = 0;
                            var sum1 = 0.0f;
                            var sum2 = 0.0f;
                            var sum3 = 0.0f;
                            for (var k = j - filterSize / 2; k < j + filterSize / 2; k++)
                            {
                                for (var l = i - filterSize / 2; l < i + filterSize / 2; l++)
                                {
                                    var x = (l < 0) ? 0 : ((l >= w) ? w - 1 : l);
                                    var y = (k < 0) ? 0 : ((k >= h) ? h - 1 : k);
                                    sum1 += srcPtr1[x + y * w];
                                    sum2 += srcPtr2[x + y * w];
                                    sum3 += srcPtr3[x + y * w];
                                    n++;
                                }
                            }
                            tmpPtr1[i + j * w] = sum1 / n;
                            tmpPtr2[i + j * w] = sum2 / n;
                            tmpPtr3[i + j * w] = sum3 / n;
                        }
                    }
                }
            }

            //average
            var x1 = 200;
            var x2 = w - x1;
            var y1 = 150;
            var y2 = h - y1;
            var x3 = x2 - x1;
            var y3 = y2 - y1;

            var ave1 = 0.0f;
            var ave2 = 0.0f;
            var ave3 = 0.0f;

            unsafe
            {
                fixed (float* tmpBase = tmp)
                {
                    var tmpPtr1 = tmpBase;
                    var tmpPtr2 = tmpBase + framesize;
                    var tmpPtr3 = tmpBase + framesize * 2;

                    for (var j = y1; j < y2; j++)
                    {
                        for (var i = x1; i < x2; i++)
                        {

                            ave1 += tmpPtr1[i + j * w];
                            ave2 += tmpPtr2[i + j * w];
                            ave3 += tmpPtr3[i + j * w];
                        }
                    }
                }

                ave1 /= x3 * y3;
                ave2 /= x3 * y3;
                ave3 /= x3 * y3;
            }


            //
            unsafe
            {
                fixed (float* tmpBase = tmp)
                {
                    var tmpPtr1 = tmpBase;
                    var tmpPtr2 = tmpBase + framesize;
                    var tmpPtr3 = tmpBase + framesize * 2;

                    for (var j = 0; j < h; j++)
                    {
                        for (var i = 0; i < w; i++)
                        {
                            *tmpPtr1 /= ave1; tmpPtr1++;
                            *tmpPtr2 /= ave2; tmpPtr2++;
                            *tmpPtr3 /= ave3; tmpPtr3++;
                        }
                    }
                }
            }

            return new ShadingCorrectPixels(tmp, w, h);
        }

        CIEXYZ CalcWhiteReference(ROI roi, XYZPixels pixels)
        {
            var px = roi.X;
            var py = roi.Y;
            var pw = roi.Width;
            var ph = roi.Height;

            var x = 0.0;
            var y = 0.0;
            var z = 0.0;
            var n = 0;

            var w = pixels.Width;
            var h = pixels.Height;

            unsafe
            {
                fixed (ushort* srcBase = pixels.Pix)
                {
                    var srcPtr1 = srcBase;
                    var srcPtr2 = srcBase + w * h;
                    var srcPtr3 = srcBase + w * h * 2;

                    for (var j = py; j < py + ph; j++)
                    {
                        for (var i = px; i < px + pw; i++)
                        {
                            x += srcPtr1[i + j * w];
                            y += srcPtr2[i + j * w];
                            z += srcPtr3[i + j * w];
                            n++;
                        }
                    }

                }
            }

            x = x / n;
            y = y / n;
            z = z / n;

            return new CIEXYZ(x, y, z);
        }

        CIEXYZ CalcWhiteReference(XYZPixels pixels)
        {
            var offset = 5;
            var xsize = 115;
            var ysize = 85;
            var w = pixels.Width;
            var h = pixels.Height;

            var xyz1 = CalcWhiteReference(new ROI(offset, offset, xsize, ysize), pixels);
            var xyz2 = CalcWhiteReference(new ROI(w - offset - xsize, offset, xsize, ysize), pixels);
            var xyz3 = CalcWhiteReference(new ROI(offset, h - ysize - offset, xsize, ysize), pixels);
            var xyz4 = CalcWhiteReference(new ROI(w - xsize - offset, h - ysize - offset, xsize, ysize), pixels);

            var x = (xyz1.X + xyz2.X + xyz3.X + xyz4.X) / 4.0;
            var y = (xyz1.Y + xyz2.Y + xyz3.Y + xyz4.Y) / 4.0;
            var z = (xyz1.Z + xyz2.Z + xyz3.Z + xyz4.Z) / 4.0;

            return new CIEXYZ(x, y, z);
        }

        XYZPixels CorrectWhite(XYZPixels value, double[] factor)
        {
            var w = value.Width;
            var h = value.Height;
            var srcPixels = value.Pix;
            var dstPixels = new ushort[w * h * 3];

            unsafe
            {
                fixed (ushort* srcBase = srcPixels)
                fixed (ushort* dstBase = dstPixels)
                {
                    var framesize = w * h;
                    var srcPtr1 = srcBase + 0;
                    var srcPtr2 = srcBase + framesize;
                    var srcPtr3 = srcBase + framesize * 2;

                    var dstPtr1 = dstBase;
                    var dstPtr2 = dstBase + framesize;
                    var dstPtr3 = dstBase + framesize * 2;

                    for (var i = 0; i < framesize; i++)
                    {
                        var x = *srcPtr1 == 0 ? 0.0 : *srcPtr1 / factor[0];
                        var y = *srcPtr2 == 0 ? 0.0 : *srcPtr2 / factor[1];
                        var z = *srcPtr3 == 0 ? 0.0 : *srcPtr3 / factor[2];

                        *(dstPtr1) = (ushort)x;
                        *(dstPtr2) = (ushort)y;
                        *(dstPtr3) = (ushort)z;

                        srcPtr1++;
                        srcPtr2++;
                        srcPtr3++;
                        dstPtr1++;
                        dstPtr2++;
                        dstPtr3++;

                    }
                }
            }

            return new XYZPixels(dstPixels, w, h);
        }

        public static XYZPixels CorrectShading(XYZPixels value, ShadingCorrectPixels shading)
        {
            var w = value.Width;
            var h = value.Height;
            var srcPixels = value.Pix;
            var shdPixels = shading.Pix;
            var dstPixels = new ushort[w * h * 3];

            unsafe
            {
                fixed (ushort* srcBase = srcPixels)
                fixed (ushort* dstBase = dstPixels)
                fixed (float* shdBase = shdPixels)
                {
                    var framesize = w * h;
                    var srcPtr1 = srcBase + 0;
                    var srcPtr2 = srcBase + framesize;
                    var srcPtr3 = srcBase + framesize * 2;

                    var dstPtr1 = dstBase;
                    var dstPtr2 = dstBase + framesize;
                    var dstPtr3 = dstBase + framesize * 2;

                    var shdPtr1 = shdBase;
                    var shdPtr2 = shdBase + framesize;
                    var shdPtr3 = shdBase + framesize * 2;

                    for (var i = 0; i < framesize; i++)
                    {
                        var x = *shdPtr1 == 0 ? 0.0 : *srcPtr1 / *shdPtr1;
                        var y = *shdPtr2 == 0 ? 0.0 : *srcPtr2 / *shdPtr2;
                        var z = *shdPtr3 == 0 ? 0.0 : *srcPtr3 / *shdPtr3;

                        *(dstPtr1) = (ushort)x;
                        *(dstPtr2) = (ushort)y;
                        *(dstPtr3) = (ushort)z;

                        srcPtr1++;
                        srcPtr2++;
                        srcPtr3++;
                        dstPtr1++;
                        dstPtr2++;
                        dstPtr3++;
                        shdPtr1++;
                        shdPtr2++;
                        shdPtr3++;
                    }
                }
            }

            return new XYZPixels(dstPixels, w, h);
        }

        MeasureArea DetectContour(XYZPixels value)
        {
            var w = value.Width;
            var h = value.Height;
            var pixels = value.Pix;
            var contour = new byte[w * h];
            DiamondContour.Measure.DetectContour(w, h, pixels, contour);
            return new MeasureArea(w, h, contour);
        }

        XYZPixels RAW2XYZ(XYZPixels value, double[][] raw2XYZMatrix)
        {
            var w = value.Width;
            var h = value.Height;
            var pixels = value.Pix;
            var mat = raw2XYZMatrix;
            var dst = new ushort[w * h * 3];

            unsafe
            {
                fixed (ushort* srcBase = pixels)
                fixed (ushort* dstBase = dst)
                {
                    var framesize = w * h;
                    var srcPtr1 = srcBase + 0;
                    var srcPtr2 = srcBase + framesize;
                    var srcPtr3 = srcBase + framesize * 2;

                    var dstPtr1 = dstBase;
                    var dstPtr2 = dstBase + framesize;
                    var dstPtr3 = dstBase + framesize * 2;

                    for (var i = 0; i < framesize; i++)
                    {
                        var s1 = (double)*(srcPtr1);
                        var s2 = (double)*(srcPtr2);
                        var s3 = (double)*(srcPtr3);

                        var x = mat[0][0] * s1 + mat[0][1] * s2 + mat[0][2] * s3;
                        var y = mat[1][0] * s1 + mat[1][1] * s2 + mat[1][2] * s3;
                        var z = mat[2][0] * s1 + mat[2][1] * s2 + mat[2][2] * s3;

                        *(dstPtr1) = (ushort)(x < 0 ? 0 : (ushort)x);
                        *(dstPtr2) = (ushort)(y < 0 ? 0 : (ushort)y);
                        *(dstPtr3) = (ushort)(z < 0 ? 0 : (ushort)z);

                        srcPtr1++;
                        srcPtr2++;
                        srcPtr3++;
                        dstPtr1++;
                        dstPtr2++;
                        dstPtr3++;
                    }

                }
            }

            return new XYZPixels(dst, w, h);
        }

        unsafe ushort[][] MaskDetectContour(int w, int h, ushort[] value, byte[] mask)
        {
            var s1 = new List<ushort>();
            var s2 = new List<ushort>();
            var s3 = new List<ushort>();

            fixed (ushort* srcBase = value)
            fixed (byte* maskBase = mask)
            {
                var framesize = w * h;
                var srcPtr1 = srcBase + 0;
                var srcPtr2 = srcBase + framesize;
                var srcPtr3 = srcBase + framesize * 2;
                var maskPtr = maskBase;

                for (var i = 0; i < framesize; i++)
                {
                    if (*maskPtr == 255)
                    {
                        s1.Add(*srcPtr1);
                        s2.Add(*srcPtr2);
                        s3.Add(*srcPtr3);
                    }
                    srcPtr1++;
                    srcPtr2++;
                    srcPtr3++;
                    maskPtr++;
                }
            }
            return new ushort[][] { s1.ToArray(), s2.ToArray(), s3.ToArray() };
        }

        unsafe CIEXYZ[] MeasureEffectivePixelValues(XYZPixels value, MeasureArea mask)
        {
            //var s1 = new List<ushort>();
            //var s2 = new List<ushort>();
            //var s3 = new List<ushort>();
            var dst = new List<CIEXYZ>();

            var w = value.Width;
            var h = value.Height;

            fixed (ushort* srcBase = value.Pix)
            fixed (byte* maskBase = mask.Pix)
            {
                var framesize = w * h;
                var srcPtr1 = srcBase + 0;
                var srcPtr2 = srcBase + framesize;
                var srcPtr3 = srcBase + framesize * 2;
                var maskPtr = maskBase;

                for (var i = 0; i < framesize; i++)
                {
                    if (*maskPtr == 255)
                    {
                        dst.Add(new CIEXYZ(*srcPtr1, *srcPtr2, *srcPtr3));
                    }
                    srcPtr1++;
                    srcPtr2++;
                    srcPtr3++;
                    maskPtr++;
                }
            }
            return dst.ToArray();
        }
        XYZPixels CalibrateWhiteReference(XYZPixels src, CIEXYZ XYZ, CIEXYZ whitePoint)
        {
            var w = src.Width;
            var h = src.Height;
            var pixels = src.Pix;
            var dst = new ushort[w * h * 3];

            var factor = new double[] { XYZ.X / whitePoint.X, XYZ.Y / whitePoint.Y, XYZ.Z / whitePoint.Z };

            unsafe
            {
                fixed (ushort* srcBase = pixels)
                fixed (ushort* dstBase = dst)
                {
                    var framesize = w * h;
                    var srcPtr1 = srcBase + 0;
                    var srcPtr2 = srcBase + framesize;
                    var srcPtr3 = srcBase + framesize * 2;

                    var dstPtr1 = dstBase;
                    var dstPtr2 = dstBase + framesize;
                    var dstPtr3 = dstBase + framesize * 2;

                    for (var i = 0; i < framesize; i++)
                    {
                        *(dstPtr1) = (ushort)(*srcPtr1 / factor[0]);
                        *(dstPtr2) = (ushort)(*srcPtr2 / factor[1]);
                        *(dstPtr3) = (ushort)(*srcPtr3 / factor[2]);

                        srcPtr1++;
                        srcPtr2++;
                        srcPtr3++;
                        dstPtr1++;
                        dstPtr2++;
                        dstPtr3++;
                    }

                }
            }

            return new XYZPixels(dst, w, h);
        }

        CIEXYZ[] XYZ1D(ushort[][] pixels)
        {
            var res = new List<CIEXYZ>();
            var range = (Min: 0, Max: 1023);
            for (var i = 0; i < pixels[0].Length; i++)
            {
                if ((pixels[0][i] > range.Min && pixels[0][i] < range.Max)
                    && (pixels[1][i] > range.Min && pixels[1][i] < range.Max)
                    && (pixels[2][i] > range.Min && pixels[2][i] < range.Max))
                    res.Add(new CIEXYZ(pixels[0][i], pixels[1][i], pixels[2][i]));
            }

            return res.ToArray();
        }
        CIEXYZ[] XYZ1D(ushort[] pixels)
        {
            var res = new List<CIEXYZ>();
            var range = (Min: 0, Max: 1023);
            //var offset = 0;
            for (var i = 0; i < pixels.Length/3; i+=3)
            {
                if ((pixels[i] > range.Min && pixels[i] < range.Max)
                    && (pixels[i + 1] > range.Min && pixels[i + 1] < range.Max)
                    && (pixels[i + 2] > range.Min && pixels[i + 2] < range.Max))
                    res.Add(new CIEXYZ(pixels[i], pixels[i + 1], pixels[i + 2]));
            }

            return res.ToArray();
        }
        CIEXYZ[] MaskEffectiveData(CIEXYZ[] values, byte[] mask)
        {
            var res = new List<CIEXYZ>();

            for(var i=0;i<values.Length;i++)
            {
                if(mask[i] == 255)
                {
                    res.Add(values[i]);
                }
            }

            return res.ToArray();
        }
        

        CIELAB[] Lab1D(CIEXYZ[] XYZ, CIEXYZ whitePoint)
        {

            var res = new CIELAB[XYZ.Length];
            var w = new CIEXYZ(whitePoint.X, whitePoint.Y, whitePoint.Z);
            for (var i = 0; i < XYZ.Length; i++)
            {
                try
                {
                    res[i] = XYZ[i].ToLAB(w);
                }
                catch
                {
                    res[i] = new CIELAB(0, 0, 0);
                }
            }
            return res;
        }

        public CIELAB CorrectLAB(CIELAB value, double[] m)
        {
            var l1 = value.L / 100.0;
            var a1 = value.A;
            var b1 = value.B;
            var l2 = l1 * m[0] + a1 * m[1] + b1 * m[2];
            var a2 = l1 * m[3] + a1 * m[4] + b1 * m[5];
            var b2 = l1 * m[6] + a1 * m[7] + b1 * m[8];

            return new CIELAB(l2 * 100.0, a2, b2);
        }

        //CIELCH[] Lab1D(CIELAB[] LAB)
        //{

        //    var res = new CIELCH[LAB.Length];
        //    for (var i = 0; i < LAB.Length; i++)
        //    {
        //        res[i] = LAB[i].ToLCH();
        //    }
        //    return res;
        //}

    }

}
