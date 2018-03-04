using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace VelocityDbSchema.NUnit
{
    [Serializable]
    public class ModelInfo
           : ISerializable
    {

        private List<BomTable> BomTablesWithOtherName = new List<BomTable>();

        public List<BomTable> BomTables
        {
            get { return BomTablesWithOtherName; }
            set
            {
                if (value != BomTablesWithOtherName)
                {
                    BomTablesWithOtherName = value;
                }
            }
        }


        public ModelInfo()
        {
            //BomTablesWithOtherName = new List<BomTable>();
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.SetType(typeof(ModelInfo));
            info.AddValue(nameof(BomTablesWithOtherName), BomTablesWithOtherName, typeof(List<BomTable>));
          ///   info.AddValue(nameof(BomTablesWithOtherName), BomTablesWithOtherName);
       }

        public ModelInfo(SerializationInfo info, StreamingContext context)
            : this()
        {
            
            BomTablesWithOtherName = (List<BomTable>)info.GetValue(nameof(BomTablesWithOtherName), typeof(List<BomTable>));
        }

    }
}

