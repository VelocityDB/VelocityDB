using VelocityDb;
using VelocityDb.Indexing;
using VelocityDb.TypeInfo;

namespace RelSandbox
{
    public class Node : OptimizedPersistable
    {
        #region What property
        [Index]
        string _what;

        [FieldAccessor("_what")]
        public string What
        {
            get
            {
                return _what;
            }
            set
            {
                Update();
                _what = value;
            }
        }
        #endregion

        #region ArtifactUid property
        [Index]
        ulong _artifactUid;

        [FieldAccessor("_artifactUid")]
        public ulong ArtifactUid
        {
            get
            {
                return _artifactUid;
            }
            set
            {
                Update();
                _artifactUid = value;
            }
        }
        #endregion

        #region Object overrides
        //public override int GetHashCode()
        //{
        //    return What.GetHashCode() ^ ArtifactUid.GetHashCode();
        //}

        //public override bool Equals(object obj)
        //{
        //    if (obj is Node n)
        //        return What == n.What && ArtifactUid == n.ArtifactUid;

        //    return false;
        //}

        //public override string ToString()
        //{
        //    return $"{What} {base.ToString()}";
        //}
        #endregion
    }
}