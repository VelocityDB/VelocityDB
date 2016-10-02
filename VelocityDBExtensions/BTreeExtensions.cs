using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using VelocityDb.Collection.BTree;
using VelocityDb.Collection.Comparer;
using VelocityDb.Session;
using VelocityDb.TypeInfo;

namespace VelocityDBExtensions.Extensions.BTree
{
  /// <summary>
  /// A few extensions to improve performance of Linq for Objects queries
  /// We need your HELP to improve it to cover more use cases of queries!
  /// </summary>
  public static class BTreeExtensions
  {
    private static bool HasIndexablePropertyOnLeft<Key>(Expression leftSide, BTreeBase<Key, Key> sourceCollection, DataMember dataMember, out MemberExpression theMember)
#if WINDOWS_PHONE
      where Key : new()
#endif
    {
      theMember = null;
      MemberExpression mex = leftSide as MemberExpression;
      if (leftSide.NodeType == ExpressionType.Convert)
      {
        UnaryExpression convert = leftSide as UnaryExpression;
        mex = convert.Operand as MemberExpression;
      }
      if (leftSide.NodeType == ExpressionType.Call)
      {
        MethodCallExpression call = leftSide as MethodCallExpression;
        if (call.Method.Name == "CompareString")
        {
          mex = call.Arguments[0] as MemberExpression;
        }
      }
      else if (mex == null)
        return false;
      else
      {
        theMember = mex;
        if (dataMember.Field.Name == mex.Member.Name)
          return true;
        FieldAccessor accessor = CustomAttributeExtensions.GetCustomAttribute<FieldAccessor>(theMember.Member, true);
        if (accessor != null)
        {
          return dataMember.Field.Name == accessor.FieldName;
        }
      }
      return false;
    }

    private static object GetRightValue(Expression leftSide, Expression rightSide)
    {
      if (leftSide.NodeType == ExpressionType.Call)
      {
        var call = leftSide as System.Linq.Expressions.MethodCallExpression;
        if (call.Method.Name == "CompareString")
        {
          LambdaExpression evalRight = System.Linq.Expressions.Expression.Lambda(call.Arguments[1], null);
          return evalRight.Compile().DynamicInvoke(null);
        }
      }
      //rightside is where we get our hash...
      switch (rightSide.NodeType)
      {
        //shortcut constants, dont eval, will be faster
        case ExpressionType.Constant:
          ConstantExpression constExp = (ConstantExpression)rightSide;
          return constExp.Value;
        //case ExpressionType.MemberAccess:
        //  throw new NotSupportedException("Query with field/property access on right side of expression currently not supported (swap left & right and adjust operator used)");
        //if not constant (which is provably terminal in a tree), convert back to Lambda and eval to get the hash.
        default:
          //Lambdas can be created from expressions... yay
          LambdaExpression evalRight = System.Linq.Expressions.Expression.Lambda(rightSide, null);
          return evalRight.Compile().DynamicInvoke(null);
      }
    }

