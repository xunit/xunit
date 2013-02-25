using System;

namespace Xunit.Sdk
{
    /// <summary>
    /// This class exists to have a base class for attributes that don't run afoul of
    /// the brain-dead caching algorithm in the CLR's attribute discovery system.
    /// Thanks, CLR team.
    /// </summary>
    public abstract class AttributeBase : Attribute
    {
        object typeId = new object();

        /// <inheritdoc/>
        public override object TypeId
        {
            get { return typeId; }
        }
    }
}