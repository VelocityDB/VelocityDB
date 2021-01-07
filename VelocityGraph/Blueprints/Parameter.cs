namespace Frontenac.Blueprints
{
    public class Parameter
    {
        public Parameter(object key, object value)
        {
            Key = key;
            Value = value;
        }

        public object Key { get; protected set; }

        public object Value { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is Parameter)
            {
                var param = obj as Parameter;
                var otherKey = param.Key;
                var otherValue = param.Value;
                if (otherKey == null)
                {
                    if (Key != null)
                        return false;
                }
                else
                {
                    if (!otherKey.Equals(Key))
                        return false;
                }

                if (otherValue == null)
                {
                    if (Value != null)
                        return false;
                }
                else
                {
                    if (!otherValue.Equals(Value))
                        return false;
                }

                return true;
            }

            return false;
        }

        public override int GetHashCode()
        {
            const int prime = 31;
            var result = 1;
// ReSharper disable NonReadonlyFieldInGetHashCode
            result = prime*result + ((Key == null) ? 0 : Key.GetHashCode());
            result = prime*result + ((Value == null) ? 0 : Value.GetHashCode());
// ReSharper restore NonReadonlyFieldInGetHashCode
            return result;
        }

        public override string ToString()
        {
            return string.Format("parameter[{0},{1}]", Key, Value);
        }
    }

    public class Parameter<TK, TV> : Parameter
    {
        public Parameter(TK key, TV value)
            : base(key, value)
        {
        }

        public new TK Key
        {
            get { return (TK) base.Key; }
        }

        public new TV Value
        {
            get { return (TV) base.Value; }
            set { base.Value = value; }
        }
    }
}