using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace VelocityDbSchema.NUnit
{

    [Serializable]
    public class BomTable
        : ISerializable
    {
        #region Variabili private

        [NonSerialized]
        private string _BomName;
        private int _TotalRow;
        private int _TotalCols;
        //private List<BomTableRow> _Rows;
        private List<string> _Header;

        #endregion

        #region Proprieta'

        public string BomName
        {
            get { return _BomName; }
            set { _BomName = value; }
        }

        public int TotalRow
        {
            get { return _TotalRow; }
            set { _TotalRow = value; }
        }

        public int TotalCols
        {
            get { return _TotalCols; }
            set { _TotalCols = value; }
        }

        public List<string> Header
        {
            get { return _Header; }
            set { _Header = value; }
        }

        //public List<BomTableRow> Rows
        //{
        //    get { return _Rows; }
        //    set { _Rows = value; }
        //}

        #endregion

        public BomTable()
        {
            //Rows = new List<BomTableRow>();
            //Header = new List<string>();
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(_BomName), _BomName);
            info.AddValue(nameof(_TotalRow), _TotalRow);
            info.AddValue(nameof(_TotalCols), _TotalCols);
            //info.AddValue(nameof(_Rows), _Rows);
            info.AddValue(nameof(_Header), _Header);
        }

        public BomTable(SerializationInfo info, StreamingContext context)
            : this()
        {
            _BomName = info.GetString(nameof(_BomName));
            _TotalRow = info.GetInt32(nameof(_TotalRow));
            _TotalCols = info.GetInt32(nameof(_TotalCols));
            //_Rows = (List<BomTableRow>)info.GetValue(nameof(_Rows), typeof(List<BomTableRow>));
            _Header = (List<string>)info.GetValue(nameof(_Header), typeof(List<string>));
        }
    }

    //[Serializable]
    //public class BomTableRow
    //    : ISerializable
    //{

    //    private List<string> _Cells;

    //    public List<string> Cells
    //    {
    //        get { return _Cells; }
    //        set { _Cells = value; }
    //    }

    //    public BomTableRow()
    //    {
    //        _Cells = new List<string>();
    //    }

    //    public BomTableRow(SerializationInfo info, StreamingContext context)
    //        : this()
    //    {
    //        _Cells = (List<string>)info.GetValue(nameof(_Cells), typeof(List<string>));
    //    }

    //    public void GetObjectData(SerializationInfo info, StreamingContext context)
    //    {
    //        info.AddValue(nameof(_Cells), _Cells);
    //    }
    //}
}