    static bool canUseIndex<Key>(BTreeBase<Key, Key> sourceCollection, BinaryExpression binExp, CompareByField<Key> comparer)
#if WINDOWS_PHONE
      where Key : new()
#endif
    {
      if (comparer.FieldsToCompare == null)
        comparer.SetupFieldsToCompare();
      int indexNumberOfFields = comparer.FieldsToCompare.Length;
      while (binExp.NodeType == ExpressionType.AndAlso)
      {
        BinaryExpression leftExpr = (BinaryExpression)binExp.Left;
        BinaryExpression rightExpr = (BinaryExpression)binExp.Right;
        MemberExpression returnedEx = null;
        if (indexNumberOfFields == 0 || !HasIndexablePropertyOnLeft(rightExpr.Left, sourceCollection, comparer.FieldsToCompare[--indexNumberOfFields], out returnedEx))
          return false;
        binExp = leftExpr;
      }
      return true;
    }
    /// <summary>
    /// Override to improve performance over IEnumerable LINQ extension
    /// </summary>
    /// <typeparam name="Key">key type</typeparam>
    /// <param name="sourceCollection">the collection</param>
    /// <returns>Size of the collection</returns>
    [Obfuscation(Feature = "renaming", Exclude = true)]
    static public int Count<Key>(this BTreeBase<Key, Key> sourceCollection)
    {
      return sourceCollection.Count;
    }
    static bool GreaterThanOrEqualUseIndex<Key>(BTreeBase<Key, Key> sourceCollection, BinaryExpression binExp)
#if WINDOWS_PHONE
 where Key : new()
#endif
    {
      SessionBase session = sourceCollection.Session;
      CompareByField<Key> comparer = sourceCollection.Comparer as CompareByField<Key>;

      Expression leftSide = binExp.Left;
      Expression rightSide = binExp.Right;
      object rightValue = GetRightValue(leftSide, rightSide);
      if (leftSide.NodeType == ExpressionType.Parameter)
      {
        Key key = (Key)rightValue;
        if (key != null)
        {
          if (session.TraceIndexUsage)
            Trace.WriteLine(DateTime.Now.ToString("HH:mm:ss:fff") + " Index used with " + sourceCollection.ToString());
          return true;
        }
      }
      else
      {
        if (comparer != null)
        {
          //if we were able to create a hash from the right side (likely)
          MemberExpression returnedEx = null;
          if (comparer.FieldsToCompare == null)
            comparer.SetupFieldsToCompare();
          DataMember dataMember = comparer.FieldsToCompare[0];
          if (rightValue != null && HasIndexablePropertyOnLeft<Key>(leftSide, sourceCollection, dataMember, out returnedEx))
          {
            if (session.TraceIndexUsage)
              Trace.WriteLine(DateTime.Now.ToString("HH:mm:ss:fff") + " Index used with " + sourceCollection.ToString());
            return true;
          }
          else if (leftSide.NodeType == ExpressionType.Call)
          {
            // don't know yet how to handle TODO
            /*MethodCallExpression expression = leftSide as MethodCallExpression;
            Trace.Out.WriteLine("Method: " + expression.Method.Name);
            Trace.Out.WriteLine("Args: ");
            foreach (var exp in expression.Arguments) 
              sourceCollection where */
          }
        }
      }
      return false;
    }
    static IEnumerable<Key> GreaterThanOrEqual<Key>(BTreeBase<Key, Key> sourceCollection, BinaryExpression binExp)
#if WINDOWS_PHONE
 where Key : new()
#endif
    {
      SessionBase session = sourceCollection.Session;
      CompareByField<Key> comparer = sourceCollection.Comparer as CompareByField<Key>;

      Expression leftSide = binExp.Left;
      Expression rightSide = binExp.Right;
      object rightValue = GetRightValue(leftSide, rightSide);
      if (leftSide.NodeType == ExpressionType.Parameter)
      {
        Key key = (Key)rightValue;
        if (key != null)
        {
          BTreeSetIterator<Key> itr = sourceCollection.Iterator();
          itr.GoTo(key);
          while (itr.Next() != null)
          {
            yield return itr.Current();
          }
        }
      }
      else
      {
        if (comparer != null)
        {
          //if we were able to create a hash from the right side (likely)
          MemberExpression returnedEx = null;
#if WINDOWS_PHONE || PORTABLE || WINDOWS_UWP || NET_CORE
          Key key = (Key) Activator.CreateInstance(typeof(Key));
#else
          Key key = (Key)FormatterServices.GetUninitializedObject(typeof(Key));
#endif
          if (comparer.FieldsToCompare == null)
            comparer.SetupFieldsToCompare();
          DataMember dataMember = comparer.FieldsToCompare[0];
          if (rightValue != null && HasIndexablePropertyOnLeft<Key>(leftSide, sourceCollection, dataMember, out returnedEx))
          {
            //cast to MemberExpression - it allows us to get the property
            MemberExpression propExp = (MemberExpression)returnedEx;
            MemberInfo property = propExp.Member;
            foreach (DataMember member in comparer.FieldsToCompare.Skip(1))
            {
              if (member.GetTypeCode == TypeCode.String)
                member.SetMemberValue(key, "");
            }
            dataMember.SetMemberValueWithPossibleConvert(key, rightValue);
            BTreeSetIterator<Key> itr = sourceCollection.Iterator();
            itr.GoTo(key);
            while (itr.Current() != null)
            {
              yield return itr.Current();
              itr.Next();
            }
          }
          else if (leftSide.NodeType == ExpressionType.Call)
          {
            // don't know yet how to handle TODO
            /*MethodCallExpression expression = leftSide as MethodCallExpression;
            Trace.Out.WriteLine("Method: " + expression.Method.Name);
            Trace.Out.WriteLine("Args: ");
            foreach (var exp in expression.Arguments) 
              sourceCollection where */
          }
        }
      }
    }
    static bool GreaterThanUseIndex<Key>(BTreeBase<Key, Key> sourceCollection, BinaryExpression binExp)
#if WINDOWS_PHONE
 where Key : new()
#endif
    {
      SessionBase session = sourceCollection.Session;
      CompareByField<Key> comparer = sourceCollection.Comparer as CompareByField<Key>;
      Expression leftSide = binExp.Left;
      Expression rightSide = binExp.Right;
      object rightValue = GetRightValue(leftSide, rightSide);
      if (leftSide.NodeType == ExpressionType.Parameter)
      {
        Key key = (Key)rightValue;
        if (key != null)
        {
          if (session.TraceIndexUsage)
            Trace.WriteLine(DateTime.Now.ToString("HH:mm:ss:fff") + " Index used with " + sourceCollection.ToString());
          return true;
        }
      }
      else
      {
        if (comparer != null)
        {
          //if we were able to create a hash from the right side (likely)
          MemberExpression returnedEx = null;
          if (comparer.FieldsToCompare == null)
            comparer.SetupFieldsToCompare();
          DataMember dataMember = comparer.FieldsToCompare[0];
          if (rightValue != null && HasIndexablePropertyOnLeft<Key>(leftSide, sourceCollection, dataMember, out returnedEx))
          {
            if (session.TraceIndexUsage)
              Trace.WriteLine(DateTime.Now.ToString("HH:mm:ss:fff") + " Index used with " + sourceCollection.ToString());
            return true;
          }
        }
      }
      return false;
    }


