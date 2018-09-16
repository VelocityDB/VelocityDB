using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb.Collection.BTree;

namespace VelocityDbSchema.Samples.Wikipedia
{
  public class XmlSchema : XmlSchemaObject
  {
    public XmlSchema()
    {
      includes = null;
      groups = null;
      iteams = null;
      notations = null;
      schemaTypes = null;
    }
    //BTreeMap<string, XmlSchemaObject> propertyGroups;
    BTreeSet<XmlSchemaObject> includes;
    BTreeMap<string, XmlSchemaObject> groups;
    BTreeSet<XmlSchemaObject> iteams;
    BTreeMap<string, XmlSchemaObject> notations;
    BTreeMap<string, XmlSchemaObject> schemaTypes;

    public BTreeMap<string, XmlSchemaObject> PropertyGroups // instead of XmlSchemaObjectTable
    { 
      get
      {
        return PropertyGroups;
      }
    }
    public BTreeSet<XmlSchemaObject> Includes  // instead of XmlSchemaObjectCollection
    {
      get
      {
        return includes;
      }
    }
    public BTreeMap<string, XmlSchemaObject> Groups  // instead of XmlSchemaObjectTable
    {
      get
      {
        return groups;
      }
    }
    public BTreeSet<XmlSchemaObject> Items  // instead of XmlSchemaObjectCollection
    { 
      get
      {
        return iteams;
      }
    } 
    public BTreeMap<string, XmlSchemaObject> Notations  // instead of XmlSchemaObjectTable
    {
      get
      {
        return notations;
      }
    }
    public XmlSchemaObject Parent { get; set; }
    public BTreeMap<string, XmlSchemaObject> SchemaTypes  // instead of XmlSchemaObjectTable
    {
      get
      {
        return schemaTypes;
      }
    }
    public string TargetNamespace { get; set; }
    public string Version { get; set; }
  }
}
