using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace System.Runtime.Serialization
{
    internal sealed class SerializableAttribute : Attribute
    {
    }

    internal interface ISerializable
    {
        void GetObjectData(SerializationInfo info, StreamingContext context);
    }

    public class SerializationInfo
    {
        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.All,
        };

      
        [JsonProperty]
        private readonly Dictionary<string, Tuple<object, Type>> data = new Dictionary<string, Tuple<object, Type>>();
        
        [JsonProperty]
        private readonly Type type;

        internal SerializationInfo(Type type)
        {
            this.type = type;
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
            var ser = value as ISerializable;
            if (ser != null)
            {
                data[key] = Tuple.Create<object, Type>(ObjectToSerializationInfo(ser), value.GetType());
            }
            else
            {
                data[key] = Tuple.Create(value, type ?? (value != null ? value.GetType() : null));
            }
        }

        private static object CreateObjectFromSerializationInfo(SerializationInfo info)
        {
            // get the ctor
            var ctor = (from ci in info.type.GetTypeInfo()
                                      .DeclaredConstructors
                        let p = ci.GetParameters()
                        where p.Length == 2 && p[0].ParameterType == typeof(SerializationInfo) && p[1].ParameterType == typeof(StreamingContext)
                        select ci).First(); // this ctor must be present

            var obj = ctor.Invoke(new object[] { info, new StreamingContext() });
            return obj;
        }

        private static SerializationInfo ObjectToSerializationInfo(ISerializable ser)
        {
            var info = new SerializationInfo(ser.GetType());
            var ctx = new StreamingContext();
            ser.GetObjectData(info, ctx);

            return info;
        }


        internal static string ToJson(ISerializable @object)
        {
            if (@object == null) throw new ArgumentNullException("object");


            var info = ObjectToSerializationInfo(@object);

            return JsonConvert.SerializeObject(info, Formatting.Indented, SerializerSettings);
        }

        internal static object FromJson(string json)
        {
            var info = JsonConvert.DeserializeObject<SerializationInfo>(json, SerializerSettings);

            return CreateObjectFromSerializationInfo(info);
        }

    }
}
