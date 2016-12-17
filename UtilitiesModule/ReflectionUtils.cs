using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace UtilitiesModule
{
    /// <summary>
    /// Helper methods used with reflection.
    /// </summary>
    public class ReflectionUtils
    {
        /// <summary>
        /// Get type of a MemberInfo which is a FieldInfo or PropertyInfo.
        /// </summary>
        /// <param name="pMember"></param>
        /// <returns></returns>
        public static Type GetDataMemberType(MemberInfo pMember)
        {
            switch (pMember.MemberType)
            {
                case MemberTypes.Field:
                    return ((FieldInfo)pMember).FieldType;
                case MemberTypes.Property:
                    return ((PropertyInfo)pMember).PropertyType;
                default:
                    throw new ArgumentException("Input MemberInfo must be of type FieldInfo or PropertyInfo.");
            }
        }
        /// <summary>
        /// Get value of a MemberInfo which is a FieldInfo or PropertyInfo.
        /// </summary>
        /// <param name="pMember"></param>
        /// <param name="pObj"></param>
        /// <returns></returns>
        public static object GetDataMemberValue(MemberInfo pMember, object pObj)
        {
            switch (pMember.MemberType)
            {
                case MemberTypes.Field:
                    return ((FieldInfo)pMember).GetValue(pObj);
                case MemberTypes.Property:
                    PropertyInfo pProp = (PropertyInfo)pMember;
                    // For indexed properties, return object's enumerator.     
                    if (pProp.GetIndexParameters().Count() > 0)
                    {
                        return ((IEnumerable)pObj).GetEnumerator();
                    }
                    return pProp.GetValue(pObj, null);
                default:
                    throw new ArgumentException("Input MemberInfo must be of type FieldInfo or PropertyInfo.");
            }
        }


        //*********************************************************
        //
        //    Copyright (c) Microsoft. All rights reserved.
        //    This code is licensed under the Microsoft Public License.
        //    THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
        //    ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
        //    IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
        //    PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
        //
        //*********************************************************
        /// <summary>
        /// Finds type that implements IEnumerable so can get elemtent type
        /// </summary>
        /// <param name="pSeqType">The Type to check</param>
        /// <returns>returns the type which implements IEnumerable</returns>
        public static Type FindIEnumerable(Type pSeqType)
        {
            if (pSeqType == null || pSeqType == typeof(string))
            {
                return null;
            }

            if (pSeqType.IsArray)
            {
                return typeof(IEnumerable<>)
                    .MakeGenericType(pSeqType.GetElementType());
            }

            if (pSeqType.IsGenericType)
            {
                foreach (Type arg in pSeqType.GetGenericArguments())
                {
                    Type ienum = typeof(IEnumerable<>).MakeGenericType(arg);
                    if (ienum.IsAssignableFrom(pSeqType))
                    {
                        return ienum;
                    }
                }
            }

            Type[] ifaces = pSeqType.GetInterfaces();
            if (ifaces != null && ifaces.Length > 0)
            {
                foreach (Type iface in ifaces)
                {
                    Type ienum = FindIEnumerable(iface);
                    if (ienum != null)
                    {
                        return ienum;
                    }
                }
            }

            if (pSeqType.BaseType != null &&
                pSeqType.BaseType != typeof(object))
            {
                return FindIEnumerable(pSeqType.BaseType);
            }

            return null;
        }
        /// <summary>
        /// Gets the element type for enumerable
        /// </summary>
        /// <param name="pType">The pType to inspect.</param>
        /// <returns>If the <paramref name="pType"/> was IEnumerable 
        /// then it returns the pType of a single element
        /// otherwise it returns null.</returns>
        public static Type GetIEnumerableElementType(Type pType)
        {
            Type lIEnum = FindIEnumerable(pType);
            if (lIEnum == null)
            {
                return null;
            }

            return lIEnum.GetGenericArguments()[0];
        }
        //*********************************************************
        // End Microsoft code.
        //*********************************************************
    }
}