    static IEnumerable<Key> GreaterThan<Key>(BTreeBase<Key, Key> sourceCollection, BinaryExpression binExp)
#if WINDOWS_PHONE
 where Key : new()
#endif
    {
      SessionBase session = sourceCollection.Session;
      CompareByField<Key> comparer = sourceCollection.Comparer as CompareByField<Key>;
      Expression leftSide = binExp.Left;
      Expression rightSide = binExp.Right;
      object rightValue = GetRightValue(leftSide, rightSide);
      if (leftSide.NodeType == ExpressionType.Parameter)
      {
        Key key = (Key)rightValue;
        if (key != null)
        {
          BTreeSetIterator<Key> itr = sourceCollection.Iterator();
          itr.GoTo(key);
          while (itr.Next() != null)
          {
            yield return itr.Current();
          }
        }
      }
      else
      {
        if (comparer != null)
        {
          //if we were able to create a hash from the right side (likely)
          MemberExpression returnedEx = null;
#if WINDOWS_PHONE || WINDOWS_UWP || NET_CORE
          Key key = (Key) Activator.CreateInstance(typeof(Key));
#else
          Key key = (Key)FormatterServices.GetUninitializedObject(typeof(Key));
#endif
          if (comparer.FieldsToCompare == null)
            comparer.SetupFieldsToCompare();
          DataMember dataMember = comparer.FieldsToCompare[0];
          if (rightValue != null && HasIndexablePropertyOnLeft<Key>(leftSide, sourceCollection, dataMember, out returnedEx))
          {
            //cast to MemberExpression - it allows us to get the property
            MemberExpression propExp = (MemberExpression)returnedEx;
            MemberInfo property = propExp.Member;
            foreach (DataMember member in comparer.FieldsToCompare.Skip(1))
            {
              if (member.GetTypeCode == TypeCode.String)
                member.SetMemberValue(key, "");
            }
            dataMember.SetMemberValueWithPossibleConvert(key, rightValue);
            BTreeSetIterator<Key> itr = sourceCollection.Iterator();
            itr.GoTo(key);
            var v = itr.Current();
            if (v == null)
              v = itr.Next();
            else
            {
              while (comparer.CompareField(dataMember, key, v, 0) == 0)
                v = itr.Next();
            }
            while (v != null)
            {
              yield return v;
              v = itr.Next();
            }
          }
          else if (leftSide.NodeType == ExpressionType.Call)
          {
            // don't know yet how to handle TODO
            /*MethodCallExpression expression = leftSide as MethodCallExpression;
            Trace.Out.WriteLine("Method: " + expression.Method.Name);
            Trace.Out.WriteLine("Args: ");
            foreach (var exp in expression.Arguments) 
              sourceCollection where */
          }
        }
      }
    }
    static bool LessThanOrEqualUseIndex<Key>(BTreeBase<Key, Key> sourceCollection, BinaryExpression binExp)
#if WINDOWS_PHONE
 where Key : new()
#endif
    {
      SessionBase session = sourceCollection.Session;
      CompareByField<Key> comparer = sourceCollection.Comparer as CompareByField<Key>;
      Expression leftSide = binExp.Left;
      Expression rightSide = binExp.Right;
      object rightValue = GetRightValue(leftSide, rightSide);
      if (leftSide.NodeType == ExpressionType.Parameter)
      {
        Key key = (Key)rightValue;
        if (key != null)
        {
          if (session.TraceIndexUsage)
            Trace.WriteLine(DateTime.Now.ToString("HH:mm:ss:fff") + " Index used with " + sourceCollection.ToString());
          return true;
        }
      }
      else
      {
        if (comparer != null)
        {
          //if we were able to create a hash from the right side (likely)
          MemberExpression returnedEx = null;
          if (comparer.FieldsToCompare == null)
            comparer.SetupFieldsToCompare();
          DataMember dataMember = comparer.FieldsToCompare[0];
          if (rightValue != null && HasIndexablePropertyOnLeft<Key>(leftSide, sourceCollection, dataMember, out returnedEx))
          {
            if (session.TraceIndexUsage)
              Trace.WriteLine(DateTime.Now.ToString("HH:mm:ss:fff") + " Index used with " + sourceCollection.ToString());
            return true;
          }
          else if (leftSide.NodeType == ExpressionType.Call)
          {
            // don't know yet how to handle TODO
            /*MethodCallExpression expression = leftSide as MethodCallExpression;
            Trace.Out.WriteLine("Method: " + expression.Method.Name);
            Trace.Out.WriteLine("Args: ");
            foreach (var exp in expression.Arguments) 
              sourceCollection where */
          }
        }
      }
      return false;
    }
    static IEnumerable<Key> LessThanOrEqual<Key>(BTreeBase<Key, Key> sourceCollection, BinaryExpression binExp)
#if WINDOWS_PHONE
 where Key : new()
#endif
    {
      SessionBase session = sourceCollection.Session;
      CompareByField<Key> comparer = sourceCollection.Comparer as CompareByField<Key>;

      Expression leftSide = binExp.Left;
      Expression rightSide = binExp.Right;
      object rightValue = GetRightValue(leftSide, rightSide);
      if (leftSide.NodeType == ExpressionType.Parameter)
      {
        Key key = (Key)rightValue;
        if (key != null)
        {
          BTreeSetIterator<Key> itr = sourceCollection.Iterator();
          itr.GoTo(key);
          while (itr.Current() != null)
          {
            yield return itr.Current();
            itr.Next();
          }
        }
      }
      else
      {
        if (comparer != null)
        {
          //if we were able to create a hash from the right side (likely)
          MemberExpression returnedEx = null;
#if WINDOWS_PHONE || WINDOWS_UWP || NET_CORE
          Key key = (Key) Activator.CreateInstance(typeof(Key));
#else
          Key key = (Key)FormatterServices.GetUninitializedObject(typeof(Key));
#endif
          if (comparer.FieldsToCompare == null)
            comparer.SetupFieldsToCompare();
          DataMember dataMember = comparer.FieldsToCompare[0];
          if (rightValue != null && HasIndexablePropertyOnLeft<Key>(leftSide, sourceCollection, dataMember, out returnedEx))
          {
            //cast to MemberExpression - it allows us to get the property
            MemberExpression propExp = (MemberExpression)returnedEx;
            MemberInfo property = propExp.Member;
            foreach (DataMember member in comparer.FieldsToCompare.Skip(1))
            {
              if (member.GetTypeCode == TypeCode.String)
                member.SetMemberValue(key, "");
            }
            dataMember.SetMemberValueWithPossibleConvert(key, rightValue);
            BTreeSetIterator<Key> itr = sourceCollection.Iterator();
            itr.GoTo(key);
            while (itr.Current() != null)
            {
              yield return itr.Current();
              itr.Previous();
            }
          }
          else if (leftSide.NodeType == ExpressionType.Call)
          {
            // don't know yet how to handle TODO
            /*MethodCallExpression expression = leftSide as MethodCallExpression;
            Trace.Out.WriteLine("Method: " + expression.Method.Name);
            Trace.Out.WriteLine("Args: ");
            foreach (var exp in expression.Arguments) 
              sourceCollection where */
          }
        }
      }
    }

