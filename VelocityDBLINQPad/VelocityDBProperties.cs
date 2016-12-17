using VelocityDBAccess;
using LINQPad.Extensibility.DataContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace VelocityDB.LINQPad
{

    class VelocityDBProperties
    {
        public const String NoServerSession = "NoServerSession";
        public const String NoServerSharedSession = "NoServerSharedSession";
        public const String ServerClientSession = "ServerClientSession";
        public const Char Separator = '|';

        readonly XElement driverData;

        /// <summary>
        /// Array of file names (with path) of dependencies loaded at
        /// runtime.
        /// </summary>
        public string[] ActualDepencies 
        {
            get
            {
                if (PrivateActualDepencies.Length > 0)
                {
                    return PrivateActualDepencies.Split(Separator);
                }
                else return new string[] { };
            }
            set
            {
                PrivateActualDepencies = 
                    String.Join(Separator.ToString(), value);
            }
        }

        /// <summary>
        /// File name (with path) of assemblies containing classes definitions.
        /// File names are separeted by Separator.
        /// </summary>
        public string ClassesFilenames
        {
            get { return (string)driverData.Element("ClassesFilenames") ?? ""; }
            set { driverData.SetElementValue("ClassesFilenames", value); }
        }

        /// <summary>
        /// Array of file names (with path) of assemblies containing class
        /// definition.
        /// </summary>
        public string[] ClassesFilenamesArray
        {
            get 
            {
                if (ClassesFilenames.Length > 0)
                {
                    return ClassesFilenames.Split(Separator);
                }
                else return new string[]{};
            }
        }

        /// <summary>
        /// File name (with path) of dependency assemblies.
        /// File names are separated by Separator.
        /// </summary>
        public string DependencyFiles
        {
            get { return (string)driverData.Element("DependencyFiles") ?? ""; }
            set { driverData.SetElementValue("DependencyFiles", value); }
        }

        /// <summary>
        /// Array of file names (with path) of dependency assemblies.
        /// </summary>
        public string[] DependencyFilesArray
        {
            get 
            {
                if (DependencyFiles.Length > 0)
                {
                    return DependencyFiles.Split(Separator);
                }
                else return new string[]{};
            }
        }

        public string DBFolder
        {
            get { return (string)driverData.Element("DBFolder") ?? ""; }
            set { driverData.SetElementValue("DBFolder", value); }
        }

        /// <summary>
        /// Database Host name or IP.
        /// </summary>
        public string Host
        {
            get { return (string)driverData.Element("Host") ?? ""; }
            set { driverData.SetElementValue("Host", value); }
        }

        public bool PessimisticLocking
        {
            get { return (bool)driverData.Element("PessimisticLocking"); }
            set { driverData.SetElementValue("PessimisticLocking", value); }
        }

        /// <summary>
        /// Used to persist ActualDependencies.
        /// </summary>
        private string PrivateActualDepencies
        {
            get { return (string)driverData.Element("ActualDepencies") ?? ""; }
            set { driverData.SetElementValue("ActualDepencies", value); }
        }

        public SessionInfo.SessionTypeEnum SessionType
        {
            get
            {
                return (SessionInfo.SessionTypeEnum)Enum
                    .Parse(typeof(SessionInfo.SessionTypeEnum),
                        (string)driverData.Element("SessionType") ?? 
                        "NoServerSession");
            }
            set { driverData.SetElementValue("SessionType", value.ToString()); }
        }

        public bool WindowsAuth
        {
            get { return (bool)driverData.Element("WindowsAuth"); }
            set { driverData.SetElementValue("WindowsAuth", value); }
        }

        public VelocityDBProperties(IConnectionInfo pCxInfo)
        {
            driverData = pCxInfo.DriverData;
        }
    }
}
