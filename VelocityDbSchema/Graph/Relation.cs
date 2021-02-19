using VelocityDb;
using VelocityDb.Indexing;
using VelocityDb.TypeInfo;

namespace RelSandbox
{
    public class Relation : OptimizedPersistable
    {
        #region How property
        [Index]
        string _how;

        [FieldAccessor("_how")]
        public string How
        {
            get
            {
                return _how;
            }
            set
            {
                Update();
                _how = value;
                //ResetIx1();
                //ResetIx2();
            }
        }
        #endregion

        #region StartNodeId property
        [Index]
        ulong _startNodeId;

        [FieldAccessor("_startNodeId")]
        public ulong StartNodeId
        {
            get
            {
                return _startNodeId;
            }
            set
            {
                Update();
                _startNodeId = value;
                //ResetIx1();
            }
        }
        #endregion

        #region EndNodeId property
        [Index]
        ulong _endNodeId;

        [FieldAccessor("_endNodeId")]
        public ulong EndNodeId
        {
            get
            {
                return _endNodeId;
            }
            set
            {
                Update();
                _endNodeId = value;
                //ResetIx2();
            }
        }
        #endregion

        #region Ix1 index
        //[Index]
        //internal string _ix1;

        //void ResetIx1()
        //{
        //    _ix1 = How + StartNodeId;
        //}
        #endregion

        #region Ix2 index
        //[Index]
        //internal string _ix2;

        //void ResetIx2()
        //{
        //    _ix2 = How + EndNodeId;
        //}
        #endregion

        #region Object overrides
        //public override int GetHashCode()
        //{
        //    return How.GetHashCode() ^ StartNodeId.GetHashCode() ^ EndNodeId.GetHashCode();
        //}

        //public override bool Equals(object obj)
        //{
        //    if (obj is Relation r)
        //        return How == r.How && StartNodeId == r.StartNodeId && EndNodeId == r.EndNodeId;

        //    return false;
        //}
        #endregion
    }
}
