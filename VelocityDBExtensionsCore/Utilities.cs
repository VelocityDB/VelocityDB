using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using VelocityDb;
using VelocityDb.Session;
using VelocityDb.TypeInfo;

namespace VelocityDBExtensions
{
  /// <summary>
  /// Currently only used by Database Manager
  /// </summary>
  public static class Utilities
  {
    /// <summary>
    /// Outputs a string representing an array
    /// </summary>
    /// <param name="array">The array to represent as a string</param>
    /// <param name="isEncodedOidArray">True if <see cref="Oid"/> is encoded as a UInt32 or UInt64</param>
    /// <param name="page">The page containing the array</param>
    /// <param name="elementType">The element <see cref="Type"/></param>
    /// <param name="prefix">A prefix to use before each array element in the output string</param>
    /// <returns>A <see cref="string"/> representing the array.</returns>
    public static string ArrayToString(Array array, bool isEncodedOidArray, Page page, Type elementType, string prefix = "\t")
    {
      IOptimizedPersistable pObj;
      StringBuilder sb = new StringBuilder(100);
      int i = 0;
      SessionBase session = page.Database.GetSession();
      TypeCode tCode = elementType.GetTypeCode();
      bool isValueType = elementType.GetTypeInfo().IsValueType;
      foreach (object arrayObj in array)
      {
        if (isValueType == false || i % 10 == 0)
        {
          if (i > 0)
            sb.AppendLine();
          if (arrayObj == null)
            sb.Append(prefix + "[" + i.ToString() + "]\t" + "null");
          else
          {
            pObj = arrayObj as IOptimizedPersistable;
            if (pObj != null)
              sb.Append(prefix + "[" + i.ToString() + "]\t" + Oid.AsString(pObj.Id));
            else
            {
              bool foundIt = session.GlobalObjWrapperGet(arrayObj, out pObj);
              if (foundIt)
                sb.Append(prefix + "[" + i.ToString() + "]\t" + Oid.AsString(pObj.Id));
              else if (arrayObj is Array)
              {
                Array a = arrayObj as Array;
                sb.Append(prefix + "[" + i.ToString() + "]\t" + arrayObj.ToString() + " = { ");
                foreach (object obj in a)
                  sb.Append(obj.ToString() + " ");
                sb.Append("}");
              }
              else if (isEncodedOidArray)
              {
                if (tCode == TypeCode.UInt32)
                  sb.Append(prefix + "[" + i.ToString() + "]\t" + new OidShort((UInt32)arrayObj).ToString());
                else
                  sb.Append(prefix + "[" + i.ToString() + "]\t" + new Oid((UInt64)arrayObj).ToString());

              }
              else if (elementType.IsEnum)
                sb.Append(prefix + "[" + i.ToString() + "]\t" + Enum.ToObject(elementType, arrayObj).ToString());
              else
                sb.Append(prefix + "[" + i.ToString() + "]\t" + arrayObj.ToString());
            }
          }
        }
        else
        {
          if (arrayObj == null)
            sb.Append("\tnull");
          else
          {
            pObj = arrayObj as IOptimizedPersistable;
            if (pObj != null)
              sb.Append("\t" + Oid.AsString(pObj.Id));
            else
            {
              bool foundIt = session.GlobalObjWrapperGet(arrayObj, out pObj);
              if (foundIt)
                sb.Append("\t" + Oid.AsString(pObj.Id));
              else if (isEncodedOidArray)
              {
                if (tCode == TypeCode.UInt32)
                  sb.Append($"\t{new OidShort((UInt32)arrayObj).ToString()}");
                else
                  sb.Append("\t" + new Oid((UInt64)arrayObj).ToString());
              }
              else if (elementType.IsEnum)
                sb.Append($"\t{Enum.ToObject(elementType, arrayObj).ToString()}");
              else
                sb.Append($"\t{arrayObj.ToString()}");
            }
          }
        }
        i++;
      }
      return sb.ToString();
    }

    /// <summary>
    /// Currently only used by Database Manager
    /// </summary>
    /// <param name="pObj">Object for which we want detailed to string data</param>
    /// <param name="schema">The active schema</param>
    /// <param name="typeVersion">describes the type of the pObj</param>
    /// <param name="skipArrays">if <c>true</c> include array data in generated string</param>
    /// <returns>content of an object as string</returns>
    static public string ToStringDetails(this OptimizedPersistable pObj, Schema schema, TypeVersion typeVersion, bool skipArrays)
    {
      object obj = pObj.GetWrappedObject() ?? pObj;
      return ToStringDetails(obj, schema, pObj.GetPage(), typeVersion, skipArrays);
    }

