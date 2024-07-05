using System;

namespace Xunit.v3;

// Used to tag data attributes that want to know the type they originated from
internal interface ITypeAwareDataAttribute
{
	Type? MemberType { get; set; }
}
