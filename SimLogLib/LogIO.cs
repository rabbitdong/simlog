using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SimLogLib
{
    /// <summary>
    /// Log write/read interface
    /// </summary>
    public interface LogIO : IDisposable
    {
        /// <summary>
        /// Write the log item to storage.
        /// </summary>
        /// <param name="item"></param>
        void WriteLogItem(LogItem item);

        /// <summary>
        /// Get the log for the day.
        /// </summary>
        /// <param name="time">The day of log</param>
        /// <returns>The log item list</returns>
        List<LogItem> GetLog(DateTime time);

        /// <summary>
        /// Get the log identify by the id
        /// </summary>
        /// <param name="id">The log id</param>
        /// <returns>The log item list</returns>
        List<LogItem> GetLog(string id);

        /// <summary>
        /// Open the log storage for write/read.
        /// </summary>
        void Open();

        /// <summary>
        /// Close the log storage after write/read.
        /// </summary>
        void Close();
    }

    /// <summary>
    /// The file system implements of LogIO
    /// </summary>
    internal class FileLog : LogIO
    {
        private BinaryWriter _writer;

        public void WriteLogItem(LogItem item)
        {
            _writer.Write(item.Time.ToBinary());
            _writer.Write((int)item.Level);
            _writer.Write(item.Message);
        }

        public void Dispose()
        {
            this.Close();
        }


        public List<LogItem> GetLog(DateTime time)
        {
            DirectoryInfo di = new DirectoryInfo(LogConfig.LogDir);
            FileInfo[] fis = di.GetFiles(string.Format("{0:yyyyMMdd}*", time));
            List<LogItem> logitems = new List<LogItem>();
            foreach (FileInfo fi in fis)
            {
                BinaryReader sr = new BinaryReader(fi.OpenRead());
                while (sr.BaseStream.Position < fi.Length)
                {
                    LogItem item = new LogItem();
                    item.Time = DateTime.FromBinary(sr.ReadInt64());
                    item.Level = (LogLevel)sr.ReadInt32();
                    item.Message = sr.ReadString();
                    logitems.Add(item);
                }
                sr.Close();
            }

            return logitems;
        }


        public void Open()
        {
            string filename = string.Format("{0}/{1:yyyyMMddHHmmssff}.log", LogConfig.LogDir, DateTime.Now);
            _writer = new BinaryWriter(new FileStream(filename, FileMode.OpenOrCreate));
        }

        public void Close()
        {
            if (_writer != null)
            {
                _writer.Flush();
                _writer.Close();
                _writer = null;
            }
        }

        /// <summary>
        /// Get the log identify be id.
        /// </summary>
        /// <param name="id">the file name</param>
        /// <returns></returns>
        public List<LogItem> GetLog(string id)
        {
            //for FileLog, id refer the file name.
            if (!File.Exists(id))
                throw new FileNotFoundException(string.Format("The file {0} is not exist!", id));

            List<LogItem> logitems = new List<LogItem>();
            FileInfo fi = new FileInfo(id);
            BinaryReader sr = new BinaryReader(fi.OpenRead());
            while (sr.BaseStream.Position < fi.Length)
            {
                LogItem item = new LogItem();
                item.Time = DateTime.FromBinary(sr.ReadInt64());
                item.Level = (LogLevel)sr.ReadInt32();
                item.Message = sr.ReadString();
                logitems.Add(item);
            }

            return logitems;
        }
    }
}