    static bool LessThanUseIndex<Key>(BTreeBase<Key, Key> sourceCollection, BinaryExpression binExp)
#if WINDOWS_PHONE
 where Key : new()
#endif
    {
      SessionBase session = sourceCollection.Session;
      CompareByField<Key> comparer = sourceCollection.Comparer as CompareByField<Key>;
      Expression leftSide = binExp.Left;
      Expression rightSide = binExp.Right;
      object rightValue = GetRightValue(leftSide, rightSide);
      if (leftSide.NodeType == ExpressionType.Parameter)
      {
        Key key = (Key)rightValue;
        if (key != null)
        {
          if (session.TraceIndexUsage)
            Trace.WriteLine(DateTime.Now.ToString("HH:mm:ss:fff") + " Index used with " + sourceCollection.ToString());
          return true;
        }
      }
      else
      {
        if (comparer != null)
        {
          //if we were able to create a hash from the right side (likely)
          MemberExpression returnedEx = null;
          if (comparer.FieldsToCompare == null)
            comparer.SetupFieldsToCompare();
          DataMember dataMember = comparer.FieldsToCompare[0];
          if (rightValue != null && HasIndexablePropertyOnLeft<Key>(leftSide, sourceCollection, dataMember, out returnedEx))
          {
            if (session.TraceIndexUsage)
              Trace.WriteLine(DateTime.Now.ToString("HH:mm:ss:fff") + " Index used with " + sourceCollection.ToString());
            return true;
          }
          else if (leftSide.NodeType == ExpressionType.Call)
          {
            // don't know yet how to handle TODO
            /*MethodCallExpression expression = leftSide as MethodCallExpression;
            Trace.Out.WriteLine("Method: " + expression.Method.Name);
            Trace.Out.WriteLine("Args: ");
            foreach (var exp in expression.Arguments) 
              sourceCollection where */
          }
        }
      }
      return false;
    }

