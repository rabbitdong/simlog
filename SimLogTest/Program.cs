using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimLogLib;
using System.Threading;
using System.Threading.Tasks;

namespace SimLogTest
{
    class Program
    {
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

            LogConfig.LogDir = @"D:\weblogs\simlogtest";
            LogConfig.LogCapacityEachBlock = 500;
            Random rand = new Random();
            int count = 0;

            Task t = new Task(() => {
                while (true)
                {
                    Thread.Sleep(rand.Next(2000, 30000));
                    Log.Instance.WriteLog(string.Format("The Loging in time:{0}", DateTime.Now));
                    Console.WriteLine("generate log:{0}", ++count);
                }
            });

            t.Start();

            Console.WriteLine(DateTime.Now);
            Console.WriteLine(DateTime.Today);

            while (true)
            {
                int c = Console.Read();
                if (c == 'p')
                {
                    List<LogItem> logs = Log.Instance.GetLog(DateTime.Now);
                    foreach (LogItem item in logs)
                        Console.WriteLine("[{0}] [{1}] [{2}]", item.Time, item.Level, item.Message);
                }
                else if (c == 'x')
                    break;
            }
            t.Wait();
        }

        static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            Log.Instance.Dispose();
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Log.Instance.Dispose();
        }
    }
}
