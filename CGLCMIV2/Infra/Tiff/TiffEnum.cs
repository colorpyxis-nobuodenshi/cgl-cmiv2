using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tiff
{
    public enum TagCode : ushort
    {
        NewSubfileType = 254,
        SubfileType = 255,
        ImageWidth = 256,
        ImageLength = 257,
        BitsPerSample = 258,
        Compression = 259,
        PhotometricInterpretation = 262,
        Threshholding = 263,
        CellWidth = 264,
        CellLength = 265,
        FillOrder = 266,
        DocumentName = 269,
        ImageDescription = 270,
        Make = 271,
        Model = 272,
        StripOffsets = 273,
        Orientation = 274,
        SamplesPerPixel = 277,
        RowsPerStrip = 278,
        StripByteCounts = 279,
        MinSampleValue = 280,
        MaxSampleValue = 281,
        XResolution = 282,
        YResolution = 283,
        PlanarConfiguration = 284,
        PageName = 285,
        XPosition = 286,
        YPosition = 287,
        FreeOffsets = 288,
        FreeByteCounts = 289,
        GrayResponseUnit = 290,
        GrayResponseCurve = 291,
        T4Options = 292,
        T6Options = 293,
        ResolutionUnit = 296,
        PageNumber = 297,
        TransferFunction = 301,
        Software = 305,
        DateTime = 306,
        Artist = 315,
        HostComputer = 316,
        Predictor = 317,
        WhitePoint = 318,
        PrimaryChromaticities = 319,
        ColorMap = 320,
        HalftoneHints = 321,
        TileWidth = 322,
        TileLength = 323,
        TileOffsets = 324,
        TileByteCounts = 325,
        InkSet = 332,
        InkNames = 333,
        NumberOfInks = 334,
        DotRange = 336,
        TargetPrinter = 337,
        ExtraSamples = 338,
        SampleFormat = 339,
        SMinSampleValue = 340,
        SMaxSampleValue = 341,
        TransferRange = 342,
        JPEGProc = 512,
        JPEGInterchangeFormat = 513,
        JPEGInterchangeFormatLngth = 514,
        JPEGRestartInterval = 515,
        JPEGLosslessPredictors = 517,
        JPEGPointTransforms = 518,
        JPEGQTables = 519,
        JPEGDCTables = 520,
        JPEGACTables = 521,
        YCbCrCoefficients = 529,
        YCbCrSubSampling = 530,
        YCbCrPositioning = 531,
        ReferenceBlackWhite = 532,
        Copyright = 33432,
        //-2014/04/17 zhang-s
        ImageType = 65534,
        //-2014/04/17 zhang-e
        ColorSpace = 65535,
    }

    public enum TagType : ushort
    {
        Byte = 1,
        Ascii = 2,
        Short = 3,
        Long = 4,
        Rational = 5,
        SByte = 6,
        Undefined = 7,
        SShort = 8,
        SLong = 9,
        SRational = 10,
        Float = 11,
        Double = 12
    }

    public enum TagTypeAllocation : byte
    {
        Byte = 1,
        Ascii = 1,
        Short = 2,
        Long = 4,
        Rational = 8,
        SByte = 1,
        Undefined = 1,
        SShort = 2,
        SLong = 4,
        SRational = 8,
        Float = 4,
        Double = 8
    }

    public enum Compression : uint
    {
        Uncompressed = 1,
        CCITT1D = 2,
        Group3Fax = 3,
        Group4Fax = 4,
        LZW = 5,
        JPEG = 6,
        PackBits = 32773
    }

    public enum PhotometricInterpretation : uint
    {
        WhiteIsZero = 0,
        BlackIsZero = 1,
        RGB = 2,
        RGBPalette = 3,
        TransparencyMask = 4,
        CMYK = 5,
        YCbCr = 6,
        CIELab = 8
    }

    public enum SampleFormat : ushort
    {
        UInt = 1,
        Int = 2,
        IEEEFP = 3,
        Void = 4,
        ComplexInt = 5,
        ComplexIEEEFP = 6,
    }

    public enum ColorSpace : ushort
    {
        Raw = 1,
        XYZ = 2,
        Yxy = 3,
        Lab = 4,
        Luv = 5,
    }
}
