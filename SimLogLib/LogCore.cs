using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections.Specialized;

namespace SimLogLib
{
    /// <summary>
    /// Log Severity Level
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// Information Level, no severity.
        /// </summary>
        Information,

        /// <summary>
        /// Waring Level, low severity.
        /// </summary>
        Warning,

        /// <summary>
        /// Error Level, hight severity.
        /// </summary>
        Error
    }

    /// <summary>
    /// Log Item Struct.
    /// </summary>
    public class LogItem
    {
        /// <summary>
        /// Get or set the log level.
        /// </summary>
        public LogLevel Level { get; set; }

        /// <summary>
        /// Get or set the log message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Get or set the log time.
        /// </summary>
        public DateTime Time { get; set; }
    }

    /// <summary>
    /// Log processing.
    /// </summary>
    public class Log : IDisposable
    {
        private static Log _inst = new Log();

        private Queue<LogItem> _logQueue = new Queue<LogItem>();

        //The thread for write the log item.
        private Thread _wthread;

        private LogIO _logio;

        private bool _running;

        private Timer _timer;

        private Log()
        {
            _logio = new FileLog();

            _running = true;
            _wthread = new Thread(WritingProcess);
            _wthread.IsBackground = true;            
            _wthread.Start();

            _timer = new Timer(TimeLogProcess, null, 0, 1000);
        }

        /// <summary>
        /// Dump the log when enter next day.
        /// </summary>
        /// <param name="state">no used</param>
        private void TimeLogProcess(object state)
        {
            //dump the log when 23:59:59
            if (DateTime.Now < DateTime.Today.AddDays(1).AddSeconds(-1))
                return;

            //indicate the write thread to dump the log.
            Monitor.Pulse(_logQueue);

            //Timer begin after: 1000 * 60 * 60 * 23 + 1000 * 60 * 59  = 23hr 59min 
            _timer.Change(86340000, 1000);
        }

        /// <summary>
        /// 写日志的过程，该方法需要放在同步锁中
        /// </summary>
        private void FlushLog()
        {
            _logio.Open();
            while (_logQueue.Count > 0)
            {
                LogItem item = _logQueue.Dequeue();
                _logio.WriteLogItem(item);
            }
            _logio.Dispose();
        }

        /// <summary>
        /// Dump the log(for mannul calling.)
        /// </summary>
        public void Dump()
        {
            lock (_logQueue)
            {
                //Dump only for the queue isn't empty.
                if (_logQueue.Count > 0)
                    FlushLog();
            }
        }

        /// <summary>
        /// Write log item.
        /// </summary>
        private void WritingProcess()
        {
            while (_running)
            {
                lock (_logQueue)
                {
                    if (_logQueue.Count < LogConfig.LogCapacityEachBlock)
                        Monitor.Wait(_logQueue);

                    Dump();
                }
            }
        }

        /// <summary>
        /// Get the log instance.
        /// </summary>
        public static Log Instance
        {
            get { return _inst; }
        }


        /// <summary>
        /// Write the log for information message.
        /// </summary>
        /// <param name="message">The message for writing.</param>
        public void WriteLog(string message)
        {
            LogItem item = new LogItem();
            item.Time = DateTime.Now;
            item.Level = LogLevel.Information;
            item.Message = message;

            lock (_logQueue)
            {
                _logQueue.Enqueue(item);
                if (_logQueue.Count > LogConfig.LogCapacityEachBlock)
                    Monitor.Pulse(_logQueue);
            }
        }

        /// <summary>
        /// Write the log for error message.
        /// </summary>
        /// <param name="message">The message for writing.</param>
        public void WriteError(string message)
        {
            LogItem item = new LogItem();
            item.Time = DateTime.Now;
            item.Level = LogLevel.Error;
            item.Message = message;

            lock (_logQueue)
            {
                _logQueue.Enqueue(item);
                if (_logQueue.Count > LogConfig.LogCapacityEachBlock)
                    Monitor.Pulse(_logQueue);
            }
        }

        /// <summary>
        /// Write the log for warning message
        /// </summary>
        /// <param name="message">The message for warning.</param>
        public void WriteWarn(string message)
        {
            LogItem item = new LogItem();
            item.Time = DateTime.Now;
            item.Level = LogLevel.Warning;
            item.Message = message;

            lock (_logQueue)
            {
                _logQueue.Enqueue(item);
                if (_logQueue.Count > LogConfig.LogCapacityEachBlock)
                    Monitor.Pulse(_logQueue);
            }
        }

        /// <summary>
        ///  Get the log for sometime.
        /// </summary>
        /// <param name="time">the time of log(the day)</param>
        /// <returns>The Content of Log Files</returns>
        public List<LogItem> GetLog(DateTime time)
        {
            Dump();
            return _logio.GetLog(time);
        }

        /// <summary>
        /// Get the log identify be id.
        /// </summary>
        /// <param name="id">the log identity</param>
        /// <returns>The content of log</returns>
        public List<LogItem> GetLog(string id)
        {
            return _logio.GetLog(id);
        }

        /// <summary>
        /// Release the log instance
        /// </summary>
        public void Dispose()
        {
            //Dump the log message
            _running = false;
            FlushLog();

            if (_wthread != null && _wthread.ThreadState != ThreadState.Stopped)
                _wthread.Abort();

            if (_logio != null)
                _logio.Dispose();
        }
    }
}
