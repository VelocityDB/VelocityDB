using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.WindowsServices;
using Microsoft.AspNetCore.Server.HttpSys;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VelocityDb;
using VelocityDb.Session;
using VelocityDB.Server;

namespace VelocityDBCoreServer
{
  public class Program
  {
    public static void Main(string[] args)
    {
      var isService = !(Debugger.IsAttached || args.Contains("--console"));

      if (!System.Diagnostics.EventLog.SourceExists("VelocityDbServer"))
      {
        System.Diagnostics.EventLog.CreateEventSource("VelocityDbServer", "VelocityDbServerLog");
      }
      var pathToExe = Process.GetCurrentProcess().MainModule.FileName;
      var pathToContentRoot = Path.GetDirectoryName(pathToExe);
      if (!isService)
        pathToContentRoot = Directory.GetCurrentDirectory();
      var config = new ConfigurationBuilder().SetBasePath(pathToContentRoot).AddJsonFile("appsettings.json").AddCommandLine(args).Build();
      var vdbSection = config.GetSection("VelocityDBServer");
      string serverLogFile = vdbSection["ServerActivityLogFile"] ?? "".ToString();
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
      else if (!isService)
        Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
      string baseDatabasePath = vdbSection["BaseDatabasePath"] ?? "".ToString();
      if (baseDatabasePath.Length > 0)
        SessionBase.BaseDatabasePath = baseDatabasePath;

      string doWindowsAuth = vdbSection["DoWindowsAuthentication"] ?? "".ToString();
      bool doWindowsAuthentication;
      if (bool.TryParse(doWindowsAuth, out doWindowsAuthentication))
        SessionBase.DoWindowsAuthentication = doWindowsAuthentication;
      string workerThreadCt = vdbSection["NumberOfWorkerThreads"] ?? "".ToString();
      int.TryParse(workerThreadCt, out ServerTcpClient.s_numberOfWorkerThreads);
      string portNumber = vdbSection["TcpIpPortNumber"] ?? "".ToString();
      int.TryParse(portNumber, out SessionBase.s_serverTcpIpPortNumber);
      string maximumMemoryUseStr = vdbSection["MaximumMemoryUse"] ?? "".ToString();
      long maximumMemoryUse = 0;
      long.TryParse(maximumMemoryUseStr, out maximumMemoryUse);
      if (maximumMemoryUse > 0)
        DataCache.MaximumMemoryUse = maximumMemoryUse;
      var schemaSection = vdbSection.GetSection("Schema");
      if (schemaSection != null)
      {
        var toLoad = schemaSection.Get<string[]>();
        //schemaSection = schemaSection.AsEnumerable.Trim('[').Trim(']');
        //var toLoad = schemaSection.Split(',');
        foreach (var p in toLoad)
        {
          try
          {
            var a = Assembly.LoadFrom(p);
            // var t = a.GetTypes();
          }
          catch (Exception ex)
          {
            Trace.WriteLine($"Failed to load Schema assembly: {ex.Message}");
          }
        }
      }
      var webHostArgs = args.Where(arg => arg != "--console").ToArray();
      var builder = WebHost.CreateDefaultBuilder(webHostArgs).UseContentRoot(pathToContentRoot).UseKestrel(options => { options.Listen(IPAddress.Loopback, 7033);}); //options.Listen(IPAddress.Loopback, 7034, listenOptions => { listenOptions.UseHttps();}); });
      //builder.UseHttpSys(options =>
      // {
      //   options.Authentication.Schemes = AuthenticationSchemes.NTLM;
      //   options.Authentication.AllowAnonymous = true;
      //   options.MaxConnections = 100;
      //   options.MaxRequestBodySize = 30000000;
      //   options.UrlPrefixes.Add("https://localhost:5001");
      // });
      builder.ConfigureServices((hostContext, services) =>
      {
        //services.AddAuthentication(Microsoft.AspNetCore.Server.IISIntegration.IISDefaults.AuthenticationScheme);
        services.AddHostedService<Service>();
      });
      var host = builder.UseStartup<Startup>().Build();

      if (isService)
      {
        host.RunAsService();
      }
      else
      {
        host.Run();
      }
    }
  }
}
