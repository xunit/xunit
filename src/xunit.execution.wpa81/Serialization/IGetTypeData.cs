using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Runtime.Serialization
{
    internal interface IGetTypeData
    {
        void GetData(Xunit.Serialization.SerializationInfo data);
    }
}
