using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Session;
using VelocityDb.Collection;

namespace VelocityDbSchema.VelocityDb
{
  public class VisitEvent : OptimizedPersistable  
  {
    public enum TriggerEvent { DownloadNoServer, DownloadWithServer, Register, DownloadMono, DownloadMonoAndroid, Download35NoServer, Download35WithServer };
    CustomerContact contact;
    public DateTime eventTime;
    //public Uri url; - figure out how to store
    //public Uri urlReferrer;
    public string url;
    public string urlReferrer;
    public string anonymousID;
    public string currentExecutionFilePath;
    public string filePath;
    public bool isAuthenticated;
    public bool isLocal;
    public string requestType;
    public string userAgent;
    public string userHostAddress;
    public string userHostName;
    public string[] userLanguages;
    public TriggerEvent triggerEvent;
    public VisitEvent(TriggerEvent anEvent, CustomerContact aContact) 
    {
      eventTime = DateTime.Now;
      triggerEvent = anEvent;
      contact = aContact;
    }

    public string AnonymousID
    {
      get
      {
        return anonymousID;
      }
    }

    public CustomerContact Client
    {
      get
      {
        return contact;
      }
    }

    public string CurrentExecutionFilePath
    {
      get
      {
        return currentExecutionFilePath;
      }
    }

    public DateTime EventTime
    {
      get
      {
        return eventTime;
      }
    }

    public string FilePath
    {
      get
      {
        return filePath;
      }
    }

    public bool IsAuthenticated
    {
      get
      {
        return isAuthenticated;
      }
    }

    public bool IsLocal
    {
      get
      {
        return isLocal;
      }
    }

    public string Url
    {
      get
      {
        return url;
      }
    }

    public string UrlReferrer
    {
      get
      {
        return urlReferrer;
      }
    }

    public string UserAgent
    {
      get
      {
        return userAgent;
      }
    }

    public string UserHostAddress
    {
      get
      {
        return userHostAddress;
      }
    }

    public string UserHostName
    {
      get
      {
        return userHostName;
      }
    }

    public TriggerEvent Trigger
    {
      get
      {
        return triggerEvent;
      }
    }

    public string RequestType
    {
      get
      {
        return requestType;
      }
    }
  }
}
