using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;
using System.Windows.Forms;
using System.IO;

namespace Selenium_Testing_Asset
{
    public class LogManager
    {
        //string TaskNo;
        public void CreateLogFile(string taskno)
        {

            // 1. Log Folder 생성 
            string sDirPath;
            sDirPath = Application.StartupPath + "\\log";
            DirectoryInfo di = new DirectoryInfo(sDirPath);
            if (di.Exists == false)
            {
                di.Create();
            }

            // 2. log file 생성
            string savePath = string.Format("log\\{0}_{1}-{2}-{3}", taskno, DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day) + ".txt";
            string testValue = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " | " + @"****************** START Testing Package ******************";

            // 2. log file 존제 유무
            FileInfo fi = new FileInfo(Application.StartupPath + "\\" + savePath);
            if (fi.Exists == false)
            {
                System.IO.File.WriteAllText(savePath, testValue, Encoding.Default);
            }
            else
            {
                //System.IO.File.AppendAllText(savePath, "\r\n" + testValue, Encoding.Default);
            }


        }

        public void AppendLogFile(string TaskNo, string log)
        {
            string savePath = string.Format("log\\{0}_{1}-{2}-{3}", TaskNo, DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day) + ".txt";
            string testValue = "\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " | " + log;
            System.IO.File.AppendAllText(savePath, testValue, Encoding.Default);
        }

        public void AppendErrorLogFile(string TaskNo, string log)
        {
            string savePath = string.Format("log\\{0}_{1}-{2}-{3}", TaskNo, DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day) + ".txt";
            string testValue = "\r\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " | [ERROR]" + log;
            System.IO.File.AppendAllText(savePath, testValue, Encoding.Default);
        }
    }
}
