using CGLCMIV2.Domain;
using System;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;

namespace CGLCMIV2.Device
{
    public class Camera : ICamera
    {
        RTC21 _rtc21 = new RTC21();
        
        public Camera()
        {

        }
        public bool IsConnected { get; private set; } = false;
        bool Capturing { get; set; } = false;
        int ExposureTimeCache { get; set; } = -9;
        public void Start()
        {
            var name = GetDeviceNames()?.First();
            if(name is null)
            {
                throw new ApplicationException("camera inilialization error. device not found.");
            }
            _rtc21.Initialize();
            _rtc21.Reset();
            TakePicture(1, 1);
            IsConnected = true;

            string[] GetDeviceNames()
            {
                var deviceNameList = new System.Collections.ArrayList();
                var check = new System.Text.RegularExpressions.Regex("VID_12C1&PID_3130");

                var mcPnPEntity = new ManagementClass("Win32_PnPEntity");
                var manageObjCol = mcPnPEntity.GetInstances();


                foreach (var manageObj in manageObjCol)
                {
                    var devicePropertyValue = manageObj.GetPropertyValue("DeviceID");
                    if (devicePropertyValue == null)
                    {
                        continue;
                    }


                    var id = devicePropertyValue.ToString();
                    if (check.IsMatch(id))
                    {

                        var name = manageObj.GetPropertyValue("Name").ToString();


                        deviceNameList.Add(name);
                    }
                }

                if (deviceNameList.Count > 0)
                {
                    string[] deviceNames = new string[deviceNameList.Count];
                    int index = 0;
                    foreach (var name in deviceNameList)
                    {
                        deviceNames[index++] = name.ToString();
                    }
                    return deviceNames;
                }
                else
                {
                    return null;
                }
            }
        }

        public void Stop()
        {
            while (Capturing) { };
            IsConnected = false;
            _rtc21.Deinitialize();
        }

        public XYZPixels TakePicture(int exposureTime, int integration)
        {

            try
            {


                while (Capturing) { };
                Capturing = true;

                ChangeExposureTime(exposureTime);

                var p = _rtc21.TakeCapture(integration);

                //Capturing = false;

                return new XYZPixels(p, _rtc21.EffectiveSizeX, _rtc21.EffectiveSizeY);
            }
            catch
            {
                throw;
            }
            finally
            {
                Capturing = false;
            }
        }
        
        void ChangeExposureTime(int exposureTime)
        {
            if (exposureTime == ExposureTimeCache)
                return;
            
            ExposureTimeCache = exposureTime;

            //var b = new byte[5];

            //switch (exposureTime)
            //{
            //    case 0://0.066sec
            //        b = new byte[] { 0x30, 0x34, 0x30, 0x30, 0x30 };
            //        break;
            //    case 1://0.033sec
            //        b = new byte[] { 0x30, 0x34, 0x30, 0x30, 0x31 };
            //        break;
            //    case 2://0.016sec
            //        b = new byte[] { 0x30, 0x34, 0x30, 0x30, 0x32 };
            //        break;
            //    case 3://0.001sec
            //        b = new byte[] { 0x30, 0x34, 0x30, 0x30, 0x33 };
            //        break;
            //}
            //_rtc21.SendRecvCommand(b);
            switch (exposureTime)
            {
                case 2:// 1/15 sec
                    _rtc21.SendRecvCommand(new byte[] { 0x30, 0x32, 0x30, 0x30, 0x30 });
                    _rtc21.SendRecvCommand(new byte[] { 0x30, 0x35, 0x30, 0x30, 0x30 });
                    break;
                case 3:// 1/30 sec
                    _rtc21.SendRecvCommand(new byte[] { 0x30, 0x32, 0x30, 0x30, 0x30 });
                    _rtc21.SendRecvCommand(new byte[] { 0x30, 0x35, 0x30, 0x30, 0x30 });
                    break;
                case 1:// 1/7.5 sec
                    _rtc21.SendRecvCommand(new byte[] { 0x30, 0x32, 0x30, 0x30, 0x33 });
                    _rtc21.SendRecvCommand(new byte[] { 0x30, 0x35, 0x30, 0x30, 0x31 });
                    break;
                case 0:// 1/4 sec
                    _rtc21.SendRecvCommand(new byte[] { 0x30, 0x32, 0x30, 0x30, 0x33 });
                    _rtc21.SendRecvCommand(new byte[] { 0x30, 0x35, 0x30, 0x30, 0x32 });
                    break;
            }
        }
    }
    internal class RTC21
    {
        [Flags]
        enum RTCError
        {
            RTC_ERROR_NONE = 0,
            RTC_ERROR_GRABBER_NOT_FOUND,
            RTC_ERROR_GRABBER_NOT_AVAILABLE,
            RTC_ERROR_ILLEGAL_CAMERA_CONFIG,
            RTC_ERROR_FAILED_CAMERA_CONFIG,
            RTC_ERROR_FAILED_ALLOCATE_MEMORY,
            RTC_ERROR_FAILED_ALOCATE_MEMORY,
            RTC_ERROR_MISC,
            RTC_ERROR_FAILED
        };

