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
    readonly Page _page;
    readonly SessionBase _session;

    public PageViewModel(Page page, DatabaseViewModel parentDatabase, SessionBase session)
      : base(parentDatabase, true)
    {
      _page = page;
      _session = session;
    }

    public string PageName
    {
      get
      {
        string contentType = "";
        uint typever = _page.PageInfo.ShapeNumber;
        if (typever > 0)
        {
          TypeVersion tv = _session.OpenSchema(false).GetTypeVersion(typever, _session);
          if (tv != null && tv.Type != null)
            contentType = " of type: " + tv.Type.ToGenericTypeString();
          else
            contentType = " of type: " + "unknown with id " + typever;
        }
        return "Page: " + _page.PageNumber.ToString() + " size: " +
        _page.PageInfo.UncompressedSize + " stored size: " + _page.PageInfo.OnDiskSize +  " compression: " + _page.PageInfo.Compressed + " " + _page.PageInfo.Encryption + " version: " + _page.PageInfo.VersionNumber + " objects: " + _page.PageInfo.NumberOfSlots + contentType;
      }
    }

    protected override void LoadChildren()
    {
      foreach (IOptimizedPersistable o in _page)
        base.Children.Add(new ObjectViewModel(o, this, _session));
    }
  }
}
