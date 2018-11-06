using System;

namespace ExampleServiceNov2018.Domain
{
    /// <summary>
    /// Uniquely identifies an aggregate
    /// Max length of 100 chars, always lower-case
    /// </summary>
    public class AggragateId
    {
        private readonly string _value;
        
        public AggragateId(string s)
        {
            if(s == null)
                throw new ArgumentNullException(nameof(s), "AggregateId cannot be null");
            
            if(s.Length > 100)
                throw new ArgumentException("AggregateId cannot be longer than 100 chars");
            _value = s.ToLowerInvariant();
        }
        
        public static implicit operator string(AggragateId x)
            => x._value;
        public static implicit operator AggragateId(string x)
            => new AggragateId(x);

        protected bool Equals(AggragateId other)
        {
            return string.Equals(_value, other._value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((AggragateId) obj);
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }
    }
}