        [Flags]
        enum GrabberBoard
        {
            MATROX = 0,
            EPIX,
            PLEORA,
            RTCUSB,
            RTC_NUM_OF_GRABBER_BOARD
        };
        public delegate void CaptureCallback(IntPtr args);

        [DllImport("rtcApi_x64.dll", EntryPoint = "RTC_InitializeGrabberBoard", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        static extern RTCError RTC_InitializeGrabberBoard(int boardNumber, int cameraNumber, string cameraFileName, ref int width, ref int height, GrabberBoard grabberboard);
        [DllImport("rtcApi_x64.dll", EntryPoint = "RTC_DeinitializeGrabberBoard", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        static extern void RTC_DeinitializeGrabberBoard();
        [DllImport("rtcApi_x64.dll", EntryPoint = "RTC_StartCapture", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        static extern RTCError RTC_StartCapture(CaptureCallback callback, IntPtr parameter);
        [DllImport("rtcApi_x64.dll", EntryPoint = "RTC_CaptureFrame", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        static extern void RTC_CaptureFrame(IntPtr frameBuffer);
        [DllImport("rtcApi_x64.dll", EntryPoint = "RTC_StopCapture", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        static extern void RTC_StopCapture();
        [DllImport("rtcApi_x64.dll", EntryPoint = "RTC_AllocateFrameBufferN", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        static extern int RTC_AllocateFrameBufferN(ref IntPtr frameBuffer, int width, int height, int frames);
        [DllImport("rtcApi_x64.dll", EntryPoint = "RTC_AllocateFrameBuffer", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        static extern int RTC_AllocateFrameBuffer(ref IntPtr frameBuffer, int width, int height);
        [DllImport("rtcApi_x64.dll", EntryPoint = "RTC_FreeFrameBuffer", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        static extern void RTC_FreeFrameBuffer(IntPtr frameBuffer);

        [DllImport("rtcApi_x64.dll", EntryPoint = "RTC_ClserInitializeSerial", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        static extern IntPtr RTC_ClserInitializeSerial(string filename, ulong serialIndex, string errmsg);
        [DllImport("rtcApi_x64.dll", EntryPoint = "RTC_ClserCloseSerial", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        static extern void RTC_ClserCloseSerial(IntPtr handle);

        [DllImport("rtcApi_x64.dll", EntryPoint = "RTC_SendCommand", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        static extern int RTC_SendCommand(byte[] command, ulong msecTimeout, IntPtr clserRef);
        [DllImport("rtcApi_x64.dll", EntryPoint = "RTC_RecvCommand", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        static extern int RTC_RecvCommand(byte[] command, ulong msecTimeout, IntPtr clserRef);


        const int EFFECTIVE_IMAGE_WIDTH = 2048;
        const int EFFECTIVE_IMAGE_HEIGHT = 1536;
        const int FRAME_SIZE = EFFECTIVE_IMAGE_WIDTH * EFFECTIVE_IMAGE_HEIGHT * 3;

        IntPtr _ser = IntPtr.Zero;
        string _serError = string.Empty;
        IntPtr _frameBuffer = IntPtr.Zero;
        public RTC21()
        {

        }

        public int EffectiveSizeX
        {
            get
            {
                return EFFECTIVE_IMAGE_WIDTH / 2;
            }
        }

        public int EffectiveSizeY
        {
            get
            {
                return EFFECTIVE_IMAGE_HEIGHT / 2;
            }
        }
        
        public void Reset()
        {
            SendRecvCommand(new byte[] { 0x31, 0x31, 0x32, 0x30, 0x30 });
            SendRecvCommand(new byte[] { 0x31, 0x32, 0x30, 0x30, 0x30 });
            SendRecvCommand(new byte[] { 0x31, 0x33, 0x30, 0x30, 0x30 });
            SendRecvCommand(new byte[] { 0x32, 0x31, 0x30, 0x30, 0x30 });
            SendRecvCommand(new byte[] { 0x32, 0x32, 0x32, 0x30, 0x30 });
            SendRecvCommand(new byte[] { 0x32, 0x33, 0x30, 0x30, 0x30 });
            SendRecvCommand(new byte[] { 0x33, 0x31, 0x30, 0x30, 0x30 });
            SendRecvCommand(new byte[] { 0x33, 0x32, 0x30, 0x30, 0x30 });
            SendRecvCommand(new byte[] { 0x33, 0x33, 0x32, 0x30, 0x30 });
            SendRecvCommand(new byte[] { 0x30, 0x34, 0x30, 0x30, 0x31 });

            //SendRecvCommand(new byte[] { 0x43, 0x31, 0x30, 0x30, 0x30 });
        }

        public void ResetRegister()
        {
            SendRecvCommand(new byte[] { 0x31, 0x31, 0x32, 0x30, 0x30 });
            SendRecvCommand(new byte[] { 0x31, 0x32, 0x30, 0x30, 0x30 });
            SendRecvCommand(new byte[] { 0x31, 0x33, 0x30, 0x30, 0x30 });
            SendRecvCommand(new byte[] { 0x32, 0x31, 0x30, 0x30, 0x30 });
            SendRecvCommand(new byte[] { 0x32, 0x32, 0x32, 0x30, 0x30 });
            SendRecvCommand(new byte[] { 0x32, 0x33, 0x30, 0x30, 0x30 });
            SendRecvCommand(new byte[] { 0x33, 0x31, 0x30, 0x30, 0x30 });
            SendRecvCommand(new byte[] { 0x33, 0x32, 0x30, 0x30, 0x30 });
            SendRecvCommand(new byte[] { 0x33, 0x33, 0x32, 0x30, 0x30 });
            SendRecvCommand(new byte[] { 0x30, 0x34, 0x30, 0x30, 0x31 });

            //SendRecvCommand(new byte[] { 0x43, 0x31, 0x30, 0x30, 0x30 });
        }
        public void Initialize()
        {
            int width = 0;
            int height = 0;

            if (RTC_InitializeGrabberBoard(0, 0, "", ref width, ref height, GrabberBoard.RTCUSB) != RTCError.RTC_ERROR_NONE)
            {
                throw new ApplicationException("Camera initialization error.");
            }
            if(width != EFFECTIVE_IMAGE_WIDTH)
            {
                throw new ApplicationException("Camera initialization error.");
            }
            if(height != EFFECTIVE_IMAGE_HEIGHT)
            {
                throw new ApplicationException("Camera initialization error.");
            }
            if(RTC_AllocateFrameBufferN(ref _frameBuffer, EFFECTIVE_IMAGE_WIDTH, EFFECTIVE_IMAGE_HEIGHT, 1) == 0)
            {
                throw new OutOfMemoryException("memory allocate exeption.");
            }
            _ser = RTC_ClserInitializeSerial("ArtCamSdk_CnvU3I.dll", 0, _serError);
            if (_ser == IntPtr.Zero)
            {
                throw new ApplicationException("Camera communication port initialization error.");
            }
            
            Reset();

            //TakeCapture(1);
        }

        public void Deinitialize()
        {
            RTC_FreeFrameBuffer(_frameBuffer);

            RTC_DeinitializeGrabberBoard();

            RTC_ClserCloseSerial(_ser);
        }

        public byte[] SendRecvCommand(byte[] command)
        {
            if (RTC_SendCommand(command, 5000, _ser) <= 0)
            {
                throw new ApplicationException("Camera communication error.");
            }
            var recv = new byte[8];
            if (RTC_RecvCommand(recv, 5000, _ser) <= 0)
            {
                throw new ApplicationException("Camera communication error.");
            }
            if(recv[0] == 0x15)
            {
                throw new ApplicationException("Camera communication error.");
            }
            return recv;
        }

        unsafe public ushort[] TakeCapture(int integration, int timeout = 30000)
        {
            var captured = false;
            var p = new ushort[FRAME_SIZE];
            var count = 0;
            IntPtr tmp = _frameBuffer;

            // if(RTC_AllocateFrameBufferN(ref tmp, EFFECTIVE_IMAGE_WIDTH, EFFECTIVE_IMAGE_HEIGHT, 1) == 0)
            // {
            //     throw new OutOfMemoryException("memory allocate exeption.");
            // }

            var callback = new CaptureCallback((_) =>
            {
                RTC_CaptureFrame(tmp);
                fixed(ushort* dstPtr = p)
                {
                    var srcPtr = (ushort*)tmp.ToPointer();
                    for(var i=0;i<FRAME_SIZE;i++)
                    {
                        dstPtr[i] += srcPtr[i];
                    }
                }
                count++;
                if (count == integration)
                {
                    RTC_StopCapture();
                    captured = true;
                }
            });

            RTC_StartCapture(callback, IntPtr.Zero);

            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            while (!captured) 
            {
                System.Threading.Tasks.Task.Delay(10);
                if(sw.ElapsedMilliseconds > timeout)
                {
                    sw.Stop();
                    RTC_StopCapture();
                    throw new ApplicationException("Camera capture error.");
                }
            };
            sw.Stop();
            fixed (ushort* ptr = p)
            {
                for (var i = 0; i < FRAME_SIZE; i++)
                {
                    ptr[i] /= (ushort)integration;
                }
            }

            // RTC_FreeFrameBuffer(tmp);

            var len = EFFECTIVE_IMAGE_WIDTH * EFFECTIVE_IMAGE_HEIGHT;
            var p2 = new ushort[FRAME_SIZE];
            
            fixed (ushort* srcPtr = p)
            fixed (ushort* dstPtr = p2)
            {
                var srcPtr1 = srcPtr;
                var dstPtr1 = dstPtr;
                var dstPtr2 = dstPtr + len;
                var dstPtr3 = dstPtr + len * 2;
                for (var i = 0; i < len; i++)
                {
                    *(dstPtr1) = *(srcPtr1 + 0);
                    *(dstPtr2) = *(srcPtr1 + 1);
                    *(dstPtr3) = *(srcPtr1 + 2);
                    dstPtr1++;
                    dstPtr2++;
                    dstPtr3++;
                    srcPtr1 += 3;
                }
            }

            return CorrectBinning2x2(p2);
        }
    
           
        private ushort[] CorrectBinning2x2(ushort[] value)
        {
            var src = value;
            var dst = new ushort[EFFECTIVE_IMAGE_WIDTH / 2 * EFFECTIVE_IMAGE_HEIGHT / 2 * 3];
            //
            unsafe
            {
                var w = EFFECTIVE_IMAGE_WIDTH;
                var h = EFFECTIVE_IMAGE_HEIGHT;
                var w2 = EFFECTIVE_IMAGE_WIDTH / 2;
                var h2 = EFFECTIVE_IMAGE_HEIGHT / 2;
                var threshold = (ushort)1023;

                fixed (ushort* srcBase = src)
                fixed (ushort* dstBase = dst)
                {
                    var srcPtr1 = srcBase + w * h * 0;
                    var srcPtr2 = srcBase + w * h * 1;
                    var srcPtr3 = srcBase + w * h * 2;
                    var dstPtr1 = dstBase + w2 * h2 * 0;
                    var dstPtr2 = dstBase + w2 * h2 * 1;
                    var dstPtr3 = dstBase + w2 * h2 * 2;

                    for (var y = 0; y < h - 1; y += 2)
                    {
                        for (var x = 0; x < w - 1; x += 2)
                        {
                            var p1 = *(srcPtr1 + x + y * w);
                            var p2 = *(srcPtr1 + (x + 1) + y * w);
                            var p3 = *(srcPtr1 + x + (y + 1) * w);
                            var p4 = *(srcPtr1 + (x + 1) + (y + 1) * w);
                            var p = (ushort)((p1 + p2 + p3 + p4) / 4);
                            p = p > threshold ? threshold : p;

                            *(dstPtr1 + (x / 2) + (y / 2) * w2) = p;

                            p1 = *(srcPtr2 + x + y * w);
                            p2 = *(srcPtr2 + (x + 1) + y * w);
                            p3 = *(srcPtr2 + x + (y + 1) * w);
                            p4 = *(srcPtr2 + (x + 1) + (y + 1) * w);
                            p = (ushort)((p1 + p2 + p3 + p4) / 4);
                            p = p > threshold ? threshold : p;

                            *(dstPtr2 + (x / 2) + (y / 2) * w2) = p;

                            p1 = *(srcPtr3 + x + y * w);
                            p2 = *(srcPtr3 + (x + 1) + y * w);
                            p3 = *(srcPtr3 + x + (y + 1) * w);
                            p4 = *(srcPtr3 + (x + 1) + (y + 1) * w);
                            p = (ushort)((p1 + p2 + p3 + p4) / 4);
                            p = p > threshold ? threshold : p;

                            *(dstPtr3 + (x / 2) + (y / 2) * w2) = p;


                        }
                    }
                }
            }

            return dst;
        }

    }
}