    static IEnumerable<Key> LessThan<Key>(BTreeBase<Key, Key> sourceCollection, BinaryExpression binExp)
#if WINDOWS_PHONE
 where Key : new()
#endif
    {
      SessionBase session = sourceCollection.Session;
      Expression leftSide = binExp.Left;
      Expression rightSide = binExp.Right;
      object rightValue = GetRightValue(leftSide, rightSide);
      CompareByField<Key> comparer = sourceCollection.Comparer as CompareByField<Key>;
      if (leftSide.NodeType == ExpressionType.Parameter)
      {
        Key key = (Key)rightValue;
        if (key != null)
        {
          BTreeSetIterator<Key> itr = sourceCollection.Iterator();
          if (itr.GoTo(key))
          {
            while (itr.Previous() != null)
            {
              yield return itr.Current();
            }
          }
        }
      }
      else
      {
        if (comparer != null)
        {
          //if we were able to create a hash from the right side (likely)
          MemberExpression returnedEx = null;
#if WINDOWS_PHONE || WINDOWS_UWP || NET_CORE
          Key key = (Key) Activator.CreateInstance(typeof(Key));
#else
          Key key = (Key)FormatterServices.GetUninitializedObject(typeof(Key));
#endif
          if (comparer.FieldsToCompare == null)
            comparer.SetupFieldsToCompare();
          DataMember dataMember = comparer.FieldsToCompare[0];
          if (rightValue != null && HasIndexablePropertyOnLeft<Key>(leftSide, sourceCollection, dataMember, out returnedEx))
          {
            //cast to MemberExpression - it allows us to get the property
            MemberExpression propExp = (MemberExpression)returnedEx;
            MemberInfo property = propExp.Member;
            foreach (DataMember member in comparer.FieldsToCompare.Skip(1))
            {
              if (member.GetTypeCode == TypeCode.String)
                member.SetMemberValue(key, "");
            }
            dataMember.SetMemberValueWithPossibleConvert(key, rightValue);
            BTreeSetIterator<Key> itr = sourceCollection.Iterator();
            itr.GoTo(key);
            var v = itr.Current();
            if (v == null)
              v = itr.Previous();
            while (v != null && comparer.CompareField(dataMember, key, v, 0) == 0)
              v = itr.Previous();
            while (v != null)
            {
              yield return v;
              v = itr.Previous();
            }
          }
          else if (leftSide.NodeType == ExpressionType.Call)
          {
            // don't know yet how to handle TODO
            /*MethodCallExpression expression = leftSide as MethodCallExpression;
            Trace.Out.WriteLine("Method: " + expression.Method.Name);
            Trace.Out.WriteLine("Args: ");
            foreach (var exp in expression.Arguments) 
              sourceCollection where */
          }
        }
      }
    }

