using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;
using VelocityDb.Session;
using VelocityDb.TypeInfo;

namespace DatabaseManager
{
  public class PageViewModel : TreeViewItemViewModel
  {
    readonly UInt32 m_dbNum;
    readonly UInt16 m_pageNum;
    readonly SessionBase m_session;

    public PageViewModel(Page page, DatabaseViewModel parentDatabase, SessionBase session)
      : base(parentDatabase, true)
    {
      m_dbNum = page.Database.DatabaseNumber;
      m_pageNum = page.PageNumber;
      m_session = session;
    }

    public string PageName
    {
      get
      {
        Database db = m_session.OpenDatabase(m_dbNum, false, false);
        if (db != null)
        {
          Page page = m_session.OpenPage(db, m_pageNum);
          string contentType = "";
          uint typever = page.PageInfo.ShapeNumber;
          if (typever > 0)
          {
            TypeVersion tv = m_session.OpenSchema(false).GetTypeVersion(typever, m_session);
            if (tv != null && tv.Type != null)
              contentType = " of type: " + tv.Type.ToGenericTypeString();
            else
              contentType = " of type: " + "unknown with id " + typever;
          }
          return $"Page: {page.PageNumber} size: {page.PageInfo.UncompressedSize} stored size: {page.PageInfo.OnDiskSize} offset: {page.Offset} compression: " +
            $"{page.PageInfo.Compressed} {page.PageInfo.Encryption} version: {page.PageInfo.VersionNumber}  objects: {page.PageInfo.NumberOfSlots} {contentType}";
        }
        return "Failed to open " + m_dbNum;
      }
    }

    protected override void LoadChildren()
    {
      Database db = m_session.OpenDatabase(m_dbNum, false, false);
      Page page = m_session.OpenPage(db, m_pageNum);
      using (System.Windows.Application.Current.Dispatcher.DisableProcessing())
      {
        foreach (IOptimizedPersistable o in page.ObjectsLazyLoaded())
          base.Children.Add(new ObjectViewModel(o, this, m_session));
      }
    }
  }
}