    /// <summary>
    /// Object details as a string
    /// </summary>
    /// <param name="pObj">The object extended</param>
    /// <param name="session">The session managing this object</param>
    /// <param name="skipArrays">Indicates if string should contain detailed array data.</param>
    ///<returns><see cref="string"/> containing all details of this object.</returns>
    static public string ToStringDetails(this OptimizedPersistable pObj, SessionBase session, bool skipArrays = true)
    {
      Schema schema = session.OpenSchema(false);
      if (pObj.GetWrappedObject() == null)
        return pObj.ToString() + pObj.ToStringDetails(schema, pObj.GetTypeVersion(), skipArrays);
      else
      {
        Array array = pObj.GetWrappedObject() as Array;
        if (array != null)
          return pObj.GetWrappedObject().ToString() + " (" + array.Length + ") " + Oid.AsString(pObj.Id) + pObj.ToStringDetails(schema, pObj.GetTypeVersion(), skipArrays);
        else
          return pObj.GetWrappedObject().ToString() + " " + Oid.AsString(pObj.Id) + pObj.ToStringDetails(schema, pObj.GetTypeVersion(), skipArrays);
      }
    }

    /// <summary>
    /// This is a support function for the VelocityDbBrowser. It converts a field into a string.
    /// </summary>
    /// <param name="member">A field in an object</param>
    /// <param name="obj">The object containing the field</param>
    /// <param name="page">The page of the object</param>
    /// <param name="skipArrays">Option to skip arrays of the object</param>
    /// <returns>A <see cref="string"/> containing all details of this field.</returns>
    public static string ToStringDetails(DataMember member, object obj, IOptimizedPersistable pObj, Page page, bool skipArrays)
    {
      SessionBase session = pObj.GetPage().Database.GetSession();
      IOptimizedPersistable placeHolder;
      Schema schema = session.OpenSchema(false);
      FieldInfo field = member.Field;
      object o = member.GetMemberValue(obj);
      if (member.IsGuid)
      {
        Guid guid = (Guid)o;
        return guid.ToString();
      }
      StringBuilder sb = new StringBuilder(100);
      if (o == null)
        sb.Append("  " + member.FieldName + " : " + "null");
      else
      {
        bool foundIt = session.GlobalObjWrapperGet(o, out placeHolder);
        if (foundIt)
          sb.Append("  " + member.FieldName + " : " + placeHolder.GetWrappedObject().ToString() + " " + Oid.AsString(placeHolder.Id));
        else
        {
          Array array = o as Array;
          if (array != null)
          {
            Type elementType = member.FieldType.GetElementType();
            sb.Append("  " + member.FieldName + " " + field.FieldType.ToGenericTypeString() + " size: " + array.Length.ToString());
            if (!skipArrays)
              sb.Append(ArrayToString(array, false, page, elementType));
          }
          else
          {
            IList list = o as IList;
            if (list != null)
            {
              int i = 0;
              string listObjStr = "  " + member.FieldName + " " + o.GetType().ToGenericTypeString() + " size: " + list.Count.ToString();
              sb.Append(listObjStr);
              if (!skipArrays)
                foreach (object listObj in list)
                {
                  sb.AppendLine();
                  pObj = listObj as OptimizedPersistable;
                  if (listObj != null && pObj != null)
                    sb.Append("\t[" + i.ToString() + "]\t" + Oid.AsString(pObj.Id));
                  else
                  {
                    foundIt = session.GlobalObjWrapperGet(listObj, out placeHolder);
                    if (foundIt)
                      sb.Append("\t[" + i.ToString() + "]\t" + Oid.AsString(placeHolder.Id));
                    else
                      sb.Append("\t[" + i.ToString() + "]\t" + listObj.ToString());
                  }
                  i++;
                }
            }
            else
            {
              VelocityDbType t = null;
              if (field.FieldType == CommonTypes.s_typeOfType)
              {
                Type fieldType = o as Type;
                sb.Append("  " + field.Name + " : " + fieldType.ToGenericTypeString());
              }
              else
              {
                bool cond1 = field.FieldType.GetTypeInfo().IsPrimitive || member.HasId || field.FieldType == CommonTypes.s_typeOfString || field.FieldType.GetTypeInfo().IsEnum;
                if (cond1 || schema.LookupByType.TryGetValue(field.FieldType, out t) == false ||
                  (field.FieldType.GetTypeInfo().IsGenericType && field.FieldType.GetGenericTypeDefinition() == CommonTypes.s_typeOfWeakIOptimizedPersistableReference))
                  sb.Append("  " + field.Name + " : " + o.ToString());
                else
                {
                  TypeVersion memberShape = t.LastShape();
                  bool isNullable = memberShape.Type.GetTypeInfo().IsGenericType && memberShape.Type.GetGenericTypeDefinition() == CommonTypes.s_typeOfNullable;
                  if (isNullable)
                  {
                    Type elementType = memberShape.Type.GetTypeInfo().GetGenericArguments()[0];
                    schema.LookupByType.TryGetValue(elementType, out t);
                    memberShape = t.LastShape();
                  }
                  sb.Append("  " + field.Name + " : " + ToStringDetails(o, schema, page, memberShape, skipArrays));
                }
              }
            }
          }
        }
      }
      return sb.ToString();
    }

