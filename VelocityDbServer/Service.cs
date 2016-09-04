using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using VelocityDb.Session;
using VelocityDB.Server;
using System.Runtime.InteropServices;
#if !NET_CORE
using System.ServiceProcess;
#endif

namespace VelocityDb.Server
{
  public partial class Service
#if !NET_CORE
    : ServiceBase
#endif
  {
    public static Thread tcpListenThread;
    public static TcpListener listener = null;
    public static bool stopService = false;
    public Service()
    {
 #if !NET_CORE
      InitializeComponent();
#endif
    }

    public static void CreateAndStartThreads(bool asService, int numberOfWorkerThreads)
    {
      Thread.CurrentThread.Name = "VelocityDBServer Main Thread";
      if (asService)
      {
        tcpListenThread = new Thread(new ThreadStart(tcpListen));
        tcpListenThread.Name = "VelocityDBServer Listener";
        //Thread.Sleep(1000);
        tcpListenThread.Start();
      }
      else
        tcpListen();
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
#if NET_CORE
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
#if !NET_CORE
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
#if !NET_CORE
    protected override void OnStart(string[] args)
    {
      //Thread.Sleep(15000); // wait for possible prior service
      stopService = false;
      ServerTcpClient.s_odbServerLog.Source = "VelocityDbServer";
      ServerTcpClient.ShutDown = false;
      CreateAndStartThreads(true, ServerTcpClient.s_numberOfWorkerThreads);
    }

    protected override void OnStop()
    {
      stopService = true;
      ServerTcpClient.ShutDown = true;
      ServerTcpClient.s_acceptDone.Set();
      System.Environment.Exit(0); // fast shutdown so that reinstall succeeds (?)
      tcpListenThread.Join();
    }
#endif
    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool FreeConsole();

    [DllImport("kernel32", SetLastError = true)]
    static extern bool AttachConsole(int dwProcessId);

    [DllImport("user32.dll")]
    static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll", SetLastError = true)]
    static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

    public void startAsNonService(string[] args)
    {
      IntPtr ptr = GetForegroundWindow();
      int u;
      GetWindowThreadProcessId(ptr, out u);
      Process process = Process.GetProcessById(u);
      if (process.ProcessName == "cmd")
        AttachConsole(process.Id);
      ServerTcpClient.s_odbServerLog.Source = "VelocityDbServer";
      ServerTcpClient.ShutDown = false;
      CreateAndStartThreads(false, ServerTcpClient.s_numberOfWorkerThreads);
      FreeConsole();
    }
  }
}
