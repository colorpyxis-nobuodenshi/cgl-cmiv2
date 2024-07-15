using CGLCMIV2.Domain;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace CGLCMIV2.Device
{
    public class DS112 : IAutoStage
    {
        SerialPort _ser = new SerialPort();
        public DS112()
        {
            
        }
        
        ~DS112()
        {
            
        }
        public bool IsConnected { get; set; }
        bool Ready { get; set; } = true;
        

        public void MoveHome()
        {
            _ser.WriteLine("AXI1:SELSP 0:GOABS 10000");
            while (_ser.BytesToWrite > 0) ;

            _ser.WriteLine("AXI2:SELSP 1:GOABS 0");
            while (_ser.BytesToWrite > 0) ;

            while (true)
            {
                _ser.DiscardInBuffer();
                _ser.WriteLine("AXI1:POS?");
                while (_ser.BytesToWrite > 0) ;
                while (_ser.BytesToRead < 1) ;
                var res = _ser.ReadLine();
                if (res == "10000")
                    break;
                Task.Delay(500);
            }

            while (true)
            {
                _ser.DiscardInBuffer();
                _ser.WriteLine("AXI2:POS?");
                while (_ser.BytesToWrite > 0) ;
                while (_ser.BytesToRead < 1) ;
                var res = _ser.ReadLine();
                if (res == "0.000")
                    break;
                Task.Delay(500);
            }

        }

        public void MoveMechanicalHome()
        {
            _ser.WriteLine("AXI1:SELSP 0:GO ORG");
            while (_ser.BytesToWrite > 0);
            

            _ser.WriteLine("AXI2:SELSP 1:GO ORG");
            while (_ser.BytesToWrite > 0) ;

            while(true)
            {
                _ser.DiscardInBuffer();
                _ser.WriteLine("AXI1:ORG?");
                while (_ser.BytesToWrite > 0) ;
                while (_ser.BytesToRead < 1) ;
                var res = _ser.ReadLine();
                if (res == "1")
                    break;
                Task.Delay(100);
            }
            while (true)
            {
                _ser.DiscardInBuffer();
                _ser.WriteLine("AXI2:ORG?");
                while (_ser.BytesToWrite > 0) ;
                while (_ser.BytesToRead < 1) ;
                var res = _ser.ReadLine();
                if (res == "1")
                    break;
                Task.Delay(100);
            }

        }

        public void MoveMeasurePoint()
        {
            _ser.WriteLine("AXI1:SELSP 0:GOABS -5000");
            while (_ser.BytesToWrite > 0) ;
            while (true)
            {
                _ser.DiscardInBuffer();
                _ser.WriteLine("AXI1:POS?");
                while (_ser.BytesToWrite > 0) ;
                while (_ser.BytesToRead < 1) ;
                var res = _ser.ReadLine();
                if (res == "-5000")
                    break;
                Task.Delay(100);
            }
        }

        public void MoveWorkSetPoint()
        {
            _ser.WriteLine("AXI1:SELSP 0:GOABS 10000");
            while (_ser.BytesToWrite > 0) ;
            while (true)
            {
                _ser.DiscardInBuffer();
                _ser.WriteLine("AXI1:POS?");
                while (_ser.BytesToWrite > 0) ;
                while (_ser.BytesToRead < 1) ;
                var res = _ser.ReadLine();
                if (res == "10000")
                    break;
                Task.Delay(100);
            }
        }

        public void MoveReplacementPoint()
        {
            _ser.WriteLine("AXI1:SELSP 0:GOABS 9000");
            while (_ser.BytesToWrite > 0) ;
            while (true)
            {
                _ser.DiscardInBuffer();
                _ser.WriteLine("AXI1:POS?");
                while (_ser.BytesToWrite > 0) ;
                while (_ser.BytesToRead < 1) ;
                var res = _ser.ReadLine();
                if (res == "9000")
                    break;
                Task.Delay(100);
            }
        }

        public void MoveMeasurePointOnSpectralon()
        {
            _ser.WriteLine("AXI1:SELSP 0:GOABS 1500");
            while (_ser.BytesToWrite > 0) ;
            while (true)
            {
                _ser.DiscardInBuffer();
                _ser.WriteLine("AXI1:POS?");
                while (_ser.BytesToWrite > 0) ;
                while (_ser.BytesToRead < 1) ;
                var res = _ser.ReadLine();
                if (res == "1500")
                    break;
                Task.Delay(100);
            }
        }

        public void Move(int step)
        {
            throw new NotImplementedException();
        }

        public void RotateCW45()
        {
            _ser.WriteLine("AXI2:SELSP 1:PULS 45:GO CW");
            while (_ser.BytesToWrite > 0) ;
            
            while (true)
            {
                _ser.DiscardInBuffer();
                _ser.WriteLine("AXI2:MOTION?");
                while (_ser.BytesToWrite > 0) ;
                while (_ser.BytesToRead < 1) ;
                var res = _ser.ReadLine();
                if (res == "1")
                    break;
                Task.Delay(100);
            }
            
            Task.Delay(100);

            while (true)
            {
                _ser.DiscardInBuffer();
                _ser.WriteLine("AXI2:MOTION?");
                while (_ser.BytesToWrite > 0);
                while (_ser.BytesToRead < 1) ;
                var res = _ser.ReadLine();
                if (res == "0")
                    break;
                Task.Delay(100);
            }

        }
        public void RotateCCW45()
        {
            _ser.WriteLine("AXI2:SELSP 1:PULS 45:GO CCW");
            while (_ser.BytesToWrite > 0) ;
            
            while (true)
            {
                _ser.DiscardInBuffer();
                _ser.WriteLine("AXI2:MOTION?");
                while (_ser.BytesToWrite > 0) ;
                while (_ser.BytesToRead < 1) ;
                var res = _ser.ReadLine();
                if (res == "1")
                    break;
                Task.Delay(100);
            }

            Task.Delay(100);

            while (true)
            {
                _ser.DiscardInBuffer();
                _ser.WriteLine("AXI2:MOTION?");
                while (_ser.BytesToWrite > 0) ;
                while (_ser.BytesToRead < 1) ;
                var res = _ser.ReadLine();
                if (res == "0")
                    break;
                Task.Delay(100);
            }

        }
        public void RotateCWJ()
        {
            _ser.WriteLine("AXI2:SELSP 1:PULS 5:GO CW");
            while (_ser.BytesToWrite > 0) ;
        }

        public void RotateCCWJ()
        {
            _ser.WriteLine("AXI2:SELSP 1:PULS 5:GO CCW");
            while (_ser.BytesToWrite > 0) ;
        }

        public void Start()
        {
            var name = GetDeviceNames()?.First();
            if (name is null)
                throw new ApplicationException("AutoStage inilialization error. device not found.");

            var m = System.Text.RegularExpressions.Regex.Match(name, "(COM[1-9][0-9]?[0-9]?)");
            //_ser = new SerialPort(m.Value, 38400);
            //_ser = new SerialPort();
            _ser.PortName = m.Value;
            _ser.BaudRate = 38400;
            _ser.NewLine = "\r";
            _ser.ReadTimeout = 1000;
            _ser.WriteTimeout = 1000;
            _ser.Open();

            IsConnected = true;
            Ready = true;

            string[] GetDeviceNames()
            {
                var deviceNameList = new System.Collections.ArrayList();
                //var check = new System.Text.RegularExpressions.Regex("(COM[1-9][0-9]?[0-9]?)");
                var check = new System.Text.RegularExpressions.Regex(@"VID_0DFD\+PID_0002");


                var searcher = new ManagementObjectSearcher(
                "root\\CIMV2",
                "SELECT * FROM Win32_PnPEntity WHERE ClassGuid=\"{4d36e978-e325-11ce-bfc1-08002be10318}\""
                );
                foreach (var manageObj in searcher.Get())
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

        public void Rotate(int pos)
        {
            _ser.WriteLine($"AXI2:SELSP 1:GOABS {pos}");
            while (_ser.BytesToWrite > 0) ;
            while (true)
            {
                _ser.WriteLine("AXI2:POS?");
                while (_ser.BytesToWrite > 0) ;
                while (_ser.BytesToRead < 1) ;
                var res = _ser.ReadLine();
                if (double.Parse(res) == (double)pos)
                    break;
            }

        }
    }

}
