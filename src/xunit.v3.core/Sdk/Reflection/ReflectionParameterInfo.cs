using System.Reflection;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Reflection-based implementation of <see cref="IReflectionParameterInfo"/>.
    /// </summary>
    public class ReflectionParameterInfo : LongLivedMarshalByRefObject, IReflectionParameterInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReflectionParameterInfo"/> class.
        /// </summary>
        /// <param name="parameterInfo">The parameter to be wrapped.</param>
        public ReflectionParameterInfo(ParameterInfo parameterInfo)
        {
            ParameterInfo = parameterInfo;
        }

        /// <inheritdoc/>
        public string Name
        {
            get { return ParameterInfo.Name; }
        }

        /// <inheritdoc/>
        public ParameterInfo ParameterInfo { get; private set; }

        /// <inheritdoc/>
        public ITypeInfo ParameterType
        {
            get { return Reflector.Wrap(ParameterInfo.ParameterType); }
        }
    }
}
