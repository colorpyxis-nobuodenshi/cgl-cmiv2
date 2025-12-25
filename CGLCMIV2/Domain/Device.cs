using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGLCMIV2.Domain
{
    public enum CameraStatus
    {
        Stopped,
        Started,
        TakePicture,
    }
    public enum AutoStageStatus
    {
        Stopped,
        Started,
        Busy,
    }
    public enum LEDLightStatus
    {
        Stopped,
        Started,
        Busy,
    }
    public class CameraExposureTime
    {
        public static int EXPOSURE1 = 0;//0.01sec
        public static int EXPOSURE2 = 1;//0.016sec
        public static int EXPOSURE3 = 2;//0.033sec
        public static int EXPOSURE4 = 3;//0.066sec
    }
    public interface IHardware { }
    public interface ICamera : IHardware
    {
        XYZPixels TakePicture(int exposureTime, int integration);
        void Start();
        void Stop();
        bool IsConnected { get; }
    }

    public interface IAutoStage : IHardware
    {
        void MoveHome();
        void MoveMechanicalHome();
        void MoveMeasurePoint();
        void MoveWorkSetPoint();
        void Move(int step);
        void RotateCW45();
        void RotateCCW45();
        void RotateCWJ();
        void RotateCCWJ();
        void Rotate(int pos);
        void Start();
        void Stop();
        bool IsConnected { get; }
    }

    public interface ILEDLight : IHardware
    {
        void ChangeD65Value(int value);
        void ChangeUVValue(int value);
        void StoreValues(int d65, int uv);
        double GetTemperature();
        double GetOpticalPower();
        void ReadStatus();
        void Start();
        void Stop();
        bool IsConnected { get; }
    }

}
