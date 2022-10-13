using System.Text;
using System.IO;
using System;

namespace NetboxBulkConnect.Misc
{
    public class FileLogging
    {
        private static FileStream fileLogStream = null;
        private static UTF8Encoding encoder = null;

        public static void Initialize()
        {
            fileLogStream = new FileStream("Log.log", FileMode.Append, FileAccess.Write);
            encoder = new UTF8Encoding(true);

            Append("Logging initialized");
        }

        public static void Deinitialize()
        {
            Append("Logging deinitialized");

            fileLogStream.Close();
            fileLogStream.Dispose();

            encoder = null;
        }

        public static void Append(string text)
        {
            DateTime currentTime = DateTime.Now;
            string secondCount = currentTime.Second < 10 ? $"0{currentTime.Second}" : currentTime.Second.ToString();
            string minuteCount = currentTime.Minute < 10 ? $"0{currentTime.Minute}" : currentTime.Minute.ToString();
            string hourCount = currentTime.Hour < 10 ? $"0{currentTime.Hour}" : currentTime.Hour.ToString();

            string formattedString = $"[{hourCount}:{minuteCount}:{secondCount}] {text}{Environment.NewLine}";

            byte[] data = encoder.GetBytes(formattedString);
            fileLogStream.Write(data, 0, data.Length);
        }
    }
}
