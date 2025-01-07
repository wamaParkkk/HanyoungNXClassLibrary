using System;
using System.IO;

namespace HanyoungNXClassLibrary
{
    internal class Global
    {        
        public static string logfilePath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\HanyoungNXLog\"));
        public static string serialPortInfoPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\SerialComm\"));

        public static void EventLog(string Msg)
        {
            try
            {
                string sDate = DateTime.Today.ToShortDateString();
                string sTime = DateTime.Now.ToString("HH:mm:ss:fff");
                string sDateTime;
                sDateTime = "[" + sDate + ", " + sTime + "] ";

                WriteFile(sDateTime + Msg);
            }
            catch { }
        }

        private static void WriteFile(string Msg)
        {
            try
            {
                string sDate = DateTime.Today.ToShortDateString();
                string FileName = sDate + ".txt";

                if (File.Exists(logfilePath + FileName))
                {
                    StreamWriter writer;
                    writer = File.AppendText(logfilePath + FileName);
                    writer.WriteLine(Msg);
                    writer.Close();
                }
                else
                {
                    CreateFile(Msg);
                }
            }
            catch { }            
        }

        private static void CreateFile(string Msg)
        {
            try
            {
                string sDate = DateTime.Today.ToShortDateString();
                string FileName = sDate + ".txt";

                if (!File.Exists(logfilePath + FileName))
                {
                    using (File.Create(logfilePath + FileName)) ;
                }

                StreamWriter writer;
                writer = File.AppendText(logfilePath + FileName);
                writer.WriteLine(Msg);
                writer.Close();
            }
            catch {}            
        }
    }
}
