using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
using VelocityDb.Session;
using System.IO;
using System.Threading;
using VelocityDbSchema.NUnit;
using NUnit.Framework;

namespace UnitTests
{
    [TestFixture]
    public class VelocityDB
    {
        /// <summary>
        /// Tests the caching and re using of sessiosn to increase perfomance by avoding
        /// creating new sessions for each client request
        /// </summary>
       // [Test]
        public void CachedSessionTest()
        {
            ServerClientSession lSession = GetCachedSession();
            //lets simulate that we did some prior work, return the session and get it again
            ReturnSessionToCache(lSession);
            lSession = GetCachedSession();
            TestClass lTestClass = new TestClass();
            lTestClass.SomeIntVar = 123;
            lTestClass.SomeStringVar = "test";
            lTestClass.Persist(lSession, lTestClass);
            lSession.Commit();
            //return to cache, get it again and query the object
            //as this test is to verify it does not hang we do it in separate therad and kill after timeout
            ReturnSessionToCache(lSession);
            Thread lThread=new Thread(new ThreadStart(()=>
            {
                lSession = GetCachedSession();
                counter = lSession.AllObjects<TestClass>(true, false).Count();
                ReturnSessionToCache(lSession);
            }));
            lThread.Start();
            lEvent.WaitOne(5000);
            if(lThread.IsAlive)lThread.Abort();
            Assert.AreNotEqual(0, counter, "Invalid nr of objects retreived");
        }


        /// <summary>
        /// Gets a cached session.If no session is available in the cache, a new one will be created.
        /// </summary>
        /// <returns></returns>
        ServerClientSession GetCachedSession()
        {
            ServerClientSession lSession = null;
            lock (sessions)
            {
                //if this is the fiirst call or we are out of sesssions create a new one
                if (sessions.Count == 0)
                {
                    lSession= new ServerClientSession(GetDBDIrectory(), Environment.MachineName);

                }
                else
                {
                    lSession = sessions[0];
                    sessions.RemoveAt(0);
                }
            }
            lSession.BeginUpdate();
            return lSession;
        }

        /// <summary>
        /// Returns a session to the cache.To be sure we have no transactiona ctive we abort in case
        /// the caller of this method did not commit or abort
        /// </summary>
        void ReturnSessionToCache(ServerClientSession pSession)
        {
            pSession.Abort();
            lock (sessions)
            {
                sessions.Add(pSession);
            }
        }

        private string GetDBDIrectory()
        {
            string lDBLocation=System.Configuration.ConfigurationManager.AppSettings["DBURL"];
            Assert.IsNotNull(lDBLocation, "Missing DBURL in App.config");
            if (Directory.Exists(lDBLocation) == false)
            {
                try
                {
                    Directory.CreateDirectory(lDBLocation);
                }
                catch (Exception pE)
                {

                    throw new Exception(string.Format("Cannot create directory {0}, check security settings. Error:{1}",
                        lDBLocation, pE.Message));
                }

            }
            return lDBLocation;
        }

        static AutoResetEvent lEvent = new AutoResetEvent(false);
        static int counter = 0;
        static List<ServerClientSession> sessions = new List<ServerClientSession>();
    }
}
