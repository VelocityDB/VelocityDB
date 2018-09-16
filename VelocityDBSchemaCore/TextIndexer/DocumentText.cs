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
    string _documentText;

    public DocumentText(string documentText, Document doc)
    {
      this._documentText = documentText;
    }

    public override bool AllowOtherTypesOnSamePage
    {
        get
        {
            return false;
        }
    }

    public string Text
    {
      get
      {
        return _documentText;
      }
    }
  }
}
