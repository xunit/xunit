using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;


namespace Xunit.Serialization
{
    public class SerializationInfo
    {
      
      
      
        [JsonProperty]
        private IDictionary<string, Tuple<object, Type>> data = new Dictionary<string, Tuple<object, Type>>();
      

        [JsonProperty]
        private Type serializedType;

        public SerializationInfo()
        {
            
        }

        internal SerializationInfo(Type serializedType)
        {
            this.serializedType = serializedType;
        }

        public string GetString(string key)
        {
            Tuple<object, Type> val;
            if (data.TryGetValue(key, out val))
            {
                return (string)val.Item1;
            }
            return null;
        }

        internal object GetValue(string key, Type type)
        {
            Tuple<object, Type> val;
            if (data.TryGetValue(key, out val))
            {
                // check for serializable
                if (val.Item1 is SerializationInfo)
                {

                    return CreateObjectFromSerializationInfo((SerializationInfo)val.Item1);
                }
                else
                {
                    // rely on json.net
                    return val.Item1;
                }

            }

            if (type.GetTypeInfo()
                    .IsValueType)
                return Activator.CreateInstance(type);

            return null;
        }

        public void AddValue(string key, object value, Type type = null)
        {
            var ser = value as IGetTypeData;
            if (ser != null)
            {
                data[key] = Tuple.Create<object, Type>(ObjectToSerializationInfo(ser), value.GetType());
            }
            else
            {
                data[key] = Tuple.Create(value, type ?? (value != null ? value.GetType() : null));
            }
        }

        internal static object CreateObjectFromSerializationInfo(SerializationInfo info)
        {
            // get the ctor
            var ctor = (from ci in info.serializedType.GetTypeInfo()
                                       .DeclaredConstructors
                        let p = ci.GetParameters()
                        where p.Length == 2 && p[0].ParameterType == typeof(SerializationInfo) && p[1].ParameterType == typeof(StreamingContext)
                        select ci).First(); // this ctor must be present

            var obj = ctor.Invoke(new object[] { info, new StreamingContext() });
            return obj;
        }

        internal static SerializationInfo ObjectToSerializationInfo(IGetTypeData ser)
        {
            
            var ctx = new StreamingContext();

            var info = new SerializationInfo(ser.GetType());
            ser.GetData(info);
            return info;
        }


   

    }
}