    static bool EqualUseIndex<Key>(BTreeBase<Key, Key> sourceCollection, BinaryExpression binExp)
#if WINDOWS_PHONE
 where Key : new()
#endif
    {
      SessionBase session = sourceCollection.Session;
      CompareByField<Key> comparer = sourceCollection.Comparer as CompareByField<Key>;
      Expression leftSide = binExp.Left;
      Expression rightSide = binExp.Right;
      object rightValue = GetRightValue(leftSide, rightSide);
      if (leftSide.NodeType == ExpressionType.Parameter)
      {
        Key key = (Key)rightValue;
        if (key != null)
        {
          if (session.TraceIndexUsage)
            Trace.WriteLine(DateTime.Now.ToString("HH:mm:ss:fff") + " Index used with " + sourceCollection.ToString());
          return true;
        }
      }
      else
      {
        if (comparer != null)
        {
          MemberExpression returnedEx = null;
          if (comparer.FieldsToCompare == null)
            comparer.SetupFieldsToCompare();
          DataMember dataMember = comparer.FieldsToCompare[0];
          if (rightValue != null && HasIndexablePropertyOnLeft<Key>(leftSide, sourceCollection, dataMember, out returnedEx))
          {
            if (session.TraceIndexUsage)
              Trace.WriteLine(DateTime.Now.ToString("HH:mm:ss:fff") + " Index used with " + sourceCollection.ToString());
            return true;
          }
        }
      }
      return false;
    }

