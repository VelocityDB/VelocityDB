using System;
using VelocityDb.Indexing;
using VelocityDb.TypeInfo;

namespace RelSandbox
{
    public interface IArtifactPerson : IArtifactBase
    {
        string FirstName { get; set; }
        string SecondName { get; set; }
        string SurName { get; set; }
        string Gender { get; set; }
        DateTime BirthDate { get; set; }
    }

    class ArtifactPerson : ArtifactBase, IArtifactPerson
    {
        [Index]
        string _firstName;

        [FieldAccessor("_firstName")]
        public string FirstName
        {
            get
            {
                return _firstName;
            }
            set
            {
                Update(); _firstName = value;
            }
        }

        [Index]
        string _secondName;

        [FieldAccessor("_secondName")]
        public string SecondName
        {
            get
            {
                return _secondName;
            }
            set
            {
                Update(); _secondName = value;
            }
        }

        [Index]
        string _surName;

        [FieldAccessor("_surName")]
        public string SurName
        {
            get
            {
                return _surName;
            }
            set
            {
                Update(); _surName = value;
            }
        }

        string _gender;

        public string Gender
        {
            get
            {
                return _gender;
            }
            set
            {
                Update();
                _gender = value;
            }
        }

        DateTime _birthDate;

        public DateTime BirthDate
        {
            get
            {
                return _birthDate;
            }
            set
            {
                Update(); _birthDate = value;
            }
        }
    }
}
