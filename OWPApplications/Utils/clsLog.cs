using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using Newtonsoft.Json;
using System.Data;
using Microsoft.Extensions.Configuration;

namespace OWPApplications.Utils
{
    public class clsLog
    {
        public IConfiguration _configuration;

        public clsLog(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        string strFolder = Path.Combine(Environment.CurrentDirectory, "log");

        /// <summary>
        /// Write a text in a log file
        /// </summary>
        /// <param name="Text"></param>
        public void WriteError(string location, string Text, Exception ex)
        {
            Write("Error in " + location + ": " + Text + ". -> " + ex.Message);
        }
        /// <summary>
        /// Write a text in a log file
        /// </summary>
        /// <param name="Text"></param>
        public void Write(string Text)
        {
            try
            {
                DateTime oDateStart = DateTime.Now;
                string strFilePath = strFolder + "/" + oDateStart.ToString("yyyyMMdd") + ".log";

                if (!Directory.Exists(strFolder))
                    Directory.CreateDirectory(strFolder);

                if (!File.Exists(strFilePath))
                {
                    using (StreamWriter sw = File.AppendText(strFilePath))
                    { }
                }
                using (StreamWriter sw = File.AppendText(strFilePath))
                {
                    sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ": " + Text);
                }
            }
            catch (Exception) { } /*Ignore any error*/
        }

        /// <summary>
        /// Write a file to leave a copy
        /// </summary>
        /// <param name="Text"></param>
        public void WriteFile(string FileName, string extension, string Text)
        {
            WriteFile(FileName, extension, Text, true);
        }
        /// <summary>
        /// Write a file to leave a copy
        /// </summary>
        /// <param name="Text"></param>
        public void WriteFile(string FileName, string extension, string Text, bool AddTimeToNameFile)
        {
            try
            {
                DateTime oDateStart = DateTime.Now;
                string strFilePath = FileName;
                if (AddTimeToNameFile) strFilePath += "_" + oDateStart.ToString("yyyyMMddHHmmss");
                strFilePath += "." + extension;
                strFilePath = Path.Combine(strFolder, strFilePath);

                if (!Directory.Exists(strFolder))
                    Directory.CreateDirectory(strFolder);
                if (AddTimeToNameFile)
                {
                    File.WriteAllText(strFilePath, Text);
                }
                else
                {
                    if (!File.Exists(strFilePath))
                    {
                        using (StreamWriter sw = File.AppendText(strFilePath))
                        { }
                    }
                    using (StreamWriter sw = File.AppendText(strFilePath))
                    {
                        sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ": " + Text);
                    }
                }
            }
            catch (Exception)
            {
            } /*Ignore any error*/
        }
    }
}
