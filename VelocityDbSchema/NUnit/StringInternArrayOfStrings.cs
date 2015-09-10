using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;

namespace VelocityDbSchema.NUnit
{
  public class StringInternArrayOfStrings : OptimizedPersistable
  {
    string[] m_stringArray;
    public StringInternArrayOfStrings(int arraySize)
    {
      m_stringArray = new string[arraySize];
      for (int i = 0; i < arraySize; i++ )
        m_stringArray[i] = "Kinga Persson";
    }
  }
}
