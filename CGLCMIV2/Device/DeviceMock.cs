using CGLCMIV2.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGLCMIV2.Device
{
    public class CameraMock : ICamera
    {
        public bool IsConnected { get; private set; } = false;
        public void Start()
        {
            IsConnected = true;
            Console.WriteLine("camera start.");
        }

        public void Stop()
        {
            IsConnected = false;
            Console.WriteLine("camera stop.");
        }

        public XYZPixels TakePicture(int exposureTime, int integration)
        {
            Console.WriteLine("camera take picture.");
            return new XYZPixels(new ushort[1024 * 768 * 3], 1024, 768);
        }
    }

    public class AutoStageMock : IAutoStage
    {
        public bool IsConnected { get; private set; } = false;
        public void MoveHome()
        {
            Console.WriteLine("autostage move home.");
        }

        public void MoveMechanicalHome()
        {
            Console.WriteLine("autostage move mechanical home.");
        }

        public void Move(int step)
        {
            Console.WriteLine("autostage move step.");
        }

        public void MoveMeasurePoint()
        {
            Console.WriteLine("autostage move measure point.");
        }

        public void MoveWorkSetPoint()
        {
            Console.WriteLine("autostage move work set point.");
        }

        public void RotateCW45()
        {
            Console.WriteLine("autostage rotate 45 degree.");
        }
        public void RotateCCW45()
        {
            Console.WriteLine("autostage rotate 45 degree.");
        }
        public void RotateCCWJ()
        {
            throw new NotImplementedException();
        }

        public void RotateCWJ()
        {
            throw new NotImplementedException();
        }

        public void Start()
        {
            IsConnected = true;
            Console.WriteLine("autostage start.");
        }

        public void Stop()
        {
            IsConnected = false;
            Console.WriteLine("autostage stop.");
        }

        public void Rotate(int pos)
        {
            throw new NotImplementedException();
        }

        public void MoveReplacementPoint()
        {
            Console.WriteLine("autostage move replacement point.");
        }

        public void MoveMeasurePointOnSpectralon()
        {
            Console.WriteLine("autostage move measure point on spectralon.");
        }
    }

    public class LEDControllerMock : ILEDLight
    {
        public bool IsConnected => true;

        public void ChangeD65Value(int value)
        {
            Console.WriteLine($"ledlight ledcalue change {value}.");
        }

        public void ChangeUVValue(int value)
        {
            Console.WriteLine($"ledlight ledcalue change {value}.");
        }

        public double GetOpticalPower()
        {
            Console.WriteLine($"ledlight get optivalpower.");
            return 100;
        }

        public double GetTemperature()
        {
            Console.WriteLine($"ledlight get temperature.");
            var r = new Random();
            return r.Next(20, 30);
        }
        public void ReadStatus()
        {

        }
        public void Start()
        {
            Console.WriteLine($"ledlight start.");
        }

        public void Stop()
        {
            Console.WriteLine($"ledlight stop.");
        }

        public void StoreValues(int d65, int uv)
        {
            throw new NotImplementedException();
        }
    }
}