    internal static string ToStringDetails(object obj, Schema schema, Page page, TypeVersion _shape, bool skipArrays)
    {
      OptimizedPersistable pObj = obj as OptimizedPersistable;
      if (pObj != null && pObj.GetWrappedObject() != null)
        obj = pObj.GetWrappedObject();
      IOptimizedPersistable ipObj = pObj;
      StringBuilder sb = new StringBuilder(100);
      Array array = obj as Array;
      SessionBase session = page.Database.GetSession();
      if (array != null && !skipArrays)
      {
        int i = 0;
        bool isValueType = array.GetType().GetElementType().GetTypeInfo().IsValueType;
        foreach (object arrayObj in array)
        {
          if (isValueType == false || i % 10 == 0)
          {
            //sb.AppendLine();
            if (arrayObj == null)
              sb.Append("\t[" + i.ToString() + "]\t" + "null");
            else
            {
              ipObj = arrayObj as IOptimizedPersistable;
              if (arrayObj != null && ipObj != null)
                sb.Append("\t[" + i.ToString() + "]\t" + Oid.AsString(ipObj.Id));
              else
              {
                bool foundIt = session.GlobalObjWrapperGet(arrayObj, out ipObj);
                if (foundIt)
                  sb.Append("\t[" + i.ToString() + "]\t" + Oid.AsString(ipObj.Id));
                else
                  sb.Append("\t[" + i.ToString() + "]\t" + arrayObj.ToString());
              }
            }
          }
          else
          {
            if (arrayObj == null)
              sb.Append("\t" + "null");
            else
            {
              ipObj = arrayObj as IOptimizedPersistable;
              if (arrayObj != null && ipObj != null)
                sb.Append("\t" + Oid.AsString(ipObj.Id));
              else
              {
                bool foundIt = session.GlobalObjWrapperGet(arrayObj, out ipObj);
                if (foundIt)
                  sb.Append("\t" + Oid.AsString(ipObj.Id));
                else
                  sb.Append("\t" + arrayObj.ToString());
              }
            }
          }
          i++;
        }
      }
      else
      {
        if (_shape.BaseShape != null && _shape.BaseShape.Type != CommonTypes.s_typeOfValueType)
        {
          //TypeVersion baseClassShape = schema.lookupByNumber.TypeVersionLookup(_shape.baseShape);
          sb.Append(ToStringDetails(obj, schema, page, _shape.BaseShape, skipArrays));
        }
        if (_shape.DataMemberArray.Length == 0 && sb.Length == 0)
          sb.Append(obj.ToString()); // an object without member fields (like used in NodaTime)
        else
          foreach (DataMember m in _shape.DataMemberArray)
          {
            FieldInfo field = m.GetField(_shape.Type);
            object o = m.GetMemberValue(obj);
            //sb.AppendLine();
            if (o == null)
              sb.Append("  " + field.Name + " : " + "null");
            else
            {
              bool foundIt = session.GlobalObjWrapperGet(o, out ipObj);
              if (foundIt)
                sb.Append("  " + field.Name + " : " + pObj.GetWrappedObject().ToString() + " " + Oid.AsString(ipObj.Id));
              else
              {
                array = o as Array;
                if (array != null)
                {
                  Type elementType = m.FieldType.GetElementType();
                  sb.Append("  " + field.Name + " " + field.FieldType.ToGenericTypeString());
                  if (!skipArrays)
                    sb.Append(ArrayToString(array, false, page, elementType));
                }
                else
                {
                  IList list = o as IList;
                  if (list != null)
                  {
                    int i = 0;
                    sb.Append("  " + field.Name + " " + o.ToString());
                    foreach (object listObj in list)
                    {
                      //sb.AppendLine();
                      ipObj = listObj as IOptimizedPersistable;
                      if (listObj != null && pObj != null)
                        sb.Append("\t[" + i.ToString() + "]\t" + Oid.AsString(ipObj.Id));
                      else
                      {
                        if (session.GlobalObjWrapperGet(listObj, out ipObj))
                          sb.Append("\t[" + i.ToString() + "]\t" + Oid.AsString(ipObj.Id));
                        else
                          sb.Append("\t[" + i.ToString() + "]\t" + listObj.ToString());
                      }
                      i++;
                    }
                  }
                  else if (field.FieldType.GetTypeCode() != TypeCode.Object || m.HasId || !field.FieldType.GetTypeInfo().IsSerializable || (o as WeakIOptimizedPersistableReferenceBase) != null)
                    sb.Append("  " + field.Name + " : " + o.ToString());
                  else
                  {
                    TypeVersion memberShape = schema.RegisterClass(field.FieldType, session);
                    sb.Append("  " + field.Name + " : " + ToStringDetails(o, schema, page, memberShape, skipArrays));
                  }
                }
              }
            }
          }
      }
      return sb.ToString();
    }
  }
}
