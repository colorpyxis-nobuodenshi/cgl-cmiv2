using CGLCMIV2.Application;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGLCMIV2.Domain
{
    public interface IMeasureResultWriter
    {
        void Write(string message);
        void Write(string dir, string title, string message);
    }

    public interface IWhitepointWriter
    {
        //void Write(CIEXYZ whitepoint, double temperature, double opticalpower, string processingDatetime, string systemSerialNumber);
        void Write(string message);
    }

    public class ColorGrade
    {
        public ColorGrade(string value, string sufix)
        {
            Value = value;
            Sufix = sufix;
        }

        public string Value { get; private set; }
        public string Sufix { get; private set; }

        public (string value, string sufix1, string sufix2) Values
        {
            get
            {
                var value = Value.Replace("+", "").Replace("-", "");
                var sufix1 = Value.IndexOf("+") > 0 ? "+" : Value.IndexOf("-") > 0 ? "-" : "";
                var sufix2 = Sufix;

                return (value, sufix1, sufix2);
            }
        }
        public override string ToString()
        {
            return $"{Value}{Sufix}";
        }
    }

    public interface IColorGradeJudgement
    {
        ColorGrade Execute(CIELCH lch, ColorGradingCondition condition);
    }

    public class ColorGradeJudgement : IColorGradeJudgement
    {

        public ColorGrade Execute(CIELCH lch, ColorGradingCondition condition)
        {
            var l = lch.L;
            var c = lch.C;
            var h = lch.H;
            var clist = condition.CValueThresholds;
            var (hmin, hmax) = condition.HValueThreshold;
            var grade = string.Empty;

            if (c < clist[0].Value)
            {
                return new ColorGrade(clist[0].Grade, "");
            }

            for (var i = 0; i < clist.Count - 1; i++)
            {
                if (c >= clist[i].Value && c < clist[i + 1].Value)
                {
                    grade = clist[i].Grade;
                    if (grade == "D")
                    {
                        return new ColorGrade(grade, "");
                    }
                    if (grade == "D-")
                    {
                        return new ColorGrade(grade, "");
                    }
                    if (h < hmin)
                    {
                        return new ColorGrade(grade, "↓");
                    }
                    if (h > hmax)
                    {
                        return new ColorGrade(grade, "↑");
                    }

                    return new ColorGrade(grade, "");
                }
            }

            grade = clist[clist.Count - 1].Grade;
            if (h < hmin)
            {
                return new ColorGrade(grade, "↓");
            }
            if (h > hmax)
            {
                return new ColorGrade(grade, "↑");
            }

            return new ColorGrade(grade, "");
        }
    }

    public class Pixels<T>
    {
        public Pixels(T[] pix, int width, int height)
        {
            Pix = pix;
            Width = width;
            Height = height;
        }

        public T[] Pix { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        
    }

    public class XYZPixels : Pixels<ushort>
    {
        public XYZPixels(ushort[] pix, int width, int heght)
            : base(pix, width, heght)
        {
            
        }

        public ushort[] PixPlanar
        {
            get
            {
                var p1 = Pix.Where((_, i) => i % 3 == 0).ToArray();
                var p2 = Pix.Where((_, i) => i % 3 == 1).ToArray();
                var p3 = Pix.Where((_, i) => i % 3 == 2).ToArray();
                return p1.Concat(p2).Concat(p3).ToArray();
            }
        }
    }

    public class S1S2S3Pixels : Pixels<ushort>
    {
        public S1S2S3Pixels(ushort[] pix, int width, int heght)
            : base(pix, width, heght)
        {

        }
    }

    public class ShadingCorrectPixels : Pixels<float>
    {
        public ShadingCorrectPixels(float[] pix, int width, int heght)
            : base(pix, width, heght)
        {

        }
    }

    public class MultiColorConversionMatrix
    {
        //public record ColorConversionMatrix
        //{
        //    public double U1 { get; }
        //    public double U2 { get; }
        //    public double V1 { get; }
        //    public double V2 { get; }
        //    public double[][] Matrix { get; }

        //}
        MultiColorConversionMatrix(ColorConversionMatrix[] ccm)
        {
            CCM = ccm;
        }
        public static MultiColorConversionMatrix Load(ColorConversionMatrix[] ccm)
        {
            return new MultiColorConversionMatrix(ccm);
        }

        public double[][] Find(double u, double v)
        {
            foreach(var m in CCM)
            {
                if(u >= m.U[0] && u <= m.U[1] && v >= m.V[0] && v <= m.V[1])
                {
                    return m.Mat;
                }
            }
            return [[1.0, 0.0, 0.0], [0.0, 1.0, 0.0], [0.0, 0.0, 1.0]];
        }
        public ColorConversionMatrix[] CCM { get; }
    }
    public struct ROI
    {
        public ROI(int x, int y, int w, int h)
        {
            X = x;
            Y = y;
            Width = w;
            Height = h;
        }

        public int X { get; private set; }
        public int Y { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

    }

    public class CIEYxy
    {
        public CIEYxy(double _Y, double _x, double _y)
        {
            x = _x;
            y = _y;
            Y = _Y;
        }

        public double Y
        {
            get;
            private set;
        }

        public double x
        {
            get;
            private set;
        }

        public double y
        {
            get;
            private set;
        }

        public static CIEYxy From(CIEXYZ value)
        {
            var X = value.X;
            var Y = value.Y;
            var Z = value.Z;

            if (X + Y + Z == 0)
            {
                return new CIEYxy(0, 0, 0);
            }

            return new CIEYxy(Y, X / (X + Y + Z), Y / (X + Y + Z));
        }

        public override string ToString()
        {
            return $"{Y:F0},{x:F3},{y:F3}";
        }
    }

    public class CIEXYZ
    {
        public CIEXYZ(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public double X
        {
            get;
            private set;
        }

        public double Y
        {
            get;
            private set;
        }

        public double Z
        {
            get;
            private set;
        }


        public CIELAB ToLAB(CIEXYZ whitePoint)
        {
            if (X + Y + Z == 0)
                return new CIELAB(0, 0, 0);

            Func<double, double, double> f = ((v1, v2) =>
            {
                var r = v2 == 0 ? 0 : v1 / v2;
                if (r > 0.008856)
                {
                    r = Math.Pow(r, 1.0 / 3.0);
                }
                else
                {
                    r = 7.787 * r + 16.0 / 116.0;
                }
                return r;
            });


            var fx = f(X, whitePoint.X);
            var fy = f(Y, whitePoint.Y);
            var fz = f(Z, whitePoint.Z);

            var l = fy * 116.0 - 16.0;
            var a = 500.0 * (fx - fy);
            var b = 200.0 * (fy - fz);

            return new CIELAB(l, a, b);
        }

        public CIEYxy ToYxy()
        {
            return CIEYxy.From(this);
        }

        public override string ToString()
        {
            return string.Format("{0:F0},{1:F0},{2:F0}", X, Y, Z);
        }

        public double[] To1DArray()
        {
            return new double[] { X, Y, Z };
        }
    }

    public class CIELAB
    {
        public CIELAB(double l, double a, double b)
        {
            L = l;
            A = a;
            B = b;
        }

        public double L
        {
            get;
            private set;
        }

        public double A
        {
            get;
            private set;
        }

        public double B
        {
            get;
            private set;
        }


        public CIELCH ToLCH()
        {
            if (A == 0 & B == 0 & L == 0)
                return new CIELCH(0, 0, 0);

            var a = A;
            var b = B;

            var h = (Math.Atan2(b, a)) * (180.0 / Math.PI);

            if (h < 0)
            {
                h += 360.0;
            }
            else if (h >= 360)
            {
                h -= 360.0;
            }

            var c = Math.Sqrt(a * a + b * b);

            return new CIELCH(L, c, h);
        }

        public CIEXYZ ToXYZ(CIEXYZ whitepoint)
        {
            if (A == 0 & B == 0 & L == 0)
                return new CIEXYZ(0, 0, 0);

            var fy = (L + 16.0) / 116.0;
            var fx = fy + (A / 500.0);
            var fz = fy - (B / 200.0);
            var xn = whitepoint.X;
            var yn = whitepoint.Y;
            var zn = whitepoint.Z;

            var y = fy > 0.008856 ? Math.Pow(fy, 3.0) * yn : (fy - 16.0 / 116.0) / 7.787;
            var x = fx > 0.008856 ? Math.Pow(fx, 3.0) * xn : (fx - 16.0 / 116.0) / 7.787;
            var z = fz > 0.008856 ? Math.Pow(fz, 3.0) * zn : (fz - 16.0 / 116.0) / 7.787;

            return new CIEXYZ(x, y, z);
        }

        public override string ToString()
        {
            return string.Format("{0:F1},{1:F4},{2:F4}", L, A, B);
        }
    }

    public class CIELCH
    {
        public CIELCH(double l, double c, double h)
        {
            L = l;
            C = c;
            H = h;
        }

        public double L
        {
            get;
            private set;
        }

        public double C
        {
            get;
            private set;
        }

        public double H
        {
            get;
            private set;
        }

        public override string ToString()
        {
            return string.Format("{0:F1},{1:F4},{2:F2}", L, C, H);
        }
    }

    public class ColorimetryCondition
    {
        public int ExposureTime { get; }
        public int Integration { get; }
        public ShadingCorrectPixels ShadingCorrectPixels { get; }
        //public double[][] RAW2XYZMatrix { get; }
        public CIEXYZ Whitepoint { get; }
        public CIEXYZ WhitepointForCorrect { get; }
        public (int D65Value, int UVValue) LEDValue { get; }
        public int MeasurePoint { get; }
        public double[] LabCorrectMat { get; }
        public CIEXYZ WhitepointOnSpectralon { get; }
        public CIEXYZ WhitepointForCorrectionOnSpectralon { get; }
        public double[] WhitebalanceGain { get; }
        public MultiColorConversionMatrix MultiColorConversionMatrix { get; set; }
        protected ColorimetryCondition(int exposureTime, int integration, ShadingCorrectPixels correctPixels, MultiColorConversionMatrix mccm, CIEXYZ whitepoint, CIEXYZ whitepointForCorrection, (int D65, int UV) ledValue, int measurePoint, double[] labMat, CIEXYZ whitepointOnSpectralon, CIEXYZ whitepointForCorrectionOnSpectralon, double[] whitebalanceGain)
        {
            ExposureTime = exposureTime;
            Integration = integration;
            ShadingCorrectPixels = correctPixels;
            MultiColorConversionMatrix = mccm;
            Whitepoint = whitepoint;
            WhitepointForCorrect = whitepointForCorrection;
            LEDValue = ledValue;
            MeasurePoint = measurePoint;
            LabCorrectMat = labMat;
            WhitepointOnSpectralon = whitepointOnSpectralon;
            WhitepointForCorrectionOnSpectralon = whitepointForCorrectionOnSpectralon;
            WhitebalanceGain = whitebalanceGain;
        }

        public static ColorimetryCondition Create(int exposureTime, int integration, ShadingCorrectPixels correctPixels, MultiColorConversionMatrix mccm, CIEXYZ whitepoint, CIEXYZ whitepointForCorrection, (int D65, int UV) ledValue, int measurePoint, double[] labMat, CIEXYZ whitepointOnSpectralon, CIEXYZ whitepointForCorrectionOnSpectralon, double[] whitebalanceGain)
        {
            return new ColorimetryCondition(exposureTime, integration, correctPixels, mccm, whitepoint, whitepointForCorrection, ledValue, measurePoint, labMat, whitepointOnSpectralon, whitepointForCorrectionOnSpectralon, whitebalanceGain);
        }
        
    }

    public class ColorGradingCondition
    {
        public class CValueThreshold
        {
            public CValueThreshold(string grade, double cv)
            {
                Grade = grade;
                Value = cv;
            }
            public string Grade { get; }
            public double Value { get; }
        }

        public ColorGradingCondition(List<CValueThreshold> cvs, (double min, double max) hv)
        {
            CValueThresholds = cvs;
            HValueThreshold = hv;
        }
        public List<CValueThreshold> CValueThresholds { get; }
        public (double Min, double Max) HValueThreshold { get; }
    }

    public class Histogram
    {

        public Histogram(double[] values, double minValue = 0, double maxValue = 1023, int bins = 1023)
        {
            if (values.Length == 0)
                return;
            var values1 = new double[bins + 1];
            //var values2 = new double[bins + 1];

            var binSize = (maxValue - minValue) / (bins);

            foreach (var v in values)
            {
                var v2 = v;
                if (v2 < minValue)
                {
                    continue;
                }

                if (v2 > maxValue)
                {
                    continue;
                }

                v2 = (v2 - minValue) / binSize;

                values1[(int)v2]++;
            }

            Min = values.Min();
            Max = values.Max();

            var sum = 0.0;
            var total = 0.0;
            for (var i = 0; i < bins; i++)
            {
                var kaikyuu = minValue + binSize * i;
                var dosuu = values1[i];
                sum += kaikyuu * dosuu;
                total += dosuu;
            }
            CenterOfGravity = sum / total;

            Average = values.Average();
        }

        public double Min
        {
            get;
            private set;
        }

        public double Max
        {
            get;
            private set;
        }

        public double CenterOfGravity
        {
            get;
            private set;
        }

        public double Average
        {
            get;
            private set;
        }
    }

    
    public class ColorValuesLab
    {
        public Histogram L
        {
            get;
            private set;
        }

        public Histogram A
        {
            get;
            private set;
        }

        public Histogram B
        {
            get;
            private set;
        }

        public CIELAB CenterOfGravity
        {
            get
            {
                return new CIELAB(L.CenterOfGravity, A.CenterOfGravity, B.CenterOfGravity);
            }
        }

        public CIELAB Average
        {
            get
            {
                return new CIELAB(L.Average, A.Average, B.Average);
            }
        }

        public ColorValuesLab(CIELAB[] LAB)
        {
            var a = LAB.Select(_ => _.A).ToArray();
            var b = LAB.Select(_ => _.B).ToArray();
            L = new Histogram(LAB.Select(_ => _.L).ToArray(), 0, 200, 200);
            A = new Histogram(a, -100, 100, 200000);
            B = new Histogram(b, -100, 100, 200000);
            
        }
    }

    public class ColorValuesXYZ
    {
        public Histogram X
        {
            get;
            private set;
        }

        public Histogram Y
        {
            get;
            private set;
        }

        public Histogram Z
        {
            get;
            private set;
        }

        public CIEXYZ CenterOfGravity
        {
            get
            {
                return new CIEXYZ(X.CenterOfGravity, Y.CenterOfGravity, Z.CenterOfGravity);
            }
        }

        public CIEXYZ Average
        {
            get
            {
                return new CIEXYZ(X.Average, Y.Average, Z.Average);
            }
        }

        public ColorValuesXYZ(CIEXYZ[] XYZ)
        {
            var x = XYZ.Select(_ => _.X).ToArray();
            var y = XYZ.Select(_ => _.Y).ToArray();
            var z = XYZ.Select(_ => _.Z).ToArray();
            X = new Histogram(x, 0, 1023, 1023);
            Y = new Histogram(y, 0, 1023, 1023);
            Z = new Histogram(z, 0, 1023, 1023);

        }
    }

    public class DisplayPixels : Pixels<byte>
    {

        public DisplayPixels(byte[] pix, int width, int height)
            :base(pix, width, height)
        {
            
        }
        public DisplayPixels Subpixels(ROI roi)
        {
            var x1 = roi.X;
            var y1 = roi.Y;
            var x2 = roi.X + roi.Width * 4;
            var y2 = roi.Y + roi.Height;
            var w2 = roi.Width * 4;
            var h2 = roi.Height;
            var w = Width * 4;
            var sp = new byte[w2 * h2 * 4];

            for (var y = y1; y < y2; y++)
            {
                for (var x = x1; x < x2; y++)
                {
                    sp[(x - x1) + (y - y1) * w2] = Pix[x + y * w];
                }
            }

            return new DisplayPixels(sp, x1, y1);
        }
        public DisplayPixels Subpixels()
        {
            var roi = new ROI(120, 90, 1024 - 120 - 120, 768 - 90 - 90);
            var x1 = roi.X;
            var y1 = roi.Y;
            var x2 = roi.X + roi.Width * 4;
            var y2 = roi.Y + roi.Height;
            var w2 = roi.Width * 4;
            var h2 = roi.Height;
            var w = Width * 4;
            var sp = new byte[w2 * h2 * 4];

            for (var y = y1; y < y2; y++)
            {
                for (var x = x1; x < x2; x++)
                {
                    sp[(x - x1) + (y - y1) * w2] = Pix[x + y * w];
                }
            }

            return new DisplayPixels(sp, roi.Width, roi.Height);
        }
        public unsafe static DisplayPixels Convert(XYZPixels xyzPixels, MeasureArea area = null)
        {
            var width = xyzPixels.Width;
            var height = xyzPixels.Height;
            var pix = xyzPixels.Pix;
            var maskarray = area?.Pix;
            var min = 0.0;
            var max = 1.0;
            var stride = width * 4;
            var framesize = width * height;
            
            var rgbPixels = new byte[stride * height];
            var mat = new double[][]
            {
                new double[]
                {
                    1.909,
                    -0.532,
                    -0.288
                },
                new double[]
                {
                    -0.984,
                    1.999,
                    -0.0283
                },
                new double[]
                {
                    0.0583,
                    -0.118,
                    0.897
                }
            };

            
            fixed (ushort* srcPixels = pix)
            fixed (byte* dstPixels = rgbPixels)
            fixed (byte* pixels2 = maskarray)
            {
                var srcBase = srcPixels;
                var dstBase = dstPixels;
                ushort* srcPtr1 = srcBase + 0;
                ushort* srcPtr2 = srcBase + framesize;
                ushort* srcPtr3 = srcBase + framesize * 2;

                var maskPtr = pixels2;

                for (int i = 0; i < height; i++)
                {

                    byte* dstPtr = dstBase + stride * i;
                    for (int j = 0; j < width; j++)
                    {
                        var x = *(srcPtr1) / 1023.0;
                        var y = *(srcPtr2) / 1023.0;
                        var z = *(srcPtr3) / 1023.0;

                        var r = mat[0][0] * x + mat[0][1] * y + mat[0][2] * z;
                        var g = mat[1][0] * x + mat[1][1] * y + mat[1][2] * z;
                        var b = mat[2][0] * x + mat[2][1] * y + mat[2][2] * z;

                        if (r < (double)min)
                        {
                            r = 0.0;
                        }
                        else if (r >= (double)max)
                        {
                            r = 255.0;
                        }
                        else
                        {
                            r = (r - (double)min) / (double)(max - min) * 255.0;
                        }

                        if (g < (double)min)
                        {
                            g = 0.0;
                        }
                        else if (g >= (double)max)
                        {
                            g = 255.0;
                        }
                        else
                        {
                            g = (g - (double)min) / (double)(max - min) * 255.0;
                        }

                        if (b < (double)min)
                        {
                            b = 0.0;
                        }
                        else if (b >= (double)max)
                        {
                            b = 255.0;
                        }
                        else
                        {
                            b = (b - (double)min) / (double)(max - min) * 255.0;
                        }

                        *(dstPtr + 0) = (byte)b;
                        *(dstPtr + 1) = (byte)g;
                        *(dstPtr + 2) = (byte)r;
                        *(dstPtr + 3) = 255;

                        if (area is not null)
                        {
                            if (*maskPtr == 255)
                            {
                                *(dstPtr + 0) = (byte)b;
                                *(dstPtr + 1) = (byte)g;
                                *(dstPtr + 2) = (byte)r;
                                *(dstPtr + 3) = 255;
                            }
                            else
                            {
                                *(dstPtr + 0) = (byte)b;
                                *(dstPtr + 1) = (byte)g;
                                *(dstPtr + 2) = (byte)r;
                                *(dstPtr + 3) = 127;
                            }
                        }

                        dstPtr += 4;
                        srcPtr1++;
                        srcPtr2++;
                        srcPtr3++;
                        
                        if (area is not null)
                            maskPtr++;
                    }
                }
            }

            return new DisplayPixels(rgbPixels, width, height);
        }
    }

    public class ColorGradeReportFormat
    {
        public ColorGradeReportFormat()
        {

        }

        public int SerialNumber { get; }
        public ColorGrade ColorGrade { get; }
        public CIEXYZ XYZ { get; }
        public CIELCH LCH { get; }
        public CIELAB LAB { get; }
        public string MeasureDate { get; }
        public CIEXYZ Whitepoint { get; }
        public double Temperature { get; }
        public double Opticalpower { get; }
        public string FormatReport()
        {
            return $"Serial Number : {SerialNumber}\r\nColor Grade : {ColorGrade}\r\nL*C*h* : {LCH.ToString()}\r\nXYZ : {XYZ.ToString()}\r\nMeasure Date : {MeasureDate}\r\n";
        }

        public string FormatMeasureLog()
        {
            return $"{SerialNumber},{ColorGrade.ToString()},D65,{LCH.L},{LCH.C},{LCH.H},{XYZ.X},{XYZ.Y},{XYZ.Z},White,{Whitepoint.X},{Whitepoint.Y},{Whitepoint.Z},{MeasureDate}";
        }
    }

    public class DegreeOfContamination
    {
        public DegreeOfContamination(bool replacementTiming, double[] dirtRatio, WhitepointOnPtfeStage ptfestage, WhitepointOnSpectralon spectralon)
        {
            ReplacementTiming = replacementTiming;
            DirtRatio = dirtRatio;
            PtfeStage = ptfestage;
            Spectralon = spectralon;
        }

        public double[] DirtRatio { get; }
        public bool ReplacementTiming { get; }
        public WhitepointOnPtfeStage PtfeStage { get; }
        public WhitepointOnSpectralon Spectralon { get; }
        public class WhitepointOnPtfeStage
        {
            public WhitepointOnPtfeStage(CIEXYZ whitepoint, CIEXYZ whitepointOfCorner, CIEXYZ stddev)
            {
                Whitepoint = whitepoint;
                WhitepointOfCorner = whitepointOfCorner;
                StdDev = stddev;
            }
            public CIEXYZ Whitepoint { get; }
            public CIEXYZ WhitepointOfCorner { get; }
            public CIEXYZ StdDev { get; }
        }

        public class WhitepointOnSpectralon
        {
            public WhitepointOnSpectralon(CIEXYZ whitepoint, CIEXYZ whitepointOfCorner, CIEXYZ stddev)
            {
                Whitepoint = whitepoint;
                WhitepointOfCorner = whitepointOfCorner;
                StdDev = stddev;
            }
            public CIEXYZ Whitepoint { get; }
            public CIEXYZ WhitepointOfCorner { get; }
            public CIEXYZ StdDev { get; }
        }
    }
    //public record MeasurementEnviroment
    //{
    //    public MeasurementEnviroment(double temperature1, double temperature2, double opticalPower = 0.0)
    //    {
    //        TemperatureOnLED = temperature1;
    //        TemperatureOnDevice = temperature2;
    //        OpticalPower = opticalPower;
    //    }

    //    public double TemperatureOnLED
    //    {
    //        get;
    //    }
    //    public double TemperatureOnDevice
    //    {
    //        get;
    //    }
    //    public double OpticalPower
    //    {
    //        get;
    //    }
    //}
}
