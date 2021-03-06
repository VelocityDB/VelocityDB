﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VelocityDb;
using VelocityDb.Collection;
using VelocityDb.Session;
using VelocityDb.TypeInfo;

namespace DatabaseManager.Model
{
  public class FederationInfo : OptimizedPersistable
  {
    string m_name;
    string m_hostName;
    string m_sytemDbsPath;
    int m_portNumber;
    int m_waitForLockMilliseconds;
    bool m_windowsAuthentication;
    bool m_usesServerClient;
    bool m_usePessimisticLocking;
    bool m_enableSyncByTrackingChanges;
    List<DateTime> m_validated;
    string[] m_typesAssemblies;
    string[] m_typesDependencyAssemblies;
    List<FederationCopyInfo> m_federationCopies;
    [NonSerialized]
    SchemaInfo m_schemaInfo;

    public FederationInfo()
    {
      m_hostName = Dns.GetHostName();
      m_portNumber = 7031;
      m_windowsAuthentication = false;
      m_usesServerClient = false;
      m_validated = new List<DateTime>();
      m_federationCopies = new List<FederationCopyInfo>();
      m_enableSyncByTrackingChanges = false;
    }

    public FederationInfo(string hostName, string systemDbsPath, int portNumber, bool windowsAuthentication, string[] assemblyDirectory, int waitForLockMilliseconds,
      bool usesServerClient, string name):this()
    {
      m_hostName = hostName;
      m_sytemDbsPath = systemDbsPath;
      m_portNumber = portNumber;
      m_windowsAuthentication = windowsAuthentication;
      m_usesServerClient = usesServerClient;
      m_name = name;
      m_typesAssemblies = assemblyDirectory;
      m_waitForLockMilliseconds = waitForLockMilliseconds;
      LoadAllFederationAssemblies();
    }

    public List<FederationCopyInfo> FederationCopies
    {
      get
      {
        return m_federationCopies;
      }
    }

    public string HostName
    {
      get
      {
        return m_hostName;
      }
      set
      {
        Update();
        m_hostName = value;
      }
    }

    public string[] ClassesFilenames
    {
      get
      {
        return m_typesAssemblies;
      }
      set
      {
        m_typesAssemblies = value;
      }
    }

    public string[] DependencyFiles
    {
      get
      {
        return m_typesDependencyAssemblies;
      }
      set
      {
        m_typesDependencyAssemblies = value;
      }
    }

    public bool EnableSyncByTrackingChanges
    {
      get
      {
        return m_enableSyncByTrackingChanges;
      }
      set
      {
        Update();
        m_enableSyncByTrackingChanges = value;
      }
    }

    public string SystemDbsPath
    {
      get
      {
        return m_sytemDbsPath;
      }
      set
      {
        m_sytemDbsPath = value;
      }
    }

    public int PortNumber
    {
      get
      {
        return m_portNumber;
      }
    }

    public bool WindowsAuthentication
    {
      get
      {
        return m_windowsAuthentication;
      }
      set
      {
        Update();
        m_windowsAuthentication = value;
      }
    }
    public int WaitForMilliSeconds
    {
      get
      {
        return m_waitForLockMilliseconds;
      }
      set
      {
        Update();
        m_waitForLockMilliseconds = value;
      }
    }

    public bool UsePessimisticLocking
    {
      get
      {
        return m_usePessimisticLocking;
      }
      set
      {
        Update();
        m_usePessimisticLocking = value;
      }
    }

    public List<DateTime> Validated
    {
      get
      {
        return m_validated;
      }
    }

    public string Name
    {
      get
      {
        if (m_name != null)
          return m_name;
        FileInfo fileInfo = new FileInfo(m_sytemDbsPath);
        return fileInfo.Name;
      }
      set
      {
        Update();
        m_name = value;
      }
    }

    public bool UsesServerClient
    {
      get
      {
        return m_usesServerClient;
      }
      set
      {
        Update();
        m_usesServerClient = value;
      }
    }

    public override void Unpersist(SessionBase session)
    {
      if (Id == 0)
        return;
      base.Unpersist(session);
    }

    /// <summary>
    /// Gets filename of all non-dynamic loaded assemblies.
    /// </summary>
    /// <returns></returns>
    private static string[] GetLoadedAssemblies()
    {
      return AppDomain.CurrentDomain.GetAssemblies()
          .Where(lAssembly => !lAssembly.IsDynamic)
          .Select(lAssembly => lAssembly.CodeBase)
          .Where(lCodeBase => lCodeBase != null)
        // Remove the file:/// prefix.
          .Select(lCodeBase => new string(lCodeBase.Skip(8).ToArray()))
          .ToArray();
    }

    public void LoadAllFederationAssemblies()
    {
      if (m_typesAssemblies != null && m_typesAssemblies.Length > 0)
        m_schemaInfo = SchemaExtractor.Extract(m_typesAssemblies, m_typesDependencyAssemblies);
    }

    public override void InitializeAfterRead(SessionBase session)
    {
      base.InitializeAfterRead(session);
      LoadAllFederationAssemblies();
    }
  }
}
