using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace SimLogLib
{
    /// <summary>
    /// Log configuration
    /// </summary>
    /// <remarks>
    /// Use log library, must set the configuration.
    /// </remarks>
    public static class LogConfig
    {
        private static string _log_dir;

        public static int _log_capcity_each_block = 500;

        static LogConfig()
        {
            //Set the default log directory as the same as assembly location.
            _log_dir = Assembly.GetExecutingAssembly().Location;
        }

        /// <summary>
        /// Get or set the log directory.
        /// </summary>
        public static string LogDir
        {
            get { return _log_dir; }
            set
            {
                if (Directory.Exists(value))
                    _log_dir = value;
                else
                    throw new DirectoryNotFoundException();
            }
        }

        /// <summary>
        /// Get or set the log block size.
        /// </summary>
        /// <remarks>
        /// For file log. block size is the line in the file.
        /// </remarks>
        public static int LogCapacityEachBlock
        {
            get { return _log_capcity_each_block; }
            set
            {
                if (value >= 1)
                    _log_capcity_each_block = value;
                else
                    throw new ArgumentOutOfRangeException("Cann't not be smaller than 1.");
            }
        }
    }
}
