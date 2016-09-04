using System;
using System.Collections.Generic;
#if !NET_CORE
using System.Configuration;
using System.Configuration.Install;
using System.ServiceProcess;
#endif
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using VelocityDb.Session;
using VelocityDB.Server;

namespace VelocityDb.Server
{
	static class Server
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main(string[] args)
		{
      bool startAsNonService = false;
 #if !NET_CORE
			// Create the source, if it does not already exist.
			if (!EventLog.SourceExists("VelocityDbServer"))
			{
				//An event log source should not be created and immediately used.
				//There is a latency time to enable the source, it should be create
				//prior to executing the application that uses the source.
				//Execute this sample a second time to use the new source.
        EventLog.CreateEventSource("VelocityDbServer", "VelocityDbServerLog");
			}
#endif
      if (args.Length > 0)
      {
        if (bool.TryParse(args[0], out startAsNonService) == false)
          Console.WriteLine("First parameter must be a boolean (0/1) stating if server should shutdown as non service");
      }
#if NET_CORE
      string serverLogFile = "c:/serverCoreLog.txt";
#else
      string serverLogFile = ConfigurationManager.AppSettings["ServerActivityLogFile"] ?? "" .ToString();
#endif
      if (serverLogFile.Length > 0)
      {
        try
        {
          FileInfo info = new FileInfo(serverLogFile);
          Stream s = info.Open(FileMode.Append, FileAccess.Write, FileShare.Read);
          if (s != null)
            ServerTcpClient.s_serverActivityLogFile = new StreamWriter(s);
        }
        catch
        {

        }
      }
#if !NET_CORE
      string baseDatabasePath = ConfigurationManager.AppSettings["BaseDatabasePath"]  ?? "" .ToString();
      if (baseDatabasePath.Length > 0)
        SessionBase.BaseDatabasePath = baseDatabasePath;

      string doWindowsAuth = ConfigurationManager.AppSettings["DoWindowsAuthentication"] ?? "" .ToString();
      bool doWindowsAuthentication;
      if (bool.TryParse(doWindowsAuth, out doWindowsAuthentication))
        SessionBase.DoWindowsAuthentication = doWindowsAuthentication;
      string workerThreadCt = ConfigurationManager.AppSettings["NumberOfWorkerThreads"]  ?? "" .ToString();
      int.TryParse(workerThreadCt, out ServerTcpClient.s_numberOfWorkerThreads);
      string portNumber = ConfigurationManager.AppSettings["TcpIpPortNumber"] ?? "".ToString();
      int.TryParse(portNumber, out SessionBase.s_serverTcpIpPortNumber);
      string maximumMemoryUseStr = ConfigurationManager.AppSettings["MaximumMemoryUse"] ?? "".ToString();
      long maximumMemoryUse = 0;
      long.TryParse(maximumMemoryUseStr, out maximumMemoryUse);
      if (maximumMemoryUse > 0)


        DataCache.MaximumMemoryUse = maximumMemoryUse;
#if !BLOCKINGQueue
      ServerTcpClient.s_requestRingBuffer.SetGatingSequences();
#endif
			if (!startAsNonService)
			{
				ServiceBase[] ServicesToRun;
				ServicesToRun = new ServiceBase[] 
				{ 
					new Service() 
				};
				ServiceBase.Run(ServicesToRun);
			}
			else
#endif
			{
      Service velocityDbServer = new Service();
        velocityDbServer.startAsNonService(args);
			}
		}
	}
}
