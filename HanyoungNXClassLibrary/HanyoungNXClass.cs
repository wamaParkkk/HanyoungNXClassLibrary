using System;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;

namespace HanyoungNXClassLibrary
{
    public class HanyoungNXClass : Define
    {        
        private static SerialPort _serialPort;
        private static bool _continue = true;
        private static bool bSet_flag = false;

        private static bool bThread_start;
        private static Thread readThread;
        private static string readData = string.Empty;

        public static void HanyoungNX_Init()
        {
            bool bRtn = DRV_INIT();
            if (bRtn)
            {
                bThread_start = true;

                readThread = new Thread(Read);
                readThread.Start();
            }
            else
            {
                bThread_start = false;

                Global.EventLog("Heater controller initialization failed");
                DRV_CLOSE();
            }
        }

        private static bool DRV_INIT()
        {
            if (InitPortInfo())
            {
                Global.EventLog("Successfully read communication port information");
            }
            else
            {
                Global.EventLog("Failed to read communication port information");
                return false;
            }

            if (PortOpen())
            {
                Global.EventLog("Successfully opened port");
            }
            else
            {
                Global.EventLog("Failed to opened port");
                return false;
            }

            return true;
        }

        private static bool InitPortInfo()
        {
            _serialPort = new SerialPort();

            string sTmpData;
            string FileName = "HanyoungNXPortInfo.txt";

            try
            {
                if (File.Exists(Global.serialPortInfoPath + FileName))
                {
                    byte[] bytes;
                    using (var fs = File.Open(Global.serialPortInfoPath + FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        bytes = new byte[fs.Length];
                        fs.Read(bytes, 0, (int)fs.Length);
                        sTmpData = Encoding.Default.GetString(bytes);

                        char sp = ',';
                        string[] spString = sTmpData.Split(sp);
                        for (int i = 0; i < spString.Length; i++)
                        {
                            string sPortName = spString[0];
                            int iBaudRate = int.Parse(spString[1]);
                            int iDataBits = int.Parse(spString[2]);
                            int iStopBits = int.Parse(spString[3]);
                            int iParity = int.Parse(spString[4]);

                            _serialPort.PortName = sPortName;
                            _serialPort.BaudRate = iBaudRate;
                            _serialPort.DataBits = iDataBits;
                            _serialPort.StopBits = (StopBits)iStopBits;
                            _serialPort.Parity = (Parity)iParity;

                            _serialPort.ReadTimeout = 500;
                            _serialPort.WriteTimeout = 500;
                        }
                    }

                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (IOException ex)
            {
                Global.EventLog($"{ex.Message}");
                return false;
            }
        }

        private static bool PortOpen()
        {
            try
            {
                string[] ports = SerialPort.GetPortNames();
                foreach (string port in ports)
                {
                    if (port != "")
                    {
                        _serialPort.Open();
                        if (_serialPort.IsOpen)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }

                return false;
            }
            catch (IOException ex)
            {
                Global.EventLog($"{ex.Message}");
                return false;
            }
        }

        public static void DRV_CLOSE()
        {
            if (bThread_start)
            {
                readThread.Abort();
            }

            Global.EventLog("Heater communication driver has been terminated");
        }

        // HanyoungNX Thread //////////////////////////////////////////////////////////////////////////
        private static void Read()
        {
            while (_continue)
            {
                try
                {
                    if (!bSet_flag)
                    {
                        Parameter_read();
                    }

                    Thread.Sleep(50);
                }
                catch (TimeoutException)
                {

                }
            }
        }
        ///////////////////////////////////////////////////////////////////////////////////////////////

        private static void Parameter_read()
        {
            try
            {
                readData = string.Empty;

                // PV
                string send_Command = string.Format("{0}{1:D2}DRS,01,0000{2}{3}", Convert.ToChar(RS_STX), 1, Convert.ToChar(RS_CR), Convert.ToChar(RS_LF));
                _serialPort.Write(send_Command);

                Thread.Sleep(20);

                readData = _serialPort.ReadLine();
                //Global.EventLog(readData, "TEMP", "Event");
                if (readData.Length > 1)
                {
                    bool bFind = readData.Contains("OK");
                    if (bFind)
                    {
                        string strTmp = readData.Substring(10, 4);
                        // 16진수 string값을 10진수로 변환
                        int iDecimal = Int32.Parse(strTmp, System.Globalization.NumberStyles.HexNumber);
                        temp_PV = iDecimal * 0.1;
                    }
                }
            }
            catch (Exception ex)
            {
                Global.EventLog($"{ex.Message}");
            }
        }

        public static void set_Temp(double dVal)
        {
            try
            {
                bSet_flag = true;
                readData = string.Empty;

                int setVal = 0;
                setVal = Convert.ToInt32(dVal * 10.0);
                string send_Command = string.Format("{0}{1:D2}DWS,01,0103,{2:X4}{3}{4}", Convert.ToChar(RS_STX), 1, setVal, Convert.ToChar(RS_CR), Convert.ToChar(RS_LF));
                Global.EventLog(send_Command);               
                _serialPort.Write(send_Command);

                Thread.Sleep(20);

                readData = _serialPort.ReadLine();
                Global.EventLog(readData);
                if (readData.Length > 1)
                {
                    bool bFind = readData.Contains("OK");
                    if (bFind)
                        Global.EventLog($"Temperature setting in the controller was completed successfully");
                    else
                        Global.EventLog($"Controller temperature setting failed");
                }

                bSet_flag = false;
            }
            catch (Exception ex)
            {
                Global.EventLog($"{ex.Message}");
            }
        }
    }
}
