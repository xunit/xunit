using System;

namespace Xunit
{
    public partial class Assert
    {
        /// <summary/>
        internal static void GuardArgumentNotNull(string argName, object argValue)
        {
            if (argValue == null)
                throw new ArgumentNullException(argName);
        }
    }
}
