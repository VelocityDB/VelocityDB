using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Collection.BTree;
using VelocityDb.Session;

namespace VelocityDbSchema.TextIndexer
{
  public class Word : OptimizedPersistable
  {
    public const UInt32 PlaceInDatabase = 16;
    public string aWord;
    

    public Word(string word)
    {
      aWord = word;     
    }

    public override bool AllowOtherTypesOnSamePage
    {
        get
        {
            return false;
        }
    }

    public virtual BTreeSet<Document> DocumentHit
    {
        get
        {
            throw new UnexpectedException("DocumentHit - Should not be used at this level (Word)");
        }
    }
    
    public override string ToString()
    {
      return Oid + " " + aWord.ToString();
    }

    public override UInt32 PlacementDatabaseNumber
    {
      get
      {
        return PlaceInDatabase;
      }
    }

    public virtual UInt32 GlobalCount
    {
      get
      {
          throw new UnexpectedException("GlobalCount - Should not be used at this level (Word)");
      }
      set
      {
          throw new UnexpectedException("GlobalCount - Should not be used at this level (Word)");
      }
    }

    public override int GetHashCode()
    {
      return aWord.GetHashCode();
    }

    public override int CompareTo(object obj)
    {
      Word otherToken = obj as Word;
      return aWord.CompareTo(otherToken.aWord);
    }
  }
}
