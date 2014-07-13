using System;

namespace Xunit
{
    internal sealed class SerializableAttribute : Attribute { }

    internal interface ISerializable { }

    /// <summary/>
    public class SerializationInfo
    {
        /// <summary/>
        public void AddValue(string x, object y, Type z = null)
        {
            throw new NotImplementedException();
        }

        /// <summary/>
        public string GetString(string x)
        {
            throw new NotImplementedException();
        }

        /// <summary/>
        public object GetValue(string x, Type y)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary/>
    public class StreamingContext { }
}