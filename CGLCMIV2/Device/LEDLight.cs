using CGLCMIV2.Domain;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CGLCMIV2.Device
{
    public class LEDLight : ILEDLight
    {
        public LEDLight()
        {

        }

        SerialPort _ser = new SerialPort();
        double _opticalPower = 0.0;
        double _temperature = 0.0;
        object lockObject = new object();

        public void ChangeD65Value(int value)
        {

            if (value < 0 || value > 1023)
                throw new ArgumentException("out of range error.value range is 0 - 1023.");

            while (!Ready) { };
            Ready = false;

            lock (lockObject)
            {
                _ser.Write($"L1/{value}\n");
                while (_ser.BytesToWrite > 0) ;
                var res = _ser.ReadLine();
            }

            Ready = true;
        }

        public void ChangeUVValue(int value)
        {
            if (value < 0 || value > 1023)
                throw new ArgumentException("out of range error.value range is 0 - 1023.");
            
            while (!Ready) { };
            Ready = false;

            lock (lockObject)
            {
                _ser.Write($"L2/{value}\n");
                while (_ser.BytesToWrite > 0) ;
                var res = _ser.ReadLine();
            }

            Ready = true;
        }
        public void ReadStatus()
        {
            while (!Ready) { };
            Ready = false;

            lock (lockObject)
            {
                _ser.Write($"ST\n");
                while (_ser.BytesToWrite > 0) ;
                var res = _ser.ReadLine();
                var value = res.Split(' ');
                _temperature = double.Parse(value[0].Split(':')[1]);
                _opticalPower = double.Parse(value[2].Split(':')[1]);
            }

            Ready = true;
        }
        public double GetOpticalPower()
        {
            return _opticalPower;
        }

        public double GetTemperature()
        {
            return _temperature;
        }

        public void Start()
        {
            var name = GetDeviceNames()?.First();
            if(name is null)
                throw new ApplicationException("LEDLight inilialization error. device not found.");

            var m = System.Text.RegularExpressions.Regex.Match(name, "(COM[1-9][0-9]?[0-9]?)");
            //_ser = new SerialPort(m.Value, 9600);
            //_ser = new SerialPort();
            _ser.PortName = m.Value;
            _ser.BaudRate = 9600;
            _ser.NewLine = "\n";
            _ser.Open();

            IsConnected = true;
            Ready = true;

            string[] GetDeviceNames()
            {
                var deviceNameList = new System.Collections.ArrayList();
                //var check = new System.Text.RegularExpressions.Regex("(COM[1-9][0-9]?[0-9]?)");
                var check = new System.Text.RegularExpressions.Regex("VID_21A3&PID_2010");

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
            IsConnected = false;
            Ready = false;

            if (_ser.IsOpen)
            {
                _ser.DiscardInBuffer();
                _ser.DiscardOutBuffer();
                _ser.Close();
            }
            
        }

        public void StoreValues(int d65, int uv)
        {
            if (d65 < 0 || d65 > 1023)
                throw new ArgumentException("out of range error.value range is 0 - 1023.");
            
            if (uv < 0 || uv > 1023)
                throw new ArgumentException("out of range error.value range is 0 - 1023.");

            while (!Ready) { };
            Ready = false;

            lock (lockObject)
            {
                _ser.Write($"LS/{d65},{uv}\n");
                while (_ser.BytesToWrite > 0) ;
                var res = _ser.ReadLine();
            }

            Ready = true;
        }

        public bool IsConnected
        {
            get; private set;
        } = false;
        bool Ready { get; set; } = true;
    }
}