    static IEnumerable<Key> Equal<Key>(BTreeBase<Key, Key> sourceCollection, BinaryExpression binExp)
#if WINDOWS_PHONE
 where Key : new()
#endif
    {
      SessionBase session = sourceCollection.Session;
      CompareByField<Key> comparer = sourceCollection.Comparer as CompareByField<Key>;
      Expression leftSide = binExp.Left;
      Expression rightSide = binExp.Right;
      object rightValue = GetRightValue(leftSide, rightSide);
      if (leftSide.NodeType == ExpressionType.Parameter)
      {
        Key key = (Key)rightValue;
        if (key != null)
        {
          BTreeSetIterator<Key> itr = sourceCollection.Iterator();
          itr.GoTo(key);
          while (sourceCollection.Comparer.Compare(itr.Current(), key) == 0)
          {
            yield return itr.Next();
          }
        }
      }
      else
      {
        if (comparer != null)
        {
          MemberExpression returnedEx = null;
#if WINDOWS_PHONE || WINDOWS_UWP || NET_CORE
          Key key = (Key) Activator.CreateInstance(typeof(Key));
#else
          Key key = (Key)FormatterServices.GetUninitializedObject(typeof(Key));
#endif
          if (comparer.FieldsToCompare == null)
            comparer.SetupFieldsToCompare();
          DataMember dataMember = comparer.FieldsToCompare[0];
          if (rightValue != null && HasIndexablePropertyOnLeft<Key>(leftSide, sourceCollection, dataMember, out returnedEx))
          {
            MemberExpression propExp = (MemberExpression)returnedEx;
            MemberInfo property = propExp.Member;
            dataMember.SetMemberValueWithPossibleConvert(key, rightValue);
            BTreeSetIterator<Key> itr = sourceCollection.Iterator();
            itr.GoTo(key);
            Key current = itr.Current();
            while (current != null && comparer.CompareField(dataMember, key, current, 0) == 0)
            {
              yield return current;
              current = itr.Next();
            }
          }
        }
      }
    }
    static bool AndUseIndex<Key>(BTreeBase<Key, Key> sourceCollection, BinaryExpression binExp)
#if WINDOWS_PHONE
 where Key : new()
#endif
    {
      while (binExp.NodeType == ExpressionType.AndAlso)
      {
        BinaryExpression leftExpr = (BinaryExpression)binExp.Left;
        binExp = leftExpr;
        if (binExp.NodeType != ExpressionType.Equal && binExp.NodeType != ExpressionType.AndAlso)
          return false;
      }
      SessionBase session = sourceCollection.Session;
      if (session.TraceIndexUsage)
        Trace.WriteLine(DateTime.Now.ToString("HH:mm:ss:fff") + " Index used with " + sourceCollection.ToString());
      return true;
    }
    static IEnumerable<Key> And<Key>(BTreeBase<Key, Key> sourceCollection, BinaryExpression binExp)
#if WINDOWS_PHONE
 where Key : new()
#endif
    {
      CompareByField<Key> comparer = sourceCollection.Comparer as CompareByField<Key>;
      int indexNumberOfFields = comparer.FieldsToCompare.Length;
#if WINDOWS_PHONE || WINDOWS_UWP || NET_CORE
      Key key = (Key) Activator.CreateInstance(typeof(Key));
#else
      Key key = (Key)FormatterServices.GetUninitializedObject(typeof(Key));
#endif
      while (binExp.NodeType == ExpressionType.AndAlso)
      {
        BinaryExpression leftExpr = (BinaryExpression)binExp.Left;
        BinaryExpression rightExpr = (BinaryExpression)binExp.Right;
        DataMember dataMember = comparer.FieldsToCompare[--indexNumberOfFields];
        object rightValue = GetRightValue(rightExpr.Left, rightExpr.Right);
        dataMember.SetMemberValueWithPossibleConvert(key, rightValue);
        binExp = leftExpr;
        if (binExp.NodeType == ExpressionType.Equal)
        {
          dataMember = comparer.FieldsToCompare[--indexNumberOfFields];
          rightValue = GetRightValue(binExp.Left, binExp.Right);
          dataMember.SetMemberValueWithPossibleConvert(key, rightValue);
        }
      }
      BTreeSetIterator<Key> itr = sourceCollection.Iterator();
      itr.GoTo(key);
      Key current = itr.Current();
      while (current != null && comparer.Compare(key, current) == 0)
      {
        yield return current;
        current = itr.Next();
      }
    }
    /// <summary>
    /// Override to improve performance over IEnumerable LINQ extension
    /// </summary>
    /// <typeparam name="Key">key type</typeparam>
    /// <param name="sourceCollection">the collection</param>
    /// <param name="expr">an expression</param>
    /// <returns>Enumeration of collection where the expression evaluates to true</returns>
    [Obfuscation(Feature = "renaming", Exclude = true)]
    static public IEnumerable<Key> Where<Key>(this BTreeBase<Key, Key> sourceCollection, Expression<Func<Key, bool>> expr)
#if WINDOWS_PHONE
 where Key : new()
#endif
    {
      if (sourceCollection != null)
      {

        bool noIndex = true;
        SessionBase session = sourceCollection.Session;
        CompareByField<Key> comparer = sourceCollection.Comparer as CompareByField<Key>;
        BinaryExpression binExp = expr.Body as BinaryExpression;
        if (binExp != null && canUseIndex<Key>(sourceCollection, binExp, comparer))
        {
          session.WaitForIndexUpdates();
          switch (expr.Body.NodeType)
          {
            case ExpressionType.AndAlso:
              {
                noIndex = AndUseIndex(sourceCollection, binExp) == false;
                if (noIndex == false)
                {
                  foreach (var x in And<Key>(sourceCollection, binExp))
                    yield return x;
                }
                else
                {
                  BinaryExpression leftExpr = (BinaryExpression)binExp.Left;
                  binExp = leftExpr;
                  switch (binExp.NodeType)
                  {
                    case ExpressionType.Equal:
                      {
                        noIndex = EqualUseIndex<Key>(sourceCollection, binExp) == false;
                        if (noIndex == false)
                        {
                          IEnumerable<Key> equal = Equal<Key>(sourceCollection, binExp);
                          IEnumerable<Key> result = equal.Where<Key>(expr.Compile());
                          foreach (Key resultItem in result)
                            yield return resultItem;
                        }
                        yield break;
                      }
                    case ExpressionType.LessThan:
                      {
                        noIndex = LessThanUseIndex<Key>(sourceCollection, binExp) == false;
                        if (noIndex == false)
                        {
                          IEnumerable<Key> lessThan = LessThan<Key>(sourceCollection, binExp);
                          IEnumerable<Key> result = lessThan.Where<Key>(expr.Compile());
                          foreach (Key resultItem in result)
                            yield return resultItem;
                        }
                        yield break;
                      }
                    case ExpressionType.LessThanOrEqual:
                      {
                        noIndex = LessThanOrEqualUseIndex<Key>(sourceCollection, binExp) == false;
                        if (noIndex == false)
                        {
                          IEnumerable<Key> lessThan = LessThanOrEqual<Key>(sourceCollection, binExp);
                          IEnumerable<Key> result = lessThan.Where<Key>(expr.Compile());
                          foreach (Key resultItem in result)
                            yield return resultItem;
                        }
                        yield break;
                      }
                    case ExpressionType.GreaterThan:
                      {
                        noIndex = GreaterThanUseIndex<Key>(sourceCollection, binExp) == false;
                        if (noIndex == false)
                        {
                          IEnumerable<Key> greaterThan = GreaterThan<Key>(sourceCollection, binExp);
                          IEnumerable<Key> result = greaterThan.Where<Key>(expr.Compile());
                          foreach (Key resultItem in result)
                            yield return resultItem;
                        }
                        yield break;
                      }
                    case ExpressionType.GreaterThanOrEqual:
                      {
                        noIndex = GreaterThanOrEqualUseIndex<Key>(sourceCollection, binExp) == false;
                        if (noIndex == false)
                        {
                          IEnumerable<Key> greaterThan = GreaterThanOrEqual<Key>(sourceCollection, binExp);
                          IEnumerable<Key> result = greaterThan.Where<Key>(expr.Compile());
                          foreach (Key resultItem in result)
                            yield return resultItem;
                        }
                        yield break;
                      }
                  };
                }
              }
              break;
            case ExpressionType.Equal:
              {
                noIndex = EqualUseIndex<Key>(sourceCollection, binExp) == false;
                if (noIndex == false)
                  foreach (var x in Equal<Key>(sourceCollection, binExp))
                    yield return x;
              }
              break;
            case ExpressionType.GreaterThan:
              {
                noIndex = GreaterThanUseIndex<Key>(sourceCollection, binExp) == false;
                if (noIndex == false)
                  foreach (var x in GreaterThan<Key>(sourceCollection, binExp))
                    yield return x;
              }
              break;
            case ExpressionType.GreaterThanOrEqual:
              {
                noIndex = GreaterThanOrEqualUseIndex<Key>(sourceCollection, binExp) == false;
                if (noIndex == false)
                  foreach (var x in GreaterThanOrEqual<Key>(sourceCollection, binExp))
                    yield return x;
              }
              break;
            case ExpressionType.LessThan:
              {
                noIndex = LessThanUseIndex<Key>(sourceCollection, binExp) == false;
                if (noIndex == false)
                  foreach (var x in LessThan<Key>(sourceCollection, binExp))
                    yield return x;
              }
              break;
            case ExpressionType.LessThanOrEqual:
              {
                noIndex = LessThanOrEqualUseIndex<Key>(sourceCollection, binExp) == false;
                if (noIndex == false)
                  foreach (var x in LessThanOrEqual<Key>(sourceCollection, binExp))
                    yield return x;
              }
              break;
          }
        }
        if (noIndex) //no index?  just do it the normal slow way then...
        {
          IEnumerable<Key> sourceEnum;
          if (sourceCollection.UsesOidShort)
          {
            BTreeSetOidShort<Key> c = (BTreeSetOidShort<Key>)sourceCollection;
            sourceEnum = c.AsEnumerable<Key>();
          }
          else
          {
            BTreeSet<Key> c = (BTreeSet<Key>)sourceCollection;
            sourceEnum = c.AsEnumerable<Key>();
          }
          IEnumerable<Key> result = sourceEnum.Where<Key>(expr.Compile());
          foreach (Key resultItem in result)
            yield return resultItem;
        }
      }
    }

#if NET35
    // Only useful before .NET 4
    public static void CopyTo(this System.IO.Stream input, System.IO.Stream output)
    {
      byte[] buffer = new byte[16 * 1024]; // Fairly arbitrary size
      int bytesRead;

      while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
      {
        output.Write(buffer, 0, bytesRead);
      }
    }
#endif
  }
}
