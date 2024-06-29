using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using CGLCMIV2.Domain;
using ic4;
using OpenCvSharp;

namespace ConsoleApp1
{
    public interface IHardware { }
    public interface ICamera : IHardware
    {
        XYZPixels TakePicture(int exposureTime, int integration);
        void Start();
        void Stop();
        bool IsConnected { get; }
    }

    public class DFK33UX264 : ICamera
    {
        ic4.Grabber? _grabber;
        ic4.SnapSink? _sink;

        public DFK33UX264()
        {
            ic4.Library.Init();
            _grabber = new ic4.Grabber();
            _sink = new ic4.SnapSink();

        }
        ~DFK33UX264()
        {
            Stop();
        }

        public bool IsConnected { get; private set; }

        const int EFFECTIVE_IMAGE_WIDTH = 2448;
        const int EFFECTIVE_IMAGE_HEIGHT = 2048;
        const int FRAME_SIZE = EFFECTIVE_IMAGE_WIDTH * EFFECTIVE_IMAGE_HEIGHT * 3;

        public void Start()
        {
            var firstDevInfo = ic4.DeviceEnum.Devices.First();
            _grabber?.DeviceOpen(firstDevInfo);

            _grabber?.DevicePropertyMap.DeSerialize("camera.config.json");
            //_grabber?.DevicePropertyMap.SetValue(ic4.PropId.UserSetSelector, "UserSet1");
            //_grabber?.DevicePropertyMap.ExecuteCommand(ic4.PropId.UserSetLoad);
            //_grabber?.DevicePropertyMap.SetValue(ic4.PropId.PixelFormat, "BayerRG16");
            //_grabber?.DevicePropertyMap.SetValue(ic4.PropId.Width, 2448);
            //_grabber?.DevicePropertyMap.SetValue(ic4.PropId.Height, 2048);
            //_grabber?.DevicePropertyMap.SetValue(ic4.PropId.ExposureAuto, "Off");
            //_grabber?.DevicePropertyMap.SetValue(ic4.PropId.ExposureTime, 10000.0);
            //_grabber?.DevicePropertyMap.SetValue(ic4.PropId.GainAuto, "Continuous");
            //_grabber?.DevicePropertyMap.SetValue(ic4.PropId.BalanceWhiteAuto, "Off");
            //_grabber?.DevicePropertyMap.SetValue(ic4.PropId.BalanceWhiteMode, "WhiteBalanceMode_Temperature");
            //_grabber?.DevicePropertyMap.SetValue(ic4.PropId.BalanceWhiteAutoPreset, "BalanceWhiteAutoPreset_Any");
            //_grabber?.DevicePropertyMap.SetValue(ic4.PropId.BalanceWhiteTemperaturePreset, "BalanceWhiteTemperaturePreset_Daylight");
            //_grabber?.DevicePropertyMap.SetValue(ic4.PropId.AutoFunctionsROIEnable, false);
            //_grabber?.DevicePropertyMap.Serialize("camera.config.json");
            
            _grabber?.StreamSetup(_sink, ic4.StreamSetupOption.AcquisitionStart);
            IsConnected = true;
        }

        public void Stop()
        {
            _grabber?.StreamStop();
            IsConnected = false;
        }

        public XYZPixels TakePicture(int exposureTime, int integration)
        {
            _grabber?.AcquisitionStop();
            _grabber?.DevicePropertyMap.SetValue(ic4.PropId.ExposureTime, exposureTime);
            _grabber?.AcquisitionStart();

            var image = _sink?.SnapSingle(TimeSpan.FromSeconds(1));
            image.SaveAsTiff("temp.tiff");
            //image.SaveAsTiff("temp1.tiff", true);
            var mat = image.CreateOpenCvWrap();
            //mat = mat * (4095.0 / 65535.0);
            mat = mat.CvtColor(ColorConversionCodes.BayerRG2RGB, 3);
            var pix = new ushort[FRAME_SIZE];
            
            mat.ImWrite("temp2.tiff");
            
            unsafe
            {
                var len = EFFECTIVE_IMAGE_WIDTH * EFFECTIVE_IMAGE_HEIGHT;
                var srcPtr = (ushort*)mat.DataPointer;
                fixed (ushort* dstPtr = pix)
                {
                    var dstPtr1 = dstPtr;
                    var dstPtr2 = dstPtr + len;
                    var dstPtr3 = dstPtr + len * 2;
                    for (var i = 0; i < len; i++)
                    {
                        *dstPtr1 = *(srcPtr + 2);
                        *dstPtr2 = *(srcPtr + 1);
                        *dstPtr3 = *(srcPtr + 0);
                        dstPtr1++;
                        dstPtr2++;
                        dstPtr3++;
                        srcPtr += 3;
                    }
                }
            }
            //unsafe
            //{
            //    var ptr = (ushort*)mat.DataPointer;
            //    for (var i = 0; i < FRAME_SIZE; i++)
            //    {
            //        pix[i] = *ptr++;
            //    }
            //}

            //using (var sw = new BinaryWriter(new FileStream("pix.dat", FileMode.OpenOrCreate)))
            //{
            //    foreach(var p in pix)
            //    {
            //        sw.Write(p);
            //    }
            //}
            return new XYZPixels(pix, EFFECTIVE_IMAGE_WIDTH, EFFECTIVE_IMAGE_HEIGHT);
        }
    }
}
