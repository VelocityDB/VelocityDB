using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Session;

namespace VelocityDbSchema.TextIndexer
{
  public class DocumentText : OptimizedPersistable
  {
    public const UInt32 PlaceInDatabase = 15;
    string documentText;
    UInt32 documentShortId;

    public DocumentText(string documentText, Document doc)
    {
      this.documentText = documentText;
      documentShortId = (uint) doc.Id;
    }

    public Document Document
    {
      get
      {
          return (Document) Open(documentShortId);
      }
    }

    public override bool AllowOtherTypesOnSamePage
    {
        get
        {
            return false;
        }
    }

    public override UInt32 PlacementDatabaseNumber
    {
      get
      {
        return PlaceInDatabase;
      }
    }

    public string Text
    {
      get
      {
        return documentText;
      }
    }
  }
}
