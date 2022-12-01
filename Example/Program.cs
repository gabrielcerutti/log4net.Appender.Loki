using System;
using System.IO;
using System.Threading;

namespace Example
{
    class Program
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static void Main(string[] args)
        {
            var fileInfo = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "log4net.config");
            if (fileInfo.Exists)
                log4net.Config.XmlConfigurator.Configure(fileInfo);
            else
                throw new InvalidOperationException("No log config file found");

            int count = 0;

            while (true)
            {
                Thread.Sleep(2000);
                logger.Debug($"Log number {count++}");
                logger.Info($"Log number { count++ }");
                logger.Warn($"Log number {count++}");
                logger.Error($"Log number {count++}");
                logger.Fatal($"Log number {count++}");
            }
        }
    }
}
