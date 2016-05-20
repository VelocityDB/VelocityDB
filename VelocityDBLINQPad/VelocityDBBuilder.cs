using LINQPad.Extensibility.DataContext;
using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using VelocityDBAccess;
using UtilitiesModule;


namespace VelocityDB.LINQPad
{
    class VelocityDBBuilder
    {
        SchemaInfo schema;
        SessionInfo sessionInfo;

        public void BuildAssembly(AssemblyName pAssemblyName, string pNamespace, string pTypeName)
        {
            VelocityDBAccessBuilder lBuilder = new VelocityDBAccessBuilder(schema, sessionInfo);
            lBuilder.BuildAssembly(pAssemblyName, pNamespace, pTypeName, false);
        }
        public List<ExplorerItem> BuildSchema()
        {
            // Create a ExplorerItem for each persistable type.
            List<ExplorerItem> lSchema = (
                from Type lType in schema.PersistableTypes
                let lName = schema.TypesNameToPluralName[lType.FullName]
                orderby lName
                select new ExplorerItem(lName,
                    ExplorerItemKind.QueryableObject,
                    ExplorerIcon.Table)
                {
                    IsEnumerable = true,
                    // Use tag to create lookup below.
                    Tag = lType
                }).ToList();
            // Lookup from items to types. This helps creating links between Schema items.
            var lItemLookup = lSchema.ToLookup(lItem => ((Type)lItem.Tag));

            foreach (ExplorerItem lItem in lSchema)
            {
                Type lType = (Type)lItem.Tag;
                // Create a element schema for each persistable type.
                ExplorerItem lElementProperties = new ExplorerItem(
                    //schema.TypesNameToPluralName[lType.ToGenericTypeString()] + " Properties",
                    lType.ToGenericTypeString() + " Properties",
                    ExplorerItemKind.Schema,
                    ExplorerIcon.Schema);
                lItem.Children = new List<ExplorerItem> { lElementProperties };

                // Fill the element schema with ExplorerItems based on 
                // fields and properties of Type.
                lElementProperties.Children = SchemaExtractor.GetDataMembers(lType)
                  .OrderBy(lChildMember => lChildMember.Name)
                  .Select(lChildMember => GetChildItem(lItemLookup, lType, lChildMember))
                    .OrderBy(lChildItem => lChildItem.Kind)
                    .ToList();
            }
            return lSchema;
        }
        ExplorerItem GetChildItem(ILookup<Type, ExplorerItem> pItemLookup, Type lType, MemberInfo pMember)
        {
            Type lMemberType = ReflectionUtils.GetDataMemberType(pMember);
            // If this member references one of the collection types, add item
            // with hyperlink to it.
            if (schema.PersistableTypes.Contains(lMemberType))
            {
                return new ExplorerItem(pMember.Name,
                    ExplorerItemKind.ReferenceLink, ExplorerIcon.ManyToOne)
                {
                    HyperlinkTarget = pItemLookup[lMemberType].First(),
                    ToolTipText = lMemberType.Name
                };
            }

            // Check if member is enumerable.
            var lEnumerableInt = lMemberType
                .GetInterfaces()
                .Where(lInt => lInt.IsGenericType)
                .Where(lInt => lInt.GetGenericTypeDefinition() == typeof(IEnumerable<>));

            if (lEnumerableInt.Count() > 0)
            {
                // If enumerable of persistable type, add item with hyperlink.
                Type lElementType = 
                    lEnumerableInt.First().GetGenericArguments()[0];
                if (pItemLookup.Contains(lElementType))
                    return new ExplorerItem(pMember.Name, ExplorerItemKind.CollectionLink, ExplorerIcon.OneToMany)
                    {
                        HyperlinkTarget = pItemLookup[lElementType].First(),
                        ToolTipText = lElementType.Name
                    };
            }

            // Ordinary property:
            return new ExplorerItem(pMember.Name, ExplorerItemKind.Property, ExplorerIcon.Column)
                {
                    ToolTipText = lMemberType.Name
                };
        }
        public VelocityDBBuilder(VelocityDBDynamicDriver pDriver, VelocityDBProperties pProperties)
        {
            schema = SchemaExtractor.Extract(pProperties.ClassesFilenamesArray, pProperties.DependencyFilesArray);
            sessionInfo = new SessionInfo()
            {
                DBFolder = pProperties.DBFolder,
                Host = pProperties.Host,
                PessimisticLocking = pProperties.PessimisticLocking,
                SessionType = pProperties.SessionType,
                WindowsAuth = pProperties.WindowsAuth
            };
        }
    }
}
