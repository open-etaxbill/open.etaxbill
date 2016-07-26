using System;
using System.Diagnostics;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;

namespace OpenETaxBill.Engine.Signer
{
    /// <summary>
    /// 
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// 해당 응용 프로그램의 주 진입점입니다.
        /// </summary>
        public static void Main(string[] args)
        {
            // Add the event handler for handling non-UI thread exceptions to the event. 
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

            ServiceBase[] ServicesToRun = new ServiceBase[]
            {
                new eTaxSigner()
            };

            if (Environment.UserInteractive == true)
            {
                RunInteractive(ServicesToRun);
            }
            else
            {
                ServiceBase.Run(ServicesToRun);
            }
        }

        static void RunInteractive(ServiceBase[] servicesToRun)
        {
            Console.WriteLine("services running in interactive mode.");

            MethodInfo onStartMethod = typeof(ServiceBase).GetMethod("OnStart", BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (ServiceBase service in servicesToRun)
            {
                Console.WriteLine("Starting {0}...", service.ServiceName);
                onStartMethod.Invoke(service, new object[] { new string[] { } });
                Console.WriteLine("Started");
            }

            Console.WriteLine("Press any key to stop the services and end the process...");
            Console.ReadKey();

            MethodInfo onStopMethod = typeof(ServiceBase).GetMethod("OnStop", BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (ServiceBase service in servicesToRun)
            {
                Console.WriteLine("Stopping {0}...", service.ServiceName);
                onStopMethod.Invoke(service, null);
                Console.WriteLine("Stopped");
            }

            Console.WriteLine("All services stopped.");

            // Keep the console alive for a second to allow the user to see the message.
            Thread.Sleep(1000);
        }

        static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            ELogger.SNG.WriteLog("process exit:({0,3}).....", Process.GetCurrentProcess().Id);
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            ELogger.SNG.WriteLog("unhandled exception:({0,3}).....", Process.GetCurrentProcess().Id);

            try
            {
                Exception ex = (Exception)e.ExceptionObject;
                ELogger.SNG.WriteLog(ex);
            }
            catch (Exception)
            {
            }
        }
    }
}