using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using VelocityDb.Session;
using VelocityDB.Server;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.ServiceProcess;

namespace VelocityDBCoreServer
{
  public partial class Service : IHostedService
  {
    public static Thread tcpListenThread;
    public static TcpListener listener = null;
    public static bool stopService = false;
    public Service()
    {
    }

    public static void CreateAndStartThreads(int numberOfWorkerThreads)
    {
      Thread.CurrentThread.Name = "VelocityDBServer Main Thread";
      tcpListenThread = new Thread(new ThreadStart(tcpListen));
      tcpListenThread.Name = "VelocityDBServer Listener";
      //Thread.Sleep(1000);
      tcpListenThread.Start();
    }

    protected static void tcpListen()
    {
      try
      {
        //IPAddress ipAddress = Dns.Resolve("localhost").AddressList[0];
        //listener = new TcpListener(ipAddress, ServerTcpClient.odbTcpPort);
        //listener = new TcpListener(IPAddress.IPv6Any, ServerTcpClient.odbTcpPort);
        listener = new TcpListener(IPAddress.Any, SessionBase.s_serverTcpIpPortNumber);
        Socket s = listener.Server;
        LingerOption lingerOption = new LingerOption(true, 0);
        s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, lingerOption);
        s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
        s.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, 1);
        listener.Start();
      }
      catch (System.Exception e)
      {
        ServerTcpClient.s_odbServerLog.WriteEntry(e.ToString());
      }

      try
      {
        while (!ServerTcpClient.ShutDown)
        {
          ServerTcpClient.s_acceptDone.Reset();
#if NET_COREx
          //listener?.AcceptSocketAsync().Wait();
          ServerTcpClient.AcceptTcpClient(listener);
#else
          listener?.BeginAcceptTcpClient(new AsyncCallback(ServerTcpClient.AcceptTcpClient), listener);
#endif

          ServerTcpClient.s_acceptDone.WaitOne();
        }
      }
      catch (SocketException e)
      {
#if !NET_COREx
        if (e.ErrorCode != 10054)	// client closed socket
#endif
        {
          ServerTcpClient.s_odbServerLog.WriteEntry(e.ToString());
        }
      }
      catch (System.Exception e)
      {
        ServerTcpClient.s_odbServerLog.WriteEntry(e.ToString());
      }
      finally
      {
        listener.Stop();
      }

      if (!stopService)
      {
        try
        {
          listener?.Stop();
          Environment.Exit(0);
        }
        catch (System.Exception e)
        {
          ServerTcpClient.s_odbServerLog.WriteEntry(e.ToString());
        }
      }
    }
    public Task StartAsync(CancellationToken cancellationToken)
    {
      //Thread.Sleep(15000); // wait for possible prior service
      stopService = false;
      ServerTcpClient.s_odbServerLog.Source = "VelocityDbServer";
      ServerTcpClient.ShutDown = false;
      CreateAndStartThreads(ServerTcpClient.s_numberOfWorkerThreads);
      return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
      stopService = true;
      ServerTcpClient.ShutDown = true;
      ServerTcpClient.s_acceptDone.Set();
      System.Environment.Exit(0); // fast shutdown so that reinstall succeeds (?)
      tcpListenThread.Join();
      return Task.CompletedTask;
    }
  }

  // Code from https://github.com/aspnet/Hosting/blob/2a98db6a73512b8e36f55a1e6678461c34f4cc4d/samples/GenericHostSample/ServiceBaseLifetime.cs
  public class ServiceBaseLifetime : ServiceBase, IHostLifetime
  {
    private readonly TaskCompletionSource<object> _delayStart = new TaskCompletionSource<object>();

    public ServiceBaseLifetime(IApplicationLifetime applicationLifetime)
    {
      ApplicationLifetime = applicationLifetime ?? throw new ArgumentNullException(nameof(applicationLifetime));
    }

    private IApplicationLifetime ApplicationLifetime { get; }

    public Task WaitForStartAsync(CancellationToken cancellationToken)
    {
      cancellationToken.Register(() => _delayStart.TrySetCanceled());
      ApplicationLifetime.ApplicationStopping.Register(Stop);

      new Thread(Run).Start(); // Otherwise this would block and prevent IHost.StartAsync from finishing.
      return _delayStart.Task;
    }

    private void Run()
    {
      try
      {
        Run(this); // This blocks until the service is stopped.
        _delayStart.TrySetException(new InvalidOperationException("Stopped without starting"));
      }
      catch (Exception ex)
      {
        _delayStart.TrySetException(ex);
      }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
      Stop();
      return Task.CompletedTask;
    }

    // Called by base.Run when the service is ready to start.
    protected override void OnStart(string[] args)
    {
      _delayStart.TrySetResult(null);
      base.OnStart(args);
    }

    // Called by base.Stop. This may be called multiple times by service Stop, ApplicationStopping, and StopAsync.
    // That's OK because StopApplication uses a CancellationTokenSource and prevents any recursion.
    protected override void OnStop()
    {
      ApplicationLifetime.StopApplication();
      base.OnStop();
    }
  }

  public static class ServiceBaseLifetimeHostExtensions
  {
    public static IHostBuilder UseServiceBaseLifetime(this IHostBuilder hostBuilder)
    {
      return hostBuilder.ConfigureServices((hostContext, services) => services.AddSingleton<IHostLifetime, ServiceBaseLifetime>());
    }

    public static Task RunAsServiceAsync(this IHostBuilder hostBuilder, CancellationToken cancellationToken = default)
    {
      return hostBuilder.UseServiceBaseLifetime().Build().RunAsync(cancellationToken);
    }
  }
